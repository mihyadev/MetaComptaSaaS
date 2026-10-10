using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class DomiciliationService : IDomiciliationService
{
    private readonly MetaComptaDbContext _db;
    private readonly INotificationService _notifications;

    private static readonly HashSet<string> OpenCourrierStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Received", "ToProcess", "Assigned", "InProgress", "Completed", "ClientNotified"
    };

    private static readonly HashSet<string> RejectionCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Expired", "WrongDocument", "MissingPage", "Unreadable", "InformationMismatch", "Other"
    };

    public DomiciliationService(MetaComptaDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<DomiciliationDashboardRowDto>>> GetDashboardAsync(int cabinetId)
    {
        var now = DateTime.UtcNow;
        // Project only columns present on all environments (avoid ResponsableId if DB lagging).
        var services = await _db.ServiceContrats
            .AsNoTracking()
            .Where(s => s.Client!.CabinetId == cabinetId
                && s.Actif
                && s.TypeService == RefCodes.TypeService.Domiciliation
                && !s.IsDeleted
                && !s.Client.IsDeleted)
            .Select(s => new
            {
                s.ClientId,
                s.Actif,
                s.DateDebut,
                s.DateFin,
                RaisonSociale = s.Client!.RaisonSociale
            })
            .ToListAsync();

        var clientIds = services.Select(s => s.ClientId).Distinct().ToList();
        var courriers = await _db.CourriersDomiciliation
            .Where(c => c.CabinetId == cabinetId && clientIds.Contains(c.ClientId) && !c.IsDeleted)
            .ToListAsync();
        var docReqs = await _db.DemandesDocumentDomiciliation
            .Where(d => d.CabinetId == cabinetId && clientIds.Contains(d.ClientId) && !d.IsDeleted)
            .ToListAsync();
        var timeline = await _db.DomiciliationTimelineEvents
            .Where(t => t.CabinetId == cabinetId && clientIds.Contains(t.ClientId))
            .GroupBy(t => t.ClientId)
            .Select(g => new { ClientId = g.Key, Last = g.Max(x => x.At) })
            .ToListAsync();

        var rows = services.Select(s =>
        {
            var cMails = courriers.Where(c => c.ClientId == s.ClientId).ToList();
            var dDocs = docReqs.Where(d => d.ClientId == s.ClientId).ToList();
            var pendingMail = cMails.Count(c => OpenCourrierStatuses.Contains(c.Statut) && c.Statut != "Archived" && c.Statut != "Closed");
            var pendingDocs = dDocs.Count(d => d.Statut is not ("Validated" or "Closed"));
            var overdue = cMails.Count(c => c.DateEcheance.HasValue && c.DateEcheance < now && OpenCourrierStatuses.Contains(c.Statut))
                + dDocs.Count(d => d.DateEcheance.HasValue && d.DateEcheance < now && d.Statut is not ("Validated" or "Closed"));
            var rejectedOpen = dDocs.Count(d => d.Statut is "Rejected" or "WaitingClient");
            var docsStatut = rejectedOpen > 0 ? "À corriger" : pendingDocs > 0 ? "En attente" : "OK";
            var last = timeline.FirstOrDefault(t => t.ClientId == s.ClientId)?.Last
                ?? cMails.Select(c => (DateTime?)c.UpdatedAt ?? c.CreatedAt).DefaultIfEmpty().Max();
            var actifDate = s.Actif && s.DateDebut <= now && (s.DateFin == null || s.DateFin >= now);

            return new DomiciliationDashboardRowDto
            {
                ClientId = s.ClientId,
                RaisonSociale = s.RaisonSociale ?? "",
                DomiciliationStatut = actifDate ? "Active" : "Inactive",
                ContratStatut = s.Actif ? "Actif" : "Inactif",
                DocumentsStatut = docsStatut,
                IncomingMailCount = cMails.Count,
                PendingActions = pendingMail + pendingDocs,
                OverdueActions = overdue,
                LastActivity = last,
                ResponsableUserId = null,
                ResponsableNom = null
            };
        }).OrderBy(r => r.RaisonSociale).ToList();

        return ApiResponse<List<DomiciliationDashboardRowDto>>.Ok(rows);
    }

    public async Task<ApiResponse<DomiciliationClientWorkspaceDto>> GetClientWorkspaceAsync(int clientId, int cabinetId)
    {
        if (!await IsDomiciledAsync(clientId, cabinetId))
            return ApiResponse<DomiciliationClientWorkspaceDto>.Fail("Ce client n'a pas de service de domiciliation actif.");

        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DomiciliationClientWorkspaceDto>.Fail("Client introuvable.");

        var courriers = await QueryCourrier(cabinetId)
            .Where(c => c.ClientId == clientId && !c.IsDeleted)
            .OrderByDescending(c => c.DateReception)
            .Take(100)
            .ToListAsync();

        var demandes = await _db.DemandesDocumentDomiciliation
            .Include(d => d.Responsable)
            .Include(d => d.CurrentDocument)
            .Where(d => d.ClientId == clientId && d.CabinetId == cabinetId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(50)
            .ToListAsync();

        var timeline = await GetTimelineAsync(clientId, cabinetId, 40);
        var now = DateTime.UtcNow;

        return ApiResponse<DomiciliationClientWorkspaceDto>.Ok(new DomiciliationClientWorkspaceDto
        {
            ClientId = clientId,
            RaisonSociale = client.RaisonSociale,
            Courriers = courriers.Select(MapCourrier).ToList(),
            DemandesDocuments = demandes.Select(d => MapDemandeDoc(d, null)).ToList(),
            Timeline = timeline.Data ?? new(),
            PendingActions = courriers.Count(c => OpenCourrierStatuses.Contains(c.Statut) && c.Statut is not ("Archived" or "Closed"))
                + demandes.Count(d => d.Statut is not ("Validated" or "Closed")),
            OverdueActions = courriers.Count(c => c.DateEcheance < now && OpenCourrierStatuses.Contains(c.Statut))
                + demandes.Count(d => d.DateEcheance < now && d.Statut is not ("Validated" or "Closed"))
        });
    }

    public async Task<ApiResponse<List<DomiciliationTimelineDto>>> GetTimelineAsync(int clientId, int cabinetId, int take = 50)
    {
        var items = await _db.DomiciliationTimelineEvents
            .AsNoTracking()
            .Where(t => t.ClientId == clientId && t.CabinetId == cabinetId)
            .OrderByDescending(t => t.At)
            .Take(take)
            .Select(t => new DomiciliationTimelineDto
            {
                Id = t.Id,
                ClientId = t.ClientId,
                RelatedType = t.RelatedType,
                RelatedId = t.RelatedId,
                Action = t.Action,
                PreviousStatus = t.PreviousStatus,
                NewStatus = t.NewStatus,
                UserName = t.UserName,
                Note = t.Note,
                DocumentId = t.DocumentId,
                At = t.At
            })
            .ToListAsync();
        return ApiResponse<List<DomiciliationTimelineDto>>.Ok(items);
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> ReceiveCourrierAsync(
        CourrierReceiveDto dto, int cabinetId, string userId, string userName)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<CourrierDomiciliationDto>.Fail("Le client est obligatoire.");
        if (!await IsDomiciledAsync(dto.ClientId, cabinetId))
            return ApiResponse<CourrierDomiciliationDto>.Fail("Ce client n'a pas de service de domiciliation actif.");

        if (dto.ActionRequise && string.IsNullOrWhiteSpace(dto.ResponsableUserId))
            return ApiResponse<CourrierDomiciliationDto>.Fail("Un responsable est obligatoire lorsque une action est requise.");

        var statut = dto.ActionRequise ? "ToProcess" : "Received";
        var entity = new CourrierDomiciliation
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            DateReception = dto.DateReception ?? DateTime.UtcNow,
            Expediteur = string.IsNullOrWhiteSpace(dto.Expediteur) ? null : dto.Expediteur.Trim(),
            TypeCourrier = string.IsNullOrWhiteSpace(dto.TypeCourrier) ? "Courrier" : dto.TypeCourrier.Trim(),
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
            Confidentialite = string.IsNullOrWhiteSpace(dto.Confidentialite) ? "Normal" : dto.Confidentialite.Trim(),
            ScanDocumentId = dto.ScanDocumentId,
            ActionRequise = dto.ActionRequise,
            ResponsableUserId = string.IsNullOrWhiteSpace(dto.ResponsableUserId) ? null : dto.ResponsableUserId.Trim(),
            DateEcheance = dto.DateEcheance,
            Statut = statut,
            Notes = dto.Notes,
            CreatedBy = userId
        };

        _db.CourriersDomiciliation.Add(entity);
        await _db.SaveChangesAsync();

        if (dto.ScanDocumentId.HasValue)
            await AddPieceInternalAsync(entity, dto.ScanDocumentId.Value, cabinetId, userId, makePrimary: true);

        if (entity.ActionRequise)
            await UpsertTaskForCourrierAsync(entity, cabinetId, userId, "AFaire");

        await AddTimelineAsync(cabinetId, entity.ClientId, "Courrier", entity.Id, "Received", null, statut, userId, userName,
            $"Courrier enregistré ({entity.TypeCourrier})", entity.ScanDocumentId);

        return ApiResponse<CourrierDomiciliationDto>.Ok(await LoadCourrierMappedAsync(entity.Id, cabinetId), "Courrier enregistré.");
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> GetCourrierAsync(int id, int cabinetId)
    {
        var entity = await QueryCourrier(cabinetId)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (entity == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Courrier introuvable.");
        return ApiResponse<CourrierDomiciliationDto>.Ok(MapCourrier(entity));
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> AttachScanAsync(
        int id, int documentId, int cabinetId, string userId, string userName)
    {
        var entity = await QueryCourrier(cabinetId)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (entity == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Courrier introuvable.");

        var add = await AddPieceInternalAsync(entity, documentId, cabinetId, userId, makePrimary: !entity.Pieces.Any(p => !p.IsDeleted));
        if (!add.Success) return ApiResponse<CourrierDomiciliationDto>.Fail(add.Message ?? "Ajout impossible.");

        await AddTimelineAsync(cabinetId, entity.ClientId, "Courrier", entity.Id, "AttachScan", entity.Statut, entity.Statut,
            userId, userName, add.Message, documentId);
        await _db.SaveChangesAsync();

        return ApiResponse<CourrierDomiciliationDto>.Ok(await LoadCourrierMappedAsync(id, cabinetId), "Document ajouté à la photothèque.");
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> RemovePieceAsync(
        int id, int pieceId, int cabinetId, string userId, string userName)
    {
        var entity = await QueryCourrier(cabinetId)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (entity == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Courrier introuvable.");

        var piece = entity.Pieces.FirstOrDefault(p => p.Id == pieceId && !p.IsDeleted);
        if (piece == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Pièce introuvable.");

        piece.IsDeleted = true;
        piece.DeletedAt = DateTime.UtcNow;
        piece.DeletedBy = userId;
        piece.UpdatedAt = DateTime.UtcNow;
        piece.UpdatedBy = userId;

        var remaining = entity.Pieces.Where(p => !p.IsDeleted && p.Id != pieceId).OrderBy(p => p.SortOrder).ToList();
        if (piece.IsPrimary || entity.ScanDocumentId == piece.DocumentId)
        {
            var next = remaining.FirstOrDefault();
            foreach (var p in remaining) p.IsPrimary = false;
            if (next != null)
            {
                next.IsPrimary = true;
                entity.ScanDocumentId = next.DocumentId;
            }
            else
            {
                entity.ScanDocumentId = null;
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await AddTimelineAsync(cabinetId, entity.ClientId, "Courrier", entity.Id, "RemovePiece", entity.Statut, entity.Statut,
            userId, userName, $"Pièce retirée #{piece.DocumentId}", piece.DocumentId);
        await _db.SaveChangesAsync();
        return ApiResponse<CourrierDomiciliationDto>.Ok(await LoadCourrierMappedAsync(id, cabinetId), "Document retiré.");
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> SetPrimaryPieceAsync(
        int id, int pieceId, int cabinetId, string userId, string userName)
    {
        var entity = await QueryCourrier(cabinetId)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (entity == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Courrier introuvable.");

        var piece = entity.Pieces.FirstOrDefault(p => p.Id == pieceId && !p.IsDeleted);
        if (piece == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Pièce introuvable.");

        foreach (var p in entity.Pieces.Where(x => !x.IsDeleted))
            p.IsPrimary = p.Id == pieceId;
        entity.ScanDocumentId = piece.DocumentId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<CourrierDomiciliationDto>.Ok(await LoadCourrierMappedAsync(id, cabinetId), "Document principal mis à jour.");
    }

    public async Task<ApiResponse<CourrierDomiciliationDto>> AdvanceCourrierAsync(
        int id, CourrierStatusActionDto dto, int cabinetId, string userId, string userName)
    {
        var entity = await QueryCourrier(cabinetId)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (entity == null) return ApiResponse<CourrierDomiciliationDto>.Fail("Courrier introuvable.");

        var action = (dto.Action ?? "").Trim().ToLowerInvariant();
        var prev = entity.Statut;
        string? next = null;
        string note = dto.Notes ?? "";

        switch (action)
        {
            case "classify":
                if (dto.ActionRequise.HasValue) entity.ActionRequise = dto.ActionRequise.Value;
                next = entity.ActionRequise ? "ToProcess" : "Received";
                if (entity.ActionRequise && !string.IsNullOrWhiteSpace(dto.ResponsableUserId))
                    entity.ResponsableUserId = dto.ResponsableUserId;
                if (dto.DateEcheance.HasValue) entity.DateEcheance = dto.DateEcheance;
                break;
            case "assign":
                if (string.IsNullOrWhiteSpace(dto.ResponsableUserId))
                    return ApiResponse<CourrierDomiciliationDto>.Fail("Le responsable est obligatoire.");
                entity.ResponsableUserId = dto.ResponsableUserId;
                if (dto.DateEcheance.HasValue) entity.DateEcheance = dto.DateEcheance;
                next = "Assigned";
                break;
            case "start":
                next = "InProgress";
                break;
            case "complete":
                next = "Completed";
                break;
            case "notify":
                entity.ClientNotifie = true;
                entity.ClientNotifieAt = DateTime.UtcNow;
                entity.PartageDocumentClient = dto.PartageDocumentClient;
                if (dto.PartageDocumentClient)
                {
                    var docIds = entity.Pieces.Where(p => !p.IsDeleted).Select(p => p.DocumentId).ToList();
                    if (entity.ScanDocumentId.HasValue && !docIds.Contains(entity.ScanDocumentId.Value))
                        docIds.Add(entity.ScanDocumentId.Value);
                    if (docIds.Count > 0)
                    {
                        var docs = await _db.Documents.Where(d => docIds.Contains(d.Id) && d.CabinetId == cabinetId).ToListAsync();
                        foreach (var doc in docs) doc.PartageExtranet = true;
                    }
                }
                await NotifyClientUsersAsync(entity.ClientId, cabinetId,
                    "Nouvelle correspondance",
                    "Une nouvelle correspondance a été reçue pour votre société.",
                    $"/Domiciliation/Courrier/{entity.Id}");
                next = "ClientNotified";
                break;
            case "archive":
                next = "Archived";
                break;
            case "close":
                next = "Closed";
                break;
            default:
                return ApiResponse<CourrierDomiciliationDto>.Fail("Action non reconnue.");
        }

        entity.Statut = next!;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        if (!string.IsNullOrWhiteSpace(dto.Notes)) entity.Notes = dto.Notes;

        var taskStatut = next switch
        {
            "ToProcess" or "Assigned" => "AFaire",
            "InProgress" => "EnCours",
            "Completed" or "ClientNotified" => "EnAttenteRevision",
            "Archived" or "Closed" => "Termine",
            _ => "AFaire"
        };
        if (entity.ActionRequise || entity.LinkedTaskItemId.HasValue)
            await UpsertTaskForCourrierAsync(entity, cabinetId, userId, taskStatut);

        await AddTimelineAsync(cabinetId, entity.ClientId, "Courrier", entity.Id, action, prev, next, userId, userName, note, entity.ScanDocumentId);
        await _db.SaveChangesAsync();

        return ApiResponse<CourrierDomiciliationDto>.Ok(await LoadCourrierMappedAsync(id, cabinetId), "Statut mis à jour.");
    }

    public async Task<ApiResponse<DemandeActionCourrierDto>> CreateClientActionAsync(
        int courrierId, DemandeActionCreateDto dto, int cabinetId, string clientUserId, int clientId)
    {
        var courrier = await _db.CourriersDomiciliation
            .FirstOrDefaultAsync(c => c.Id == courrierId && c.CabinetId == cabinetId && c.ClientId == clientId && !c.IsDeleted);
        if (courrier == null) return ApiResponse<DemandeActionCourrierDto>.Fail("Courrier introuvable.");
        if (!courrier.ClientNotifie)
            return ApiResponse<DemandeActionCourrierDto>.Fail("Cette correspondance n'est pas encore notifiée.");

        var type = (dto.TypeAction ?? "").Trim();
        if (type is not ("Pickup" or "Forward" or "View" or "Archive"))
            return ApiResponse<DemandeActionCourrierDto>.Fail("Type d'action invalide.");
        if (type == "Forward" && string.IsNullOrWhiteSpace(dto.AdresseForward))
            return ApiResponse<DemandeActionCourrierDto>.Fail("L'adresse de réexpédition est obligatoire.");
        if (type == "View" && !courrier.PartageDocumentClient)
            return ApiResponse<DemandeActionCourrierDto>.Fail("La consultation du document n'est pas autorisée.");

        var statut = type switch
        {
            "Pickup" => "PickupRequested",
            "Forward" => "ForwardRequested",
            "View" => "Closed",
            "Archive" => "Closed",
            _ => "PickupRequested"
        };

        var action = new DemandeActionCourrier
        {
            CabinetId = cabinetId,
            CourrierId = courrierId,
            TypeAction = type,
            AdresseForward = dto.AdresseForward?.Trim(),
            Notes = dto.Notes,
            ClientUserId = clientUserId,
            Statut = statut,
            ConfirmedAt = type is "View" or "Archive" ? DateTime.UtcNow : null,
            CreatedBy = clientUserId
        };
        _db.DemandesActionCourrier.Add(action);
        await _db.SaveChangesAsync();

        var typeLabel = type switch
        {
            "Pickup" => "retrait au cabinet",
            "Forward" => "réexpédition",
            "View" => "consultation",
            "Archive" => "archivage",
            _ => type
        };

        if (type is "Pickup" or "Forward")
        {
            var assignee = courrier.ResponsableUserId
                ?? await _db.Users.AsNoTracking()
                    .Where(u => u.CabinetId == cabinetId && u.ClientId == null && u.IsActive)
                    .OrderBy(u => u.Id)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

            var task = new TaskItem
            {
                CabinetId = cabinetId,
                ClientId = clientId,
                Titre = type == "Pickup" ? $"Retrait courrier #{courrierId}" : $"Réexpédition courrier #{courrierId}",
                Description = string.IsNullOrWhiteSpace(dto.Notes)
                    ? $"Le client a demandé un {typeLabel}."
                    : dto.Notes,
                AssignedToUserId = assignee,
                AssignedByUserId = clientUserId,
                DateEcheance = DateTime.UtcNow.AddDays(3),
                Priorite = "Haute",
                Statut = "AFaire",
                RelatedEntityType = "DemandeActionCourrier",
                RelatedEntityId = action.Id,
                TaskType = type,
                CreatedBy = clientUserId
            };
            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();
            action.LinkedTaskItemId = task.Id;
            await _db.SaveChangesAsync();

            await NotifyStaffUsersAsync(cabinetId, courrier.ResponsableUserId,
                "Demande client — courrier",
                $"Le client demande un {typeLabel} pour le courrier #{courrierId}.",
                $"/Domiciliation/Courrier/{courrierId}");
        }
        else
        {
            await NotifyStaffUsersAsync(cabinetId, courrier.ResponsableUserId,
                "Action client — courrier",
                $"Le client a choisi « {typeLabel} » pour le courrier #{courrierId}.",
                $"/Domiciliation/Courrier/{courrierId}");
        }

        await AddTimelineAsync(cabinetId, clientId, "DemandeAction", action.Id, "ClientRequest", null, statut, clientUserId, "Client",
            $"Demande client: {type}", courrier.ScanDocumentId);

        return ApiResponse<DemandeActionCourrierDto>.Ok(MapAction(action), "Demande enregistrée.");
    }

    public async Task<ApiResponse<DemandeActionCourrierDto>> AdvanceClientActionAsync(
        int actionId, DemandeActionAdvanceDto dto, int cabinetId, string userId, string userName)
    {
        var action = await _db.DemandesActionCourrier
            .Include(a => a.Courrier)
            .FirstOrDefaultAsync(a => a.Id == actionId && a.CabinetId == cabinetId && !a.IsDeleted);
        if (action?.Courrier == null) return ApiResponse<DemandeActionCourrierDto>.Fail("Demande introuvable.");

        var act = (dto.Action ?? "").Trim().ToLowerInvariant();
        var prev = action.Statut;
        string? next = null;

        // Simple path: one staff click "done" finishes Pickup/Forward.
        // Legacy multi-step actions still accepted for old data.
        if (act is "done" or "complete" or "treat")
        {
            // One-click finish for staff.
            if (action.TypeAction == "Pickup")
            {
                next = "Closed";
                action.IdentityCheckDone = true;
                action.ConfirmedAt = DateTime.UtcNow;
            }
            else if (action.TypeAction == "Forward")
            {
                if (!string.IsNullOrWhiteSpace(dto.AdresseForward)) action.AdresseForward = dto.AdresseForward.Trim();
                if (!string.IsNullOrWhiteSpace(dto.ShippingReference)) action.ShippingReference = dto.ShippingReference.Trim();
                next = "Closed";
                action.ConfirmedAt = DateTime.UtcNow;
            }
        }
        else if (action.TypeAction == "Pickup")
        {
            next = act switch
            {
                "prepare" => "Preparing",
                "ready" => "ReadyForPickup",
                "handover" => "HandedOver",
                "close" => "Closed",
                _ => null
            };
            if (act == "handover")
            {
                if (!dto.IdentityCheckDone && !action.IdentityCheckDone)
                    return ApiResponse<DemandeActionCourrierDto>.Fail("La vérification d'identité est obligatoire avant remise.");
                action.IdentityCheckDone = true;
                action.ConfirmedAt = DateTime.UtcNow;
            }
        }
        else if (action.TypeAction == "Forward")
        {
            next = act switch
            {
                "confirm_address" => "AddressConfirmed",
                "prepare" => "PreparingShipment",
                "sent" => "Sent",
                "delivered" => "Delivered",
                "close" => "Closed",
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(dto.AdresseForward)) action.AdresseForward = dto.AdresseForward.Trim();
            if (!string.IsNullOrWhiteSpace(dto.ShippingReference)) action.ShippingReference = dto.ShippingReference.Trim();
            if (act == "sent" && string.IsNullOrWhiteSpace(action.ShippingReference))
                return ApiResponse<DemandeActionCourrierDto>.Fail("La référence d'expédition est obligatoire.");
        }

        if (next == null) return ApiResponse<DemandeActionCourrierDto>.Fail("Action non valide pour cette demande.");

        action.Statut = next;
        action.StaffUserId = userId;
        action.UpdatedAt = DateTime.UtcNow;
        action.UpdatedBy = userId;
        if (!string.IsNullOrWhiteSpace(dto.Notes)) action.Notes = dto.Notes;

        if (action.LinkedTaskItemId.HasValue)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == action.LinkedTaskItemId);
            if (task != null)
            {
                task.Statut = next is "Closed" or "Delivered" or "HandedOver" ? "Termine" : "EnCours";
                task.UpdatedAt = DateTime.UtcNow;
            }
        }

        await AddTimelineAsync(cabinetId, action.Courrier.ClientId, "DemandeAction", action.Id, act, prev, next, userId, userName, dto.Notes, action.Courrier.ScanDocumentId);

        if (next is "ReadyForPickup" or "HandedOver" or "Sent" or "Delivered" or "Closed")
        {
            var msg = next switch
            {
                "ReadyForPickup" => "Votre courrier est prêt pour retrait au cabinet.",
                "HandedOver" => "Votre courrier a été remis.",
                "Sent" => "Votre courrier a été expédié.",
                "Delivered" => "Votre courrier a été livré.",
                "Closed" => "Votre demande courrier est clôturée.",
                _ => "Mise à jour de votre demande courrier."
            };
            await NotifyClientUsersAsync(action.Courrier.ClientId, cabinetId,
                "Mise à jour courrier",
                msg,
                $"/Domiciliation/Courrier/{action.CourrierId}");
        }

        await _db.SaveChangesAsync();
        return ApiResponse<DemandeActionCourrierDto>.Ok(MapAction(action), "Demande mise à jour.");
    }

    public async Task<ApiResponse<DemandeDocumentDomiciliationDto>> CreateDocumentRequestAsync(
        DemandeDocumentCreateDto dto, int cabinetId, string userId, string userName)
    {
        if (dto.ClientId <= 0) return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Le client est obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.DocumentType))
            return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Le type de document est obligatoire.");
        if (!await IsDomiciledAsync(dto.ClientId, cabinetId))
            return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Ce client n'a pas de service de domiciliation actif.");

        var entity = new DemandeDocumentDomiciliation
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            DocumentType = dto.DocumentType.Trim(),
            Message = dto.Message?.Trim(),
            ResponsableUserId = string.IsNullOrWhiteSpace(dto.ResponsableUserId) ? userId : dto.ResponsableUserId.Trim(),
            DateEcheance = dto.DateEcheance,
            Statut = "Requested",
            CreatedBy = userId
        };
        _db.DemandesDocumentDomiciliation.Add(entity);
        await _db.SaveChangesAsync();

        var task = new TaskItem
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            Titre = $"Demande document: {entity.DocumentType}",
            Description = entity.Message,
            AssignedToUserId = entity.ResponsableUserId,
            AssignedByUserId = userId,
            DateEcheance = entity.DateEcheance ?? DateTime.UtcNow.AddDays(7),
            Priorite = "Moyenne",
            Statut = "AFaire",
            RelatedEntityType = "DemandeDocumentDomiciliation",
            RelatedEntityId = entity.Id,
            TaskType = "DocumentRequest",
            CreatedBy = userId
        };
        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();
        entity.LinkedTaskItemId = task.Id;

        await NotifyClientUsersAsync(dto.ClientId, cabinetId,
            "Document demandé",
            $"Veuillez fournir le document: {entity.DocumentType}.",
            $"/Domiciliation/Documents/{entity.Id}");
        entity.Statut = "ClientNotified";

        await AddTimelineAsync(cabinetId, dto.ClientId, "DemandeDocument", entity.Id, "Request", "Requested", "ClientNotified", userId, userName, entity.Message, null);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(d => d.Responsable).LoadAsync();
        return ApiResponse<DemandeDocumentDomiciliationDto>.Ok(MapDemandeDoc(entity, null), "Demande de document créée.");
    }

    public async Task<ApiResponse<DemandeDocumentDomiciliationDto>> AttachDocumentVersionAsync(
        int demandeId, int documentId, int cabinetId, string userId, string userName, bool fromClient)
    {
        var demande = await _db.DemandesDocumentDomiciliation
            .Include(d => d.Responsable)
            .FirstOrDefaultAsync(d => d.Id == demandeId && d.CabinetId == cabinetId && !d.IsDeleted);
        if (demande == null) return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Demande introuvable.");

        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.CabinetId == cabinetId && !d.IsDeleted);
        if (doc == null) return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Document introuvable.");

        if (demande.RootDocumentId == null)
        {
            demande.RootDocumentId = doc.Id;
            doc.Version = 1;
            doc.ParentDocumentId = null;
        }
        else
        {
            var maxVer = await _db.Documents
                .Where(d => d.Id == demande.RootDocumentId || d.ParentDocumentId == demande.RootDocumentId)
                .MaxAsync(d => (int?)d.Version) ?? 1;
            doc.ParentDocumentId = demande.RootDocumentId;
            doc.Version = maxVer + 1;
        }

        doc.ClientId = demande.ClientId;
        doc.Categorie = "Domiciliation";
        doc.Titre = string.IsNullOrWhiteSpace(doc.Titre) ? demande.DocumentType : doc.Titre;
        demande.CurrentDocumentId = doc.Id;
        var prev = demande.Statut;
        demande.Statut = "UnderReview";
        demande.RejectionReasonCode = null;
        demande.RejectionReasonText = null;
        demande.UpdatedAt = DateTime.UtcNow;

        if (demande.LinkedTaskItemId.HasValue)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == demande.LinkedTaskItemId);
            if (task != null) { task.Statut = "EnCours"; task.UpdatedAt = DateTime.UtcNow; }
        }

        await AddTimelineAsync(cabinetId, demande.ClientId, "DemandeDocument", demande.Id, fromClient ? "ClientUpload" : "StaffUpload",
            prev, "UnderReview", userId, userName, $"Version {doc.Version}", doc.Id);
        await _db.SaveChangesAsync();

        var versions = await LoadVersionsAsync(demande.RootDocumentId!.Value, cabinetId);
        return ApiResponse<DemandeDocumentDomiciliationDto>.Ok(MapDemandeDoc(demande, versions), "Document reçu pour revue.");
    }

    public async Task<ApiResponse<DemandeDocumentDomiciliationDto>> ReviewDocumentRequestAsync(
        int id, DemandeDocumentReviewDto dto, int cabinetId, string userId, string userName)
    {
        var demande = await _db.DemandesDocumentDomiciliation
            .Include(d => d.Responsable)
            .Include(d => d.CurrentDocument)
            .FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId && !d.IsDeleted);
        if (demande == null) return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Demande introuvable.");
        if (demande.Statut != "UnderReview" && demande.Statut != "Uploaded")
            return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Cette demande n'est pas en revue.");

        var prev = demande.Statut;
        if (dto.Approve)
        {
            demande.Statut = "Validated";
            if (demande.CurrentDocument != null)
            {
                demande.CurrentDocument.Statut = "Valide";
                demande.CurrentDocument.MotifRejet = null;
            }
            if (demande.LinkedTaskItemId.HasValue)
            {
                var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == demande.LinkedTaskItemId);
                if (task != null) task.Statut = "Termine";
            }
            await NotifyClientUsersAsync(demande.ClientId, cabinetId, "Document validé",
                $"Votre document « {demande.DocumentType} » a été validé.", $"/Domiciliation/Documents/{demande.Id}");
            await AddTimelineAsync(cabinetId, demande.ClientId, "DemandeDocument", demande.Id, "Validated", prev, "Validated", userId, userName, dto.Notes, demande.CurrentDocumentId);
            demande.Statut = "Closed";
            await AddTimelineAsync(cabinetId, demande.ClientId, "DemandeDocument", demande.Id, "Closed", "Validated", "Closed", userId, userName, null, demande.CurrentDocumentId);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReasonCode) || !RejectionCodes.Contains(dto.RejectionReasonCode))
                return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Le motif de rejet est obligatoire.");
            if (string.Equals(dto.RejectionReasonCode, "Other", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(dto.RejectionReasonText))
                return ApiResponse<DemandeDocumentDomiciliationDto>.Fail("Précisez le motif de rejet (Autre).");

            demande.Statut = "Rejected";
            demande.RejectionReasonCode = dto.RejectionReasonCode;
            demande.RejectionReasonText = dto.RejectionReasonText?.Trim();
            if (demande.CurrentDocument != null)
            {
                demande.CurrentDocument.Statut = "Rejete";
                demande.CurrentDocument.MotifRejet = $"{demande.RejectionReasonCode}: {demande.RejectionReasonText}".Trim(' ', ':');
            }
            await AddTimelineAsync(cabinetId, demande.ClientId, "DemandeDocument", demande.Id, "Rejected", prev, "Rejected", userId, userName,
                $"{demande.RejectionReasonCode} — {demande.RejectionReasonText}", demande.CurrentDocumentId);
            demande.Statut = "WaitingClient";
            if (demande.LinkedTaskItemId.HasValue)
            {
                var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == demande.LinkedTaskItemId);
                if (task != null) task.Statut = "EnAttenteRevision";
            }
            await NotifyClientUsersAsync(demande.ClientId, cabinetId, "Document rejeté",
                $"Votre document « {demande.DocumentType} » a été rejeté ({demande.RejectionReasonCode}). Merci de le renvoyer.",
                $"/Domiciliation/Documents/{demande.Id}");
            await AddTimelineAsync(cabinetId, demande.ClientId, "DemandeDocument", demande.Id, "WaitingClient", "Rejected", "WaitingClient", userId, userName, null, demande.CurrentDocumentId);
        }

        demande.UpdatedAt = DateTime.UtcNow;
        demande.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        var versions = demande.RootDocumentId.HasValue
            ? await LoadVersionsAsync(demande.RootDocumentId.Value, cabinetId)
            : new List<DocumentDto>();
        return ApiResponse<DemandeDocumentDomiciliationDto>.Ok(MapDemandeDoc(demande, versions), "Revue enregistrée.");
    }

    public async Task<ApiResponse<List<CourrierDomiciliationDto>>> GetClientNotifiedCourriersAsync(int clientId, int cabinetId)
    {
        var list = await QueryCourrier(cabinetId)
            .Where(c => c.ClientId == clientId && c.ClientNotifie && !c.IsDeleted)
            .OrderByDescending(c => c.DateReception)
            .ToListAsync();
        return ApiResponse<List<CourrierDomiciliationDto>>.Ok(list.Select(MapCourrier).ToList());
    }

    public async Task<ApiResponse<List<DemandeDocumentDomiciliationDto>>> GetClientDocumentRequestsAsync(int clientId, int cabinetId)
    {
        var list = await _db.DemandesDocumentDomiciliation
            .Include(d => d.CurrentDocument)
            .Where(d => d.ClientId == clientId && d.CabinetId == cabinetId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        var result = new List<DemandeDocumentDomiciliationDto>();
        foreach (var d in list)
        {
            var versions = d.RootDocumentId.HasValue ? await LoadVersionsAsync(d.RootDocumentId.Value, cabinetId) : null;
            result.Add(MapDemandeDoc(d, versions));
        }
        return ApiResponse<List<DemandeDocumentDomiciliationDto>>.Ok(result);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private async Task<bool> IsDomiciledAsync(int clientId, int cabinetId)
    {
        return await _db.ServiceContrats.AnyAsync(s =>
            s.ClientId == clientId
            && s.Client!.CabinetId == cabinetId
            && s.Actif
            && s.TypeService == RefCodes.TypeService.Domiciliation
            && !s.IsDeleted);
    }

    private async Task UpsertTaskForCourrierAsync(CourrierDomiciliation entity, int cabinetId, string userId, string statut)
    {
        TaskItem? task = null;
        if (entity.LinkedTaskItemId.HasValue)
            task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == entity.LinkedTaskItemId);

        if (task == null)
        {
            task = new TaskItem
            {
                CabinetId = cabinetId,
                ClientId = entity.ClientId,
                Titre = $"Courrier: {entity.TypeCourrier} — {entity.Expediteur ?? "Expéditeur inconnu"}",
                Description = entity.Notes,
                AssignedToUserId = entity.ResponsableUserId,
                AssignedByUserId = userId,
                DateEcheance = entity.DateEcheance ?? DateTime.UtcNow.AddDays(5),
                Priorite = entity.ActionRequise ? "Haute" : "Moyenne",
                Statut = statut,
                RelatedEntityType = "CourrierDomiciliation",
                RelatedEntityId = entity.Id,
                TaskType = "Courrier",
                CreatedBy = userId
            };
            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();
            entity.LinkedTaskItemId = task.Id;
        }
        else
        {
            task.Statut = statut;
            task.AssignedToUserId = entity.ResponsableUserId ?? task.AssignedToUserId;
            if (entity.DateEcheance.HasValue) task.DateEcheance = entity.DateEcheance.Value;
            task.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task AddTimelineAsync(int cabinetId, int clientId, string relatedType, int relatedId,
        string action, string? prev, string? next, string? userId, string? userName, string? note, int? documentId)
    {
        _db.DomiciliationTimelineEvents.Add(new DomiciliationTimelineEvent
        {
            CabinetId = cabinetId,
            ClientId = clientId,
            RelatedType = relatedType,
            RelatedId = relatedId,
            Action = action,
            PreviousStatus = prev,
            NewStatus = next,
            UserId = userId,
            UserName = userName,
            Note = note,
            DocumentId = documentId,
            At = DateTime.UtcNow,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();
    }

    private async Task NotifyClientUsersAsync(int clientId, int cabinetId, string titre, string message, string url)
    {
        var users = await _db.Users
            .Where(u => u.ClientId == clientId && u.CabinetId == cabinetId && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();
        foreach (var uid in users)
            await _notifications.CreateAsync(uid, titre, message, "Info", "Domiciliation", url, cabinetId);
    }

    private async Task NotifyStaffUsersAsync(int cabinetId, string? preferredUserId, string titre, string message, string url)
    {
        var ids = new List<string>();
        if (!string.IsNullOrWhiteSpace(preferredUserId))
            ids.Add(preferredUserId);

        if (ids.Count == 0)
        {
            ids = await _db.Users.AsNoTracking()
                .Where(u => u.CabinetId == cabinetId && u.ClientId == null && u.IsActive)
                .OrderBy(u => u.Id)
                .Select(u => u.Id)
                .Take(15)
                .ToListAsync();
        }

        foreach (var uid in ids.Distinct())
            await _notifications.CreateAsync(uid, titre, message, "Action", "Domiciliation", url, cabinetId);
    }

    private async Task<List<DocumentDto>> LoadVersionsAsync(int rootId, int cabinetId)
    {
        var docs = await _db.Documents.AsNoTracking()
            .Where(d => d.CabinetId == cabinetId && !d.IsDeleted && (d.Id == rootId || d.ParentDocumentId == rootId))
            .OrderBy(d => d.Version)
            .ToListAsync();
        return docs.Select(d => new DocumentDto
        {
            Id = d.Id,
            Titre = d.Titre,
            NomFichier = d.NomFichier,
            Categorie = d.Categorie,
            Statut = d.Statut,
            Version = d.Version,
            EstPartage = d.PartageExtranet,
            ClientId = d.ClientId,
            CreatedAt = d.CreatedAt,
            DateUpload = d.CreatedAt,
            ContentType = d.ContentType
        }).ToList();
    }

    private IQueryable<CourrierDomiciliation> QueryCourrier(int cabinetId) =>
        _db.CourriersDomiciliation
            .Include(c => c.Client)
            .Include(c => c.ScanDocument)
            .Include(c => c.Responsable)
            .Include(c => c.ActionsClient)
            .Include(c => c.Pieces.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Document)
            .Where(c => c.CabinetId == cabinetId);

    private async Task<CourrierDomiciliationDto> LoadCourrierMappedAsync(int id, int cabinetId)
    {
        var entity = await QueryCourrier(cabinetId).AsNoTracking()
            .FirstAsync(c => c.Id == id && !c.IsDeleted);
        return MapCourrier(entity);
    }

    private async Task<(bool Success, string? Message)> AddPieceInternalAsync(
        CourrierDomiciliation entity, int documentId, int cabinetId, string userId, bool makePrimary)
    {
        if (documentId <= 0) return (false, "Document invalide.");

        var doc = await _db.Documents.FirstOrDefaultAsync(d =>
            d.Id == documentId && d.CabinetId == cabinetId && !d.IsDeleted);
        if (doc == null) return (false, "Document introuvable.");
        if (doc.ClientId.HasValue && doc.ClientId != entity.ClientId)
            return (false, "Le document n'appartient pas à ce client.");
        if (!doc.ClientId.HasValue)
            doc.ClientId = entity.ClientId;

        var exists = entity.Pieces.Any(p => !p.IsDeleted && p.DocumentId == documentId)
            || await _db.CourrierPiecesJointes.AnyAsync(p =>
                p.CourrierId == entity.Id && p.DocumentId == documentId && !p.IsDeleted);
        if (exists) return (false, "Ce document est déjà dans la photothèque.");

        var sort = entity.Pieces.Where(p => !p.IsDeleted).Select(p => p.SortOrder).DefaultIfEmpty(-1).Max() + 1;
        if (makePrimary)
        {
            foreach (var p in entity.Pieces.Where(x => !x.IsDeleted))
                p.IsPrimary = false;
            entity.ScanDocumentId = documentId;
        }

        var piece = new CourrierPieceJointe
        {
            CabinetId = cabinetId,
            CourrierId = entity.Id,
            DocumentId = documentId,
            IsPrimary = makePrimary || !entity.Pieces.Any(p => !p.IsDeleted),
            SortOrder = sort,
            Label = doc.Titre,
            CreatedBy = userId
        };
        if (piece.IsPrimary)
            entity.ScanDocumentId = documentId;

        _db.CourrierPiecesJointes.Add(piece);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return (true, $"Document ajouté: {doc.Titre ?? doc.NomFichier}");
    }

    private static CourrierDomiciliationDto MapCourrier(CourrierDomiciliation c) => new()
    {
        Id = c.Id,
        ClientId = c.ClientId,
        ClientNom = c.Client?.RaisonSociale,
        DateReception = c.DateReception,
        Expediteur = c.Expediteur,
        TypeCourrier = c.TypeCourrier,
        Reference = c.Reference,
        Confidentialite = c.Confidentialite,
        ScanDocumentId = c.ScanDocumentId,
        ScanDocumentTitre = c.ScanDocument?.Titre
            ?? c.Pieces?.FirstOrDefault(p => p.IsPrimary && !p.IsDeleted)?.Document?.Titre,
        ScanPartageExtranet = c.ScanDocument?.PartageExtranet
            ?? c.Pieces?.FirstOrDefault(p => p.IsPrimary && !p.IsDeleted)?.Document?.PartageExtranet
            ?? false,
        ActionRequise = c.ActionRequise,
        ResponsableUserId = c.ResponsableUserId,
        ResponsableNom = c.Responsable != null ? $"{c.Responsable.FirstName} {c.Responsable.LastName}".Trim() : null,
        DateEcheance = c.DateEcheance,
        Statut = c.Statut,
        ClientNotifie = c.ClientNotifie,
        PartageDocumentClient = c.PartageDocumentClient,
        Notes = c.Notes,
        LinkedTaskItemId = c.LinkedTaskItemId,
        Pieces = c.Pieces?
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.IsPrimary)
            .ThenBy(p => p.SortOrder)
            .Select(MapPiece)
            .ToList() ?? new(),
        ActionsClient = c.ActionsClient?.Select(MapAction).ToList() ?? new()
    };

    private static CourrierPieceJointeDto MapPiece(CourrierPieceJointe p) => new()
    {
        Id = p.Id,
        CourrierId = p.CourrierId,
        DocumentId = p.DocumentId,
        Titre = p.Document?.Titre ?? p.Label ?? $"Document #{p.DocumentId}",
        NomFichier = p.Document?.NomFichier ?? "",
        ContentType = p.Document?.ContentType,
        TailleFichier = p.Document?.TailleFichier ?? 0,
        IsPrimary = p.IsPrimary,
        SortOrder = p.SortOrder,
        Label = p.Label,
        CreatedAt = p.CreatedAt
    };

    private static DemandeActionCourrierDto MapAction(DemandeActionCourrier a) => new()
    {
        Id = a.Id,
        CourrierId = a.CourrierId,
        TypeAction = a.TypeAction,
        AdresseForward = a.AdresseForward,
        ShippingReference = a.ShippingReference,
        Statut = a.Statut,
        Notes = a.Notes,
        IdentityCheckDone = a.IdentityCheckDone,
        ConfirmedAt = a.ConfirmedAt,
        CreatedAt = a.CreatedAt
    };

    private static DemandeDocumentDomiciliationDto MapDemandeDoc(DemandeDocumentDomiciliation d, List<DocumentDto>? versions) => new()
    {
        Id = d.Id,
        ClientId = d.ClientId,
        ClientNom = d.Client?.RaisonSociale,
        DocumentType = d.DocumentType,
        Message = d.Message,
        ResponsableUserId = d.ResponsableUserId,
        ResponsableNom = d.Responsable != null ? $"{d.Responsable.FirstName} {d.Responsable.LastName}".Trim() : null,
        DateEcheance = d.DateEcheance,
        Statut = d.Statut,
        CurrentDocumentId = d.CurrentDocumentId,
        RootDocumentId = d.RootDocumentId,
        RejectionReasonCode = d.RejectionReasonCode,
        RejectionReasonText = d.RejectionReasonText,
        Versions = versions ?? new()
    };
}
