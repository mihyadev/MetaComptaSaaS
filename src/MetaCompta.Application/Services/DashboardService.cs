using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class DashboardService : IDashboardService
{
    private static readonly HashSet<string> ClosedDeclarationStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        RefCodes.StatutDeclaration.Soumise,
        RefCodes.StatutDeclaration.Validee,
        RefCodes.StatutDeclaration.Payee,
        "Payée", "Cloturee", "Clôturée", "Archivee", "Archivée", "Rapprochee", "Rapprochée"
    };

    private readonly MetaComptaDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IContractAccessService _contractAccess;

    public DashboardService(
        MetaComptaDbContext db,
        INotificationService notifications,
        IContractAccessService contractAccess)
    {
        _db = db;
        _notifications = notifications;
        _contractAccess = contractAccess;
    }

    public async Task<ApiResponse<DashboardDto>> GetManagerDashboardAsync(int cabinetId, string userId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var dashboard = new DashboardDto
        {
            TotalClientsActifs = await _db.Clients.CountAsync(c => c.CabinetId == cabinetId && c.Statut == RefCodes.StatutClient.Active),
            DeclarationsMoisEnCours = await _db.Declarations.CountAsync(d => d.CabinetId == cabinetId && d.DateLimite >= monthStart && d.DateLimite < monthEnd),
            DeclarationsEnAttente = await _db.Declarations.CountAsync(d => d.CabinetId == cabinetId && (d.Statut == RefCodes.StatutDeclaration.Completee || d.Statut == RefCodes.StatutDeclaration.EnRevision)),
            DeclarationsSoumises = await _db.Declarations.CountAsync(d => d.CabinetId == cabinetId && d.Statut == RefCodes.StatutDeclaration.Soumise && d.DateSoumission >= monthStart),
            DocumentsEnAttente = await _db.Documents.CountAsync(d => d.CabinetId == cabinetId && d.Statut == RefCodes.StatutDocument.EnAttente),
            TachesEnCours = await _db.TaskItems.CountAsync(t => t.CabinetId == cabinetId && (t.Statut == RefCodes.StatutTache.AFaire || t.Statut == RefCodes.StatutTache.EnCours)),
        };

        var upcomingDeadlines = await _db.Declarations
            .Include(d => d.Client)
            .Where(d => d.CabinetId == cabinetId && d.Statut != RefCodes.StatutDeclaration.Soumise && d.DateLimite >= now && d.DateLimite <= now.AddDays(30))
            .OrderBy(d => d.DateLimite)
            .Take(10)
            .Select(d => new DeclarationDto
            {
                Id = d.Id,
                ClientName = d.Client!.RaisonSociale,
                Type = d.Type,
                Periode = d.Periode,
                DateLimite = d.DateLimite,
                Statut = d.Statut,
                DaysUntilDeadline = (d.DateLimite - now).Days
            }).ToListAsync();
        dashboard.ProchainsDelais = upcomingDeadlines;

        var notifs = await _notifications.GetUserNotificationsAsync(userId, false, 5);
        dashboard.NotificationsRecentes = notifs.Data ?? new();
        dashboard.Charts = await BuildChartsDataAsync(cabinetId);

        return ApiResponse<DashboardDto>.Ok(dashboard);
    }

    public async Task<ApiResponse<DashboardDto>> GetAssistantDashboardAsync(int cabinetId, string userId)
        => await GetManagerDashboardAsync(cabinetId, userId);

    public async Task<ApiResponse<ExtranetDashboardDto>> GetExtranetDashboardAsync(int clientId, int? exerciceComptableId = null)
    {
        var now = DateTime.UtcNow;
        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
        if (client == null)
            return ApiResponse<ExtranetDashboardDto>.Fail("Client introuvable.");

        var exercice = await ResolveExerciceAsync(clientId, client.CabinetId, exerciceComptableId);
        var declQuery = FilterByExercice(_db.Declarations.AsNoTracking().Where(d => d.ClientId == clientId), exercice);

        var declarations = await declQuery
            .Select(d => new DeclLite(d.Id, d.Type, d.Periode, d.DateLimite, d.Statut, d.DateSoumission, d.MontantDu, d.CreatedAt))
            .ToListAsync();

        var declarationsEnCours = declarations.Count(d =>
            !ClosedDeclarationStatuses.Contains(d.Statut)
            && !string.Equals(d.Statut, RefCodes.StatutDeclaration.Brouillon, StringComparison.OrdinalIgnoreCase));
        var declarationsSoumises = declarations.Count(d => ClosedDeclarationStatuses.Contains(d.Statut));

        var upcomingDeadlines = declarations
            .Where(d => !ClosedDeclarationStatuses.Contains(d.Statut) && d.DateLimite >= now.AddDays(-30))
            .OrderBy(d => d.DateLimite)
            .Take(10)
            .Select(d => new ExtranetDeadlineDto { Id = d.Id, Type = d.Type, Periode = d.Periode, DateLimite = d.DateLimite, Statut = d.Statut })
            .ToList();

        var recentDeclarations = declarations
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new ExtranetRecentDto
            {
                Id = d.Id,
                Type = d.Type,
                Periode = d.Periode,
                DateSoumission = d.DateSoumission,
                MontantDu = d.MontantDu ?? 0m,
                Statut = d.Statut
            })
            .ToList();

        var employeeCount = await _db.Employees.CountAsync(e => e.ClientId == clientId && e.Statut == RefCodes.StatutEmploye.Actif);
        var documentCount = await _db.Documents.CountAsync(d => d.ClientId == clientId && d.PartageExtranet);

        var tachesEnAttente = await _db.WorkflowStepInstances
            .CountAsync(s => !s.IsDeleted
                && s.WorkflowInstance != null
                && s.WorkflowInstance.ClientId == clientId
                && s.WorkflowInstance.Status == WorkflowInstanceStatus.Active
                && s.Status == WorkflowStepStatus.WaitingForClient
                && s.WorkflowStepTemplate != null
                && (s.WorkflowStepTemplate.IsClientStep
                    || s.WorkflowStepTemplate.AssignmentType == AssignmentType.Client));

        var messagesNonLus = await _db.Messages
            .CountAsync(m => m.ClientId == clientId && !m.IsReadByRecipient && m.SenderRole != "Client");

        var access = await _contractAccess.GetContractAccessContextAsync(clientId, client.CabinetId, exercice?.Id);
        var existingTypes = declarations
            .Select(d => DeclarationServiceCatalog.NormalizeType(d.Type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var dossierSummaries = BuildDossierSummaries(access.ActiveServices, existingTypes, declarations, now);

        var dto = new ExtranetDashboardDto
        {
            ClientName = client.RaisonSociale,
            ICE = client.ICE,
            FormeJuridique = client.FormeJuridique,
            SecteurActivite = client.SecteurActivite,
            Statut = client.Statut,
            EmployeeCount = employeeCount,
            DocumentCount = documentCount,
            DeclarationsEnCours = declarationsEnCours,
            DeclarationsSoumises = declarationsSoumises,
            TachesEnAttente = tachesEnAttente,
            MessagesNonLus = messagesNonLus,
            ExerciceComptableId = exercice?.Id,
            ExerciceAnnee = exercice?.Annee,
            ExerciceStatut = exercice?.Statut,
            ActiveServices = access.ActiveServices,
            DossierSummaries = dossierSummaries,
            ProchainsDelais = upcomingDeadlines,
            DernieresDeclarations = recentDeclarations
        };

        return ApiResponse<ExtranetDashboardDto>.Ok(dto);
    }

    public async Task<ApiResponse<ExtranetContextDto>> GetExtranetContextAsync(int clientId, int cabinetId, int? exerciceComptableId = null)
    {
        var exercices = await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId)
            .OrderByDescending(e => e.Annee)
            .Select(e => new ExerciceComptableDto
            {
                Id = e.Id,
                ClientId = e.ClientId,
                Annee = e.Annee,
                DateDebut = e.DateDebut,
                DateFin = e.DateFin,
                Statut = e.Statut
            })
            .ToListAsync();

        var current = ResolveExerciceFromList(exercices, exerciceComptableId);
        var access = await _contractAccess.GetContractAccessContextAsync(clientId, cabinetId, current?.Id);

        ExerciceComptable? exoEntity = null;
        if (current != null)
        {
            exoEntity = new ExerciceComptable
            {
                Id = current.Id,
                Annee = current.Annee,
                DateDebut = current.DateDebut,
                DateFin = current.DateFin,
                ClientId = current.ClientId,
                CabinetId = cabinetId,
                Statut = current.Statut
            };
        }

        var declQuery = FilterByExercice(
            _db.Declarations.AsNoTracking().Where(d => d.ClientId == clientId && d.CabinetId == cabinetId),
            exoEntity);

        var declarations = await declQuery
            .Select(d => new DeclLite(d.Id, d.Type, d.Periode, d.DateLimite, d.Statut, d.DateSoumission, d.MontantDu, d.CreatedAt))
            .ToListAsync();

        var existingTypes = declarations
            .Select(d => DeclarationServiceCatalog.NormalizeType(d.Type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var summaries = BuildDossierSummaries(access.ActiveServices, existingTypes, declarations, DateTime.UtcNow);
        if (access.ActiveServices.Any(s => string.Equals(s, RefCodes.TypeService.Domiciliation, StringComparison.OrdinalIgnoreCase)))
        {
            var openMail = await _db.CourriersDomiciliation.CountAsync(c =>
                c.ClientId == clientId && c.CabinetId == cabinetId && c.ClientNotifie && !c.IsDeleted
                && c.Statut != "Archived" && c.Statut != "Closed");
            var openDocs = await _db.DemandesDocumentDomiciliation.CountAsync(d =>
                d.ClientId == clientId && d.CabinetId == cabinetId && !d.IsDeleted
                && d.Statut != "Closed" && d.Statut != "Validated");
            summaries.Add(new ExtranetDossierSummaryDto
            {
                Type = "DOMICILIATION",
                DisplayName = "Domiciliation & courrier",
                Frequency = "Continue",
                IsShellOnly = true,
                EnCours = openMail + openDocs,
                Urgentes = 0,
                Cloturees = 0,
                Total = openMail + openDocs
            });
        }

        var dto = new ExtranetContextDto
        {
            CurrentExercice = current,
            Exercices = exercices,
            ActiveServices = access.ActiveServices,
            DossierSummaries = summaries
        };

        return ApiResponse<ExtranetContextDto>.Ok(dto);
    }

    private async Task<ExerciceComptable?> ResolveExerciceAsync(int clientId, int cabinetId, int? exerciceComptableId)
    {
        if (exerciceComptableId is > 0)
        {
            return await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciceComptableId && e.ClientId == clientId && e.CabinetId == cabinetId);
        }

        var ouvert = await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId
                && (e.Statut == "Ouvert" || e.Statut == "Ouverte"))
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();
        if (ouvert != null) return ouvert;

        return await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId)
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();
    }

    private static ExerciceComptableDto? ResolveExerciceFromList(List<ExerciceComptableDto> exercices, int? exerciceComptableId)
    {
        if (exerciceComptableId is > 0)
            return exercices.FirstOrDefault(e => e.Id == exerciceComptableId.Value)
                ?? exercices.FirstOrDefault();

        return exercices.FirstOrDefault(e =>
                   string.Equals(e.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(e.Statut, "Ouverte", StringComparison.OrdinalIgnoreCase))
               ?? exercices.FirstOrDefault();
    }

    private static IQueryable<Declaration> FilterByExercice(IQueryable<Declaration> query, ExerciceComptable? exo)
    {
        if (exo == null) return query;

        var exoId = exo.Id;
        var annee = exo.Annee;
        var debut = exo.DateDebut.Date;
        var fin = exo.DateFin.Date;
        var anneeStr = annee.ToString();

        return query.Where(d =>
            d.ExerciceComptableId == exoId
            || (d.ExerciceComptableId == null
                && d.DateDebut >= debut && d.DateFin <= fin)
            || (d.ExerciceComptableId == null
                && d.Periode != null
                && (d.Periode == anneeStr
                    || d.Periode.StartsWith(anneeStr + "-")
                    || d.Periode.StartsWith(anneeStr + "Q")
                    || d.Periode.StartsWith(anneeStr + "-Q"))));
    }

    private static List<ExtranetDossierSummaryDto> BuildDossierSummaries(
        IEnumerable<string> activeServices,
        HashSet<string> existingTypes,
        List<DeclLite> declarations,
        DateTime now)
    {
        var serviceSet = new HashSet<string>(activeServices, StringComparer.OrdinalIgnoreCase);
        var byType = declarations
            .GroupBy(d => DeclarationServiceCatalog.NormalizeType(d.Type), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var menuTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in DeclarationServiceCatalog.ForMenu(serviceSet))
        {
            if (entry.IsShellOnly && !existingTypes.Contains(entry.DeclarationType))
                continue;
            menuTypes.Add(entry.DeclarationType);
        }
        foreach (var t in existingTypes)
            menuTypes.Add(t);

        var result = new List<ExtranetDossierSummaryDto>();
        foreach (var type in DeclarationServiceCatalog.All
                     .Select(e => e.DeclarationType)
                     .Where(menuTypes.Contains)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var entry = DeclarationServiceCatalog.Get(type);
            byType.TryGetValue(type, out var items);
            items ??= new List<DeclLite>();

            var enCours = 0;
            var urgentes = 0;
            var cloturees = 0;
            DateTime? nextDeadline = null;

            foreach (var d in items)
            {
                var closed = ClosedDeclarationStatuses.Contains(d.Statut);
                if (closed) cloturees++;
                else
                {
                    enCours++;
                    var days = (d.DateLimite.Date - now.Date).TotalDays;
                    if (days <= 7) urgentes++;
                    if (nextDeadline == null || d.DateLimite < nextDeadline)
                        nextDeadline = d.DateLimite;
                }
            }

            result.Add(new ExtranetDossierSummaryDto
            {
                Type = type,
                DisplayName = entry?.DisplayName ?? type,
                Frequency = entry?.Frequency ?? "",
                IsShellOnly = entry?.IsShellOnly ?? false,
                EnCours = enCours,
                Urgentes = urgentes,
                Cloturees = cloturees,
                Total = items.Count,
                NextDeadline = nextDeadline
            });
        }

        return result;
    }

    private sealed record DeclLite(
        int Id,
        string Type,
        string Periode,
        DateTime DateLimite,
        string Statut,
        DateTime? DateSoumission,
        decimal? MontantDu,
        DateTime CreatedAt);

    private async Task<DashboardChartsDto> BuildChartsDataAsync(int cabinetId)
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        var declarationsEvolution = await _db.Declarations
            .Where(d => d.CabinetId == cabinetId && d.CreatedAt >= sixMonthsAgo)
            .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month, d.Type })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, Count = g.Count() })
            .ToListAsync();

        var clientDistribution = await _db.Clients
            .Where(c => c.CabinetId == cabinetId && c.Statut == "Active")
            .GroupBy(c => c.SecteurActivite ?? "Non défini")
            .Select(g => new { Sector = g.Key, Count = g.Count() })
            .ToListAsync();

        var monthStart = new DateTime(now.Year, now.Month, 1);
        var declarationStatus = await _db.Declarations
            .Where(d => d.CabinetId == cabinetId && d.DateLimite >= monthStart && d.DateLimite < monthStart.AddMonths(1))
            .GroupBy(d => d.Statut)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new DashboardChartsDto
        {
            DeclarationsEvolution = declarationsEvolution,
            ClientsDistribution = clientDistribution,
            DeclarationStatusGlobal = declarationStatus
        };
    }
}
