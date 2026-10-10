using System.Text.Json;
using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

/// <summary>
/// Service de gestion du cycle de vie commercial complet :
/// Devis → Bon de Commande → Contrat → Facture + Paiements
/// Avec traçabilité, workflow strict, et auto-activation des services.
/// </summary>
public class CommercialService : ICommercialService
{
    private readonly MetaComptaDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWorkflowService _workflow;

    public CommercialService(MetaComptaDbContext db, IMapper mapper, IWorkflowService workflow)
    {
        _db = db;
        _mapper = mapper;
        _workflow = workflow;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DEVIS
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<DevisDto>>> GetDevisAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Devis
            .Where(d => d.CabinetId == cabinetId)
            .Include(d => d.Client)
            .Include(d => d.Commercial)
            .Include(d => d.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(d => d.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(d => d.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<DevisDto>>.Ok(new PagedResult<DevisDto>
        {
            Items = _mapper.Map<List<DevisDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<DevisDto>> GetDevisByIdAsync(int id, int cabinetId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .Include(d => d.Client)
            .Include(d => d.Commercial)
            .Include(d => d.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(d => d.Historique.OrderByDescending(h => h.DateAction))
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");

        return ApiResponse<DevisDto>.Ok(_mapper.Map<DevisDto>(devis));
    }

    public async Task<ApiResponse<DevisDto>> CreateDevisAsync(DevisCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<DevisDto>.Fail("Le client est requis.");

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (!clientExists)
            return ApiResponse<DevisDto>.Fail("Client introuvable.");

        // Générer la référence
        var reference = await GenerateReferenceAsync("DEV", cabinetId);

        var devis = _mapper.Map<Devis>(dto);
        devis.CabinetId = cabinetId;
        devis.Reference = reference;
        devis.DateEmission = DateTime.UtcNow;
        devis.Statut = RefCodes.StatutDevis.Brouillon;
        devis.Version = 1;
        devis.CreatedBy = userId;

        // Ajouter les lignes et calculer les totaux
        if (dto.Lignes.Any())
        {
            foreach (var ligneDto in dto.Lignes)
            {
                var ligne = _mapper.Map<LigneDocumentCommercial>(ligneDto);
                ligne.CabinetId = cabinetId;
                ligne.CreatedBy = userId;
                CalculerLigne(ligne);
                devis.Lignes.Add(ligne);
            }
            RecalculerTotaux(devis);
        }

        _db.Devis.Add(devis);

        // Historique
        AddHistorique("Devis", devis, "Création", null, RefCodes.StatutDevis.Brouillon, userId);

        await _db.SaveChangesAsync();

        return await GetDevisByIdAsync(devis.Id, cabinetId);
    }

    public async Task<ApiResponse<DevisDto>> UpdateDevisAsync(int id, DevisCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<DevisDto>.Fail("Le client est requis.");

        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .Include(d => d.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");

        if (devis.Statut != RefCodes.StatutDevis.Brouillon)
            return ApiResponse<DevisDto>.Fail("Seul un devis en brouillon peut être modifié.");

        // Mise à jour des champs
        devis.ClientId = dto.ClientId;
        devis.Objet = dto.Objet;
        devis.Description = dto.Description;
        devis.Devise = dto.Devise;
        devis.TauxChange = dto.TauxChange;
        devis.DateValidite = dto.DateValidite;
        devis.ConditionsParticulieres = dto.ConditionsParticulieres;
        devis.NotesPourClient = dto.NotesPourClient;
        devis.CommercialId = dto.CommercialId;
        devis.Notes = dto.Notes;
        devis.UpdatedBy = userId;
        devis.UpdatedAt = DateTime.UtcNow;

        // Supprimer les anciennes lignes et recréer
        _db.LignesDocumentsCommerciaux.RemoveRange(devis.Lignes);
        devis.Lignes.Clear();

        foreach (var ligneDto in dto.Lignes)
        {
            var ligne = _mapper.Map<LigneDocumentCommercial>(ligneDto);
            ligne.CabinetId = cabinetId;
            ligne.CreatedBy = userId;
            CalculerLigne(ligne);
            devis.Lignes.Add(ligne);
        }
        RecalculerTotaux(devis);

        AddHistorique("Devis", devis, "Modification", RefCodes.StatutDevis.Brouillon, RefCodes.StatutDevis.Brouillon, userId);

        await _db.SaveChangesAsync();
        return await GetDevisByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<bool>> DeleteDevisAsync(int id, int cabinetId, string userId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<bool>.Fail("Devis introuvable.");

        if (devis.Statut != RefCodes.StatutDevis.Brouillon)
            return ApiResponse<bool>.Fail("Seul un devis en brouillon peut être supprimé.");

        devis.IsDeleted = true;
        devis.DeletedBy = userId;
        devis.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<DevisDto>> SoumettreDevisAsync(int id, int cabinetId, string userId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .Include(d => d.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");
        if (devis.Statut != RefCodes.StatutDevis.Brouillon)
            return ApiResponse<DevisDto>.Fail("Seul un devis en brouillon peut être soumis.");
        if (!devis.Lignes.Any())
            return ApiResponse<DevisDto>.Fail("Le devis doit contenir au moins une ligne.");

        var ancienStatut = devis.Statut;
        devis.Statut = RefCodes.StatutDevis.Soumis;
        devis.UpdatedBy = userId;
        devis.UpdatedAt = DateTime.UtcNow;

        AddHistorique("Devis", devis, "Soumission", ancienStatut, RefCodes.StatutDevis.Soumis, userId);

        await _db.SaveChangesAsync();
        return await GetDevisByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<DevisDto>> ApprouverDevisAsync(int id, int cabinetId, string userId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");
        if (devis.Statut != RefCodes.StatutDevis.Soumis)
            return ApiResponse<DevisDto>.Fail("Seul un devis soumis peut être approuvé.");

        var ancienStatut = devis.Statut;
        devis.Statut = RefCodes.StatutDevis.Approuve;
        devis.DateApprobation = DateTime.UtcNow;
        devis.ValidateurId = userId;
        devis.UpdatedBy = userId;
        devis.UpdatedAt = DateTime.UtcNow;

        AddHistorique("Devis", devis, "Approbation", ancienStatut, RefCodes.StatutDevis.Approuve, userId);

        await _db.SaveChangesAsync();
        return await GetDevisByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<DevisDto>> RejeterDevisAsync(int id, string motif, int cabinetId, string userId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");
        if (devis.Statut != RefCodes.StatutDevis.Soumis)
            return ApiResponse<DevisDto>.Fail("Seul un devis soumis peut être rejeté.");

        var ancienStatut = devis.Statut;
        devis.Statut = RefCodes.StatutDevis.Rejete;
        devis.MotifRejet = motif;
        devis.DateRejet = DateTime.UtcNow;
        devis.UpdatedBy = userId;
        devis.UpdatedAt = DateTime.UtcNow;

        AddHistorique("Devis", devis, "Rejet", ancienStatut, RefCodes.StatutDevis.Rejete, userId, motif);

        await _db.SaveChangesAsync();
        return await GetDevisByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<DevisDto>> DupliquerDevisAsync(int id, int cabinetId, string userId)
    {
        var source = await _db.Devis
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .Include(d => d.Lignes.OrderBy(l => l.Ordre))
            .FirstOrDefaultAsync();

        if (source == null)
            return ApiResponse<DevisDto>.Fail("Devis introuvable.");

        var reference = await GenerateReferenceAsync("DEV", cabinetId);

        var copie = new Devis
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = source.ClientId,
            Objet = source.Objet != null ? $"{source.Objet} (copie)" : "Copie",
            Description = source.Description,
            Devise = source.Devise,
            TauxChange = source.TauxChange,
            DateEmission = DateTime.UtcNow,
            DateValidite = source.DateValidite,
            Statut = RefCodes.StatutDevis.Brouillon,
            ConditionsParticulieres = source.ConditionsParticulieres,
            NotesPourClient = source.NotesPourClient,
            Notes = source.Notes,
            Version = 1,
            CreatedBy = userId
        };

        foreach (var l in source.Lignes)
        {
            var ligne = new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = l.MontantHT,
                MontantTVA = l.MontantTVA,
                MontantTTC = l.MontantTTC,
                CreatedBy = userId
            };
            copie.Lignes.Add(ligne);
        }

        copie.MontantHT = source.MontantHT;
        copie.MontantTVA = source.MontantTVA;
        copie.MontantTTC = source.MontantTTC;
        copie.MontantRemise = source.MontantRemise;

        _db.Devis.Add(copie);
        AddHistorique("Devis", copie, "Création", null, RefCodes.StatutDevis.Brouillon, userId, $"Dupliqué depuis {source.Reference}");
        await _db.SaveChangesAsync();

        return await GetDevisByIdAsync(copie.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CONVERSION: DEVIS → BON DE COMMANDE
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<BonCommandeDto>> ConvertirDevisEnBonCommandeAsync(int devisId, int cabinetId, string userId)
    {
        var devis = await _db.Devis
            .Where(d => d.Id == devisId && d.CabinetId == cabinetId)
            .Include(d => d.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(d => d.BonCommande)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<BonCommandeDto>.Fail("Devis introuvable.");
        if (devis.Statut != RefCodes.StatutDevis.Approuve)
            return ApiResponse<BonCommandeDto>.Fail("Seul un devis approuvé peut être converti en bon de commande.");
        if (devis.BonCommande != null)
            return ApiResponse<BonCommandeDto>.Fail("Ce devis a déjà été converti en bon de commande.");

        var reference = await GenerateReferenceAsync("PO", cabinetId);

        var bc = new BonCommande
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = devis.ClientId,
            DevisId = devis.Id,
            Objet = devis.Objet,
            Description = devis.Description,
            Devise = devis.Devise,
            TauxChange = devis.TauxChange,
            DateEmission = DateTime.UtcNow,
            Statut = RefCodes.StatutBonCommande.Brouillon,
            MontantHT = devis.MontantHT,
            MontantTVA = devis.MontantTVA,
            MontantTTC = devis.MontantTTC,
            MontantRemise = devis.MontantRemise,
            ConditionsParticulieres = devis.ConditionsParticulieres,
            Version = 1,
            CreatedBy = userId
        };

        // Copier les lignes
        foreach (var ligneSource in devis.Lignes)
        {
            bc.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = ligneSource.TypeLigne,
                TypeService = ligneSource.TypeService,
                CodeService = ligneSource.CodeService,
                ServiceContratId = ligneSource.ServiceContratId,
                Ordre = ligneSource.Ordre,
                Description = ligneSource.Description,
                DescriptionDetaillee = ligneSource.DescriptionDetaillee,
                Quantite = ligneSource.Quantite,
                Unite = ligneSource.Unite,
                PrixUnitaireHT = ligneSource.PrixUnitaireHT,
                RemisePourcentage = ligneSource.RemisePourcentage,
                RemiseMontant = ligneSource.RemiseMontant,
                TauxTVA = ligneSource.TauxTVA,
                MontantTVA = ligneSource.MontantTVA,
                MontantHT = ligneSource.MontantHT,
                MontantTTC = ligneSource.MontantTTC,
                LigneSourceId = ligneSource.Id,
                CreatedBy = userId
            });
        }

        _db.BonsCommande.Add(bc);

        // Historique sur le devis
        AddHistorique("Devis", devis, "Conversion", RefCodes.StatutDevis.Approuve, RefCodes.StatutDevis.Approuve, userId, $"Converti en Bon de Commande {reference}");
        // Historique sur le BC
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "BonCommande",
            DocumentId = 0, // Sera mis à jour après SaveChanges
            Action = "Création",
            NouveauStatut = RefCodes.StatutBonCommande.Brouillon,
            UserId = userId,
            Commentaire = $"Créé depuis le Devis {devis.Reference}",
            DateAction = DateTime.UtcNow,
            VersionDocument = 1,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        // Mettre à jour l'historique avec le bon ID
        var lastHist = await _db.DocumentsCommerciauxHistorique
            .Where(h => h.TypeDocument == "BonCommande" && h.DocumentId == 0 && h.CabinetId == cabinetId)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync();
        if (lastHist != null)
        {
            lastHist.DocumentId = bc.Id;
            lastHist.BonCommandeId = bc.Id;
            await _db.SaveChangesAsync();
        }

        return await GetBonCommandeByIdAsync(bc.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  BONS DE COMMANDE
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<BonCommandeDto>>> GetBonsCommandeAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20)
    {
        var query = _db.BonsCommande
            .Where(b => b.CabinetId == cabinetId)
            .Include(b => b.Client)
            .Include(b => b.Devis)
            .Include(b => b.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(b => b.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(b => b.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<BonCommandeDto>>.Ok(new PagedResult<BonCommandeDto>
        {
            Items = _mapper.Map<List<BonCommandeDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<BonCommandeDto>> GetBonCommandeByIdAsync(int id, int cabinetId)
    {
        var bc = await _db.BonsCommande
            .Where(b => b.Id == id && b.CabinetId == cabinetId)
            .Include(b => b.Client)
            .Include(b => b.Devis)
            .Include(b => b.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(b => b.Historique.OrderByDescending(h => h.DateAction))
            .FirstOrDefaultAsync();

        if (bc == null)
            return ApiResponse<BonCommandeDto>.Fail("Bon de commande introuvable.");

        return ApiResponse<BonCommandeDto>.Ok(_mapper.Map<BonCommandeDto>(bc));
    }

    public async Task<ApiResponse<BonCommandeDto>> ConfirmerBonCommandeAsync(int id, int cabinetId, string userId)
    {
        var bc = await _db.BonsCommande
            .Where(b => b.Id == id && b.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (bc == null)
            return ApiResponse<BonCommandeDto>.Fail("Bon de commande introuvable.");
        if (bc.Statut != RefCodes.StatutBonCommande.Brouillon)
            return ApiResponse<BonCommandeDto>.Fail("Seul un bon de commande en brouillon peut être confirmé.");

        var ancienStatut = bc.Statut;
        bc.Statut = RefCodes.StatutBonCommande.Confirme;
        bc.DateConfirmation = DateTime.UtcNow;
        bc.UpdatedBy = userId;
        bc.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueBc("BonCommande", bc, "Confirmation", ancienStatut, RefCodes.StatutBonCommande.Confirme, userId);

        await _db.SaveChangesAsync();
        return await GetBonCommandeByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<BonCommandeDto>> ApprouverBonCommandeAsync(int id, int cabinetId, string userId)
    {
        var bc = await _db.BonsCommande
            .Where(b => b.Id == id && b.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (bc == null)
            return ApiResponse<BonCommandeDto>.Fail("Bon de commande introuvable.");
        if (bc.Statut != RefCodes.StatutBonCommande.Confirme)
            return ApiResponse<BonCommandeDto>.Fail("Seul un bon de commande confirmé peut être approuvé.");

        var ancienStatut = bc.Statut;
        bc.Statut = RefCodes.StatutBonCommande.Approuve;
        bc.DateApprobation = DateTime.UtcNow;
        bc.UpdatedBy = userId;
        bc.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueBc("BonCommande", bc, "Approbation", ancienStatut, RefCodes.StatutBonCommande.Approuve, userId);

        await _db.SaveChangesAsync();
        return await GetBonCommandeByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<BonCommandeDto>> DupliquerBonCommandeAsync(int id, int cabinetId, string userId)
    {
        var source = await _db.BonsCommande
            .Where(b => b.Id == id && b.CabinetId == cabinetId)
            .Include(b => b.Lignes.OrderBy(l => l.Ordre))
            .FirstOrDefaultAsync();

        if (source == null)
            return ApiResponse<BonCommandeDto>.Fail("Bon de commande introuvable.");

        var reference = await GenerateReferenceAsync("PO", cabinetId);

        var copie = new BonCommande
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = source.ClientId,
            Objet = source.Objet != null ? $"{source.Objet} (copie)" : "Copie",
            Description = source.Description,
            Devise = source.Devise,
            TauxChange = source.TauxChange,
            DateEmission = DateTime.UtcNow,
            Statut = RefCodes.StatutBonCommande.Brouillon,
            ConditionsParticulieres = source.ConditionsParticulieres,
            Notes = source.Notes,
            MontantHT = source.MontantHT,
            MontantTVA = source.MontantTVA,
            MontantTTC = source.MontantTTC,
            MontantRemise = source.MontantRemise,
            Version = 1,
            CreatedBy = userId
        };

        foreach (var l in source.Lignes)
        {
            copie.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = l.MontantHT,
                MontantTVA = l.MontantTVA,
                MontantTTC = l.MontantTTC,
                CreatedBy = userId
            });
        }

        _db.BonsCommande.Add(copie);
        AddHistoriqueBc("BonCommande", copie, "Création", null, RefCodes.StatutBonCommande.Brouillon, userId, $"Dupliqué depuis {source.Reference}");
        await _db.SaveChangesAsync();

        return await GetBonCommandeByIdAsync(copie.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CONVERSION: BON DE COMMANDE → CONTRAT
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<ContratCommercialDto>> ConvertirBonCommandeEnContratAsync(ConvertBonCommandeEnContratDto dto, int cabinetId, string userId)
    {
        var bc = await _db.BonsCommande
            .Where(b => b.Id == dto.BonCommandeId && b.CabinetId == cabinetId)
            .Include(b => b.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(b => b.Contrat)
            .FirstOrDefaultAsync();

        if (bc == null)
            return ApiResponse<ContratCommercialDto>.Fail("Bon de commande introuvable.");
        if (bc.Statut != RefCodes.StatutBonCommande.Approuve)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un bon de commande approuvé peut être converti en contrat.");
        if (bc.Contrat != null)
            return ApiResponse<ContratCommercialDto>.Fail("Ce bon de commande a déjà été converti en contrat.");

        

        var reference = await GenerateReferenceAsync("CTR", cabinetId);

        var contrat = new ContratCommercial
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = bc.ClientId,
            BonCommandeId = bc.Id,
            Objet = bc.Objet,
            Description = bc.Description,
            Devise = bc.Devise,
            TauxChange = bc.TauxChange,
            DateEmission = DateTime.UtcNow,
            DateDebutContrat = dto.DateDebutContrat,
            DateFinContrat = dto.DateFinContrat,
            Statut = RefCodes.StatutContrat.Brouillon,
            ModeleFacturation = dto.ModeleFacturation,
            FrequenceFacturation = dto.FrequenceFacturation,
            ClausesSpecifiques = dto.ClausesSpecifiques,
            MontantHT = bc.MontantHT,
            MontantTVA = bc.MontantTVA,
            MontantTTC = bc.MontantTTC,
            MontantRemise = bc.MontantRemise,
            ConditionsParticulieres = bc.ConditionsParticulieres,
            ServicesActivesJson = dto.ServicesActives.Any()
                ? JsonSerializer.Serialize(dto.ServicesActives)
                : null,
            Version = 1,
            CreatedBy = userId
        };

        // Copier les lignes
        foreach (var ligneSource in bc.Lignes)
        {
            contrat.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = ligneSource.TypeLigne,
                TypeService = ligneSource.TypeService,
                CodeService = ligneSource.CodeService,
                ServiceContratId = ligneSource.ServiceContratId,
                Ordre = ligneSource.Ordre,
                Description = ligneSource.Description,
                DescriptionDetaillee = ligneSource.DescriptionDetaillee,
                Quantite = ligneSource.Quantite,
                Unite = ligneSource.Unite,
                PrixUnitaireHT = ligneSource.PrixUnitaireHT,
                RemisePourcentage = ligneSource.RemisePourcentage,
                RemiseMontant = ligneSource.RemiseMontant,
                TauxTVA = ligneSource.TauxTVA,
                MontantTVA = ligneSource.MontantTVA,
                MontantHT = ligneSource.MontantHT,
                MontantTTC = ligneSource.MontantTTC,
                ModeFacturation = ligneSource.ModeFacturation,
                LigneSourceId = ligneSource.Id,
                CreatedBy = userId
            });
        }

        _db.ContratsCommerciaux.Add(contrat);

        AddHistoriqueBc("BonCommande", bc, "Conversion", RefCodes.StatutBonCommande.Approuve, RefCodes.StatutBonCommande.Approuve, userId, $"Converti en Contrat {reference}");

        await _db.SaveChangesAsync();

        // Ajouter l'historique du contrat
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "Contrat",
            DocumentId = contrat.Id,
            ContratId = contrat.Id,
            Action = "Création",
            NouveauStatut = RefCodes.StatutContrat.Brouillon,
            UserId = userId,
            Commentaire = $"Créé depuis le Bon de Commande {bc.Reference}",
            DateAction = DateTime.UtcNow,
            VersionDocument = 1,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();

        return await GetContratByIdAsync(contrat.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CONVERSION DEVIS → CONTRAT (directe)
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<ContratCommercialDto>> ConvertirDevisEnContratAsync(ConvertDevisEnContratDto dto, int cabinetId, string userId)
    {
        if (dto == null)
            return ApiResponse<ContratCommercialDto>.Fail("Requête invalide : données de conversion manquantes.");

        var devis = await _db.Devis
            .Where(d => d.Id == dto.DevisId && d.CabinetId == cabinetId)
            .Include(d => d.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(d => d.ContratDirect)
            .FirstOrDefaultAsync();

        if (devis == null)
            return ApiResponse<ContratCommercialDto>.Fail("Devis introuvable.");
        if (devis.Statut != RefCodes.StatutDevis.Approuve)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un devis approuvé peut être converti en contrat.");
        if (devis.ContratDirect != null)
            return ApiResponse<ContratCommercialDto>.Fail("Ce devis a déjà été converti en contrat.");

        // Defensive check: ensure no contract exists for this devis (including soft-deleted records)
        var existsContract = await _db.ContratsCommerciaux
            .IgnoreQueryFilters()
            .AnyAsync(c => c.DevisId == devis.Id && c.CabinetId == cabinetId);
        if (existsContract)
            return ApiResponse<ContratCommercialDto>.Fail("Un contrat existe déjà pour ce devis.");

        var reference = await GenerateReferenceAsync("CTR", cabinetId);

        var contrat = new ContratCommercial
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = devis.ClientId,
            DevisId = devis.Id,
            Objet = devis.Objet,
            Description = devis.Description,
            Devise = devis.Devise,
            TauxChange = devis.TauxChange,
            DateEmission = DateTime.UtcNow,
            DateDebutContrat = dto.DateDebutContrat,
            DateFinContrat = dto.DateFinContrat,
            Statut = RefCodes.StatutContrat.Brouillon,
            ModeleFacturation = dto.ModeleFacturation,
            FrequenceFacturation = dto.FrequenceFacturation,
            ClausesSpecifiques = dto.ClausesSpecifiques,
            MontantHT = devis.MontantHT,
            MontantTVA = devis.MontantTVA,
            MontantTTC = devis.MontantTTC,
            MontantRemise = devis.MontantRemise,
            ConditionsParticulieres = devis.ConditionsParticulieres,
            Version = 1,
            Notes = dto.Notes,
            ServicesActivesJson = dto.ServicesActives.Any()
                ? JsonSerializer.Serialize(dto.ServicesActives)
                : null,
            CreatedBy = userId
        };

        foreach (var ligneSource in devis.Lignes)
        {
            contrat.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = ligneSource.TypeLigne,
                TypeService = ligneSource.TypeService,
                CodeService = ligneSource.CodeService,
                ServiceContratId = ligneSource.ServiceContratId,
                Ordre = ligneSource.Ordre,
                Description = ligneSource.Description,
                DescriptionDetaillee = ligneSource.DescriptionDetaillee,
                Quantite = ligneSource.Quantite,
                Unite = ligneSource.Unite,
                PrixUnitaireHT = ligneSource.PrixUnitaireHT,
                RemisePourcentage = ligneSource.RemisePourcentage,
                RemiseMontant = ligneSource.RemiseMontant,
                TauxTVA = ligneSource.TauxTVA,
                MontantTVA = ligneSource.MontantTVA,
                MontantHT = ligneSource.MontantHT,
                MontantTTC = ligneSource.MontantTTC,
                ModeFacturation = ligneSource.ModeFacturation,
                LigneSourceId = ligneSource.Id,
                CreatedBy = userId
            });
        }

        _db.ContratsCommerciaux.Add(contrat);
        await _db.SaveChangesAsync();

        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "Contrat",
            DocumentId = contrat.Id,
            ContratId = contrat.Id,
            Action = "Création",
            NouveauStatut = RefCodes.StatutContrat.Brouillon,
            UserId = userId,
            Commentaire = $"Créé depuis le Devis {devis.Reference} (conversion directe)",
            DateAction = DateTime.UtcNow,
            VersionDocument = 1,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();

        return await GetContratByIdAsync(contrat.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CRÉATION DIRECTE DE CONTRAT
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<ContratCommercialDto>> CreateContratAsync(ContratCommercialCreateDto dto, int cabinetId, string userId)
    {
        var validityError = ValidateContratValidity(dto);
        if (validityError != null)
            return ApiResponse<ContratCommercialDto>.Fail(validityError);

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (!clientExists)
            return ApiResponse<ContratCommercialDto>.Fail("Client introuvable.");

        var reference = await GenerateReferenceAsync("CTR", cabinetId);

        var contrat = new ContratCommercial
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = dto.ClientId,
            Objet = dto.Objet,
            Description = dto.Description,
            Devise = dto.Devise,
            DateEmission = DateTime.UtcNow,
            DateDebutContrat = dto.DateDebutContrat,
            DateFinContrat = dto.DateFinContrat,
            Statut = RefCodes.StatutContrat.Brouillon,
            ModeleFacturation = dto.ModeleFacturation,
            FrequenceFacturation = dto.FrequenceFacturation,
            ClausesSpecifiques = dto.ClausesSpecifiques,
            ConditionsParticulieres = dto.ConditionsParticulieres,
            Notes = dto.Notes,
            ResponsableId = null,
            ServicesActivesJson = dto.ServicesActives.Any()
                ? JsonSerializer.Serialize(dto.ServicesActives)
                : null,
            Version = 1,
            CreatedBy = userId
        };

        AddContratMembres(contrat, ResolveMembresInput(dto), cabinetId, userId);

        decimal totalHT = 0, totalTVA = 0, totalTTC = 0, totalRemise = 0;
        int ordre = 1;
        foreach (var l in dto.Lignes)
        {
            var (montantHT, montantTVA, montantTTC, remise) = CalculerLigneHonoraires(l);
            totalRemise += remise;

            contrat.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre > 0 ? l.Ordre : ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite <= 0 ? 1 : l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = montantHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC,
                ModeFacturation = l.ModeFacturation,
                CreatedBy = userId
            });

            totalHT += montantHT;
            totalTVA += montantTVA;
            totalTTC += montantTTC;
            ordre++;
        }

        contrat.MontantHT = totalHT;
        contrat.MontantTVA = totalTVA;
        contrat.MontantTTC = totalTTC;
        contrat.MontantRemise = totalRemise;

        _db.ContratsCommerciaux.Add(contrat);
        await _db.SaveChangesAsync();

        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "Contrat",
            DocumentId = contrat.Id,
            ContratId = contrat.Id,
            Action = "Création",
            NouveauStatut = RefCodes.StatutContrat.Brouillon,
            UserId = userId,
            Commentaire = "Contrat créé directement",
            DateAction = DateTime.UtcNow,
            VersionDocument = 1,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();

        return await GetContratByIdAsync(contrat.Id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> UpdateContratAsync(int id, ContratCommercialCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<ContratCommercialDto>.Fail("Le client est requis.");

        var validityError = ValidateContratValidity(dto);
        if (validityError != null)
            return ApiResponse<ContratCommercialDto>.Fail(validityError);

        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.EstSigne)
            return ApiResponse<ContratCommercialDto>.Fail("Un contrat signé ne peut plus être modifié.");
        if (contrat.Statut != RefCodes.StatutContrat.Brouillon && contrat.Statut != RefCodes.StatutContrat.EnCours)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un contrat en Brouillon ou EnCours peut être modifié.");

        // Mise à jour des champs
        contrat.ClientId = dto.ClientId;
        contrat.Objet = dto.Objet;
        contrat.Description = dto.Description;
        contrat.Devise = dto.Devise;
        contrat.DateDebutContrat = dto.DateDebutContrat;
        contrat.DateFinContrat = dto.DateFinContrat;
        contrat.ModeleFacturation = dto.ModeleFacturation;
        contrat.FrequenceFacturation = dto.FrequenceFacturation;
        contrat.ClausesSpecifiques = dto.ClausesSpecifiques;
        contrat.ConditionsParticulieres = dto.ConditionsParticulieres;
        contrat.Notes = dto.Notes;
        contrat.ResponsableId = null;
        contrat.Version++;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;
        contrat.ServicesActivesJson = dto.ServicesActives.Any()
            ? JsonSerializer.Serialize(dto.ServicesActives)
            : null;

        await SyncContratMembresAsync(contrat.Id, ResolveMembresInput(dto), cabinetId, userId);

        // Supprimer les anciennes lignes et recréer
        _db.LignesDocumentsCommerciaux.RemoveRange(contrat.Lignes);

        decimal totalHT = 0, totalTVA = 0, totalTTC = 0, totalRemise = 0;
        int ordre = 1;
        foreach (var l in dto.Lignes)
        {
            var (montantHT, montantTVA, montantTTC, remise) = CalculerLigneHonoraires(l);
            totalRemise += remise;

            contrat.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre > 0 ? l.Ordre : ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite <= 0 ? 1 : l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = montantHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC,
                ModeFacturation = l.ModeFacturation,
                CreatedBy = userId
            });

            totalHT += montantHT;
            totalTVA += montantTVA;
            totalTTC += montantTTC;
            ordre++;
        }

        contrat.MontantHT = totalHT;
        contrat.MontantTVA = totalTVA;
        contrat.MontantTTC = totalTTC;
        contrat.MontantRemise = totalRemise;

        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "Contrat",
            DocumentId = contrat.Id,
            ContratId = contrat.Id,
            Action = "Modification",
            AncienStatut = contrat.Statut,
            NouveauStatut = contrat.Statut,
            UserId = userId,
            Commentaire = $"Contrat modifié (v{contrat.Version})",
            DateAction = DateTime.UtcNow,
            VersionDocument = contrat.Version,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(contrat.Id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> UpdateContratEquipeAsync(int id, ContratEquipeUpdateDto dto, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.Statut == RefCodes.StatutContrat.Termine || contrat.Statut == RefCodes.StatutContrat.Resilie
            || contrat.Statut == RefCodes.StatutContrat.Annule)
            return ApiResponse<ContratCommercialDto>.Fail("Impossible de modifier l'équipe d'un contrat clôturé.");

        var membres = NormalizeMembresInput(dto.Membres);
        if (membres.Count == 0)
            return ApiResponse<ContratCommercialDto>.Fail("L'équipe du dossier doit comporter au moins un collaborateur.");

        contrat.ResponsableId = null;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        await SyncContratMembresAsync(contrat.Id, membres, cabinetId, userId);

        AddHistoriqueContrat("Contrat", contrat, "Équipe", contrat.Statut, contrat.Statut, userId,
            "Équipe du dossier mise à jour");

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(contrat.Id, cabinetId);
    }

    public async Task<ApiResponse<PagedResult<ContratCommercialDto>>> GetContratsAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20)
    {
        var query = _db.ContratsCommerciaux
            .Where(c => c.CabinetId == cabinetId)
            .Include(c => c.Client)
            .Include(c => c.BonCommande)
            .Include(c => c.Devis)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .Include(c => c.Factures)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(c => c.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(c => c.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<ContratCommercialDto>>.Ok(new PagedResult<ContratCommercialDto>
        {
            Items = _mapper.Map<List<ContratCommercialDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<ContratCommercialDto>> GetContratByIdAsync(int id, int cabinetId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Client)
            .Include(c => c.BonCommande)
            .Include(c => c.Devis)
            .Include(c => c.ParentContrat)
            .Include(c => c.RemplaceParContrat)
            .Include(c => c.Responsable)
            .Include(c => c.Membres)
                .ThenInclude(m => m.User)
            .Include(c => c.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(c => c.Factures)
            .Include(c => c.Historique.OrderByDescending(h => h.DateAction))
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");

        return ApiResponse<ContratCommercialDto>.Ok(_mapper.Map<ContratCommercialDto>(contrat));
    }

    public async Task<ApiResponse<ContratCommercialDto>> SignerContratAsync(int id, ContratSignatureDto dto, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.Statut != RefCodes.StatutContrat.Brouillon && contrat.Statut != RefCodes.StatutContrat.EnCours)
            return ApiResponse<ContratCommercialDto>.Fail("Le contrat doit être en brouillon ou en cours pour être signé.");

        var ancienStatut = contrat.Statut;
        contrat.EstSigne = true;
        contrat.SignataireNom = dto.SignataireNom;
        contrat.SignataireFonction = dto.SignataireFonction;
        contrat.SignatureUrl = dto.SignatureUrl;
        contrat.DateSignature = DateTime.UtcNow;
        contrat.Statut = RefCodes.StatutContrat.Signe;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueContrat("Contrat", contrat, "Signature", ancienStatut, RefCodes.StatutContrat.Signe, userId,
            $"Signé par {dto.SignataireNom}");

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> ValiderContratAsync(int id, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");

        // ⚠️ RÈGLE STRICTE : Le contrat DOIT être signé avant validation
        if (!contrat.EstSigne)
            return ApiResponse<ContratCommercialDto>.Fail("Le contrat doit être signé avant de pouvoir être validé.");
        if (contrat.Statut != RefCodes.StatutContrat.Signe)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un contrat signé peut être validé.");
        if (!await _db.ContratMembres.AnyAsync(m => m.ContratId == contrat.Id))
            return ApiResponse<ContratCommercialDto>.Fail("L'équipe du dossier est obligatoire avant validation.");

        // Rectificatif may overlap its parent — parent is superseded on validate
        var excludeIds = new List<int> { contrat.Id };
        if (contrat.ParentContratId is int parentExcl)
            excludeIds.Add(parentExcl);

        // Multiple active contracts are allowed for the same client when they cover different services.
        var conflict = await FindOverlappingActiveContratConflictAsync(
            contrat.ClientId, cabinetId, contrat.DateDebutContrat, contrat.DateFinContrat,
            GetContratServiceCodes(contrat), excludeContratIds: excludeIds);
        if (conflict != null)
            return ApiResponse<ContratCommercialDto>.Fail(conflict);

        var ancienStatut = contrat.Statut;
        contrat.Statut = RefCodes.StatutContrat.Valide;
        contrat.DateValidation = DateTime.UtcNow;
        contrat.ValidateurId = userId;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueContrat("Contrat", contrat, "Validation", ancienStatut, RefCodes.StatutContrat.Valide, userId);

        // Supersede original contract when validating a rectificatif
        if (contrat.ParentContratId is int parentId)
        {
            var parent = await _db.ContratsCommerciaux
                .FirstOrDefaultAsync(c => c.Id == parentId && c.CabinetId == cabinetId);
            if (parent != null
                && parent.Statut != RefCodes.StatutContrat.Remplace
                && parent.Statut != RefCodes.StatutContrat.Termine
                && parent.Statut != RefCodes.StatutContrat.Annule)
            {
                var parentOld = parent.Statut;
                parent.Statut = RefCodes.StatutContrat.Remplace;
                parent.RemplaceParContratId = contrat.Id;
                parent.UpdatedBy = userId;
                parent.UpdatedAt = DateTime.UtcNow;
                AddHistoriqueContrat("Contrat", parent, "Remplacé", parentOld, RefCodes.StatutContrat.Remplace, userId,
                    $"Remplacé par rectificatif {contrat.Reference}");
            }
        }

        // ⚠️ AUTO-ACTIVATION DES SERVICES : Créer/activer les services du client
        var activateErr = await ActiverServicesFromContratAsync(contrat, cabinetId, userId);
        if (activateErr != null)
            return ApiResponse<ContratCommercialDto>.Fail(activateErr);

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> SuspendreContratAsync(int id, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.Statut != RefCodes.StatutContrat.Valide && contrat.Statut != RefCodes.StatutContrat.Actif)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un contrat validé ou actif peut être suspendu.");

        var ancienStatut = contrat.Statut;
        contrat.Statut = RefCodes.StatutContrat.Suspendu;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueContrat("Contrat", contrat, "Suspension", ancienStatut, RefCodes.StatutContrat.Suspendu, userId);

        await DesactiverServicesFromContratAsync(contrat);

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> ReactiverContratAsync(int id, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.Statut != RefCodes.StatutContrat.Suspendu)
            return ApiResponse<ContratCommercialDto>.Fail("Seul un contrat suspendu peut être réactivé.");
        if (!await _db.ContratMembres.AnyAsync(m => m.ContratId == contrat.Id))
            return ApiResponse<ContratCommercialDto>.Fail("L'équipe du dossier est obligatoire avant réactivation.");

        var conflict = await FindOverlappingActiveContratConflictAsync(
            contrat.ClientId, cabinetId, contrat.DateDebutContrat, contrat.DateFinContrat,
            GetContratServiceCodes(contrat), excludeContratIds: new[] { contrat.Id });
        if (conflict != null)
            return ApiResponse<ContratCommercialDto>.Fail(conflict);

        var ancienStatut = contrat.Statut;
        contrat.Statut = RefCodes.StatutContrat.Valide;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueContrat("Contrat", contrat, "Réactivation", ancienStatut, RefCodes.StatutContrat.Valide, userId);

        var activateErr = await ActiverServicesFromContratAsync(contrat, cabinetId, userId);
        if (activateErr != null)
            return ApiResponse<ContratCommercialDto>.Fail(activateErr);

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> TerminerContratAsync(int id, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");
        if (contrat.Statut != RefCodes.StatutContrat.Valide && contrat.Statut != RefCodes.StatutContrat.Actif && contrat.Statut != RefCodes.StatutContrat.Suspendu)
            return ApiResponse<ContratCommercialDto>.Fail("Le contrat ne peut pas être terminé dans cet état.");

        var ancienStatut = contrat.Statut;
        contrat.Statut = RefCodes.StatutContrat.Termine;
        contrat.UpdatedBy = userId;
        contrat.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueContrat("Contrat", contrat, "Clôture", ancienStatut, RefCodes.StatutContrat.Termine, userId);

        await DesactiverServicesFromContratAsync(contrat);

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<int>> SyncServicesFromContratAsync(int contratId, int cabinetId, string userId)
    {
        var contrat = await _db.ContratsCommerciaux
            .Where(c => c.Id == contratId && c.CabinetId == cabinetId)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .FirstOrDefaultAsync();

        if (contrat == null)
            return ApiResponse<int>.Fail("Contrat introuvable.");

        if (contrat.Statut != RefCodes.StatutContrat.Valide && contrat.Statut != RefCodes.StatutContrat.Actif)
            return ApiResponse<int>.Fail("Seul un contrat validé ou actif peut être synchronisé.");

        var lignesAvant = contrat.Lignes.Count(l => !string.IsNullOrWhiteSpace(l.TypeService));
        var activateErr = await ActiverServicesFromContratAsync(contrat, cabinetId, userId);
        if (activateErr != null)
            return ApiResponse<int>.Fail(activateErr);
        await _db.SaveChangesAsync();

        return ApiResponse<int>.Ok(lignesAvant, $"{lignesAvant} service(s) synchronisé(s) depuis le contrat #{contrat.Reference}.");
    }

    public async Task<ApiResponse<ContratCommercialDto>> DupliquerContratAsync(int id, int cabinetId, string userId)
    {
        var source = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Lignes.OrderBy(l => l.Ordre))
            .FirstOrDefaultAsync();

        if (source == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");

        var reference = await GenerateReferenceAsync("CTR", cabinetId);

        var copie = new ContratCommercial
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = source.ClientId,
            Objet = source.Objet != null ? $"{source.Objet} (copie)" : "Copie",
            Description = source.Description,
            Devise = source.Devise,
            DateEmission = DateTime.UtcNow,
            DateDebutContrat = source.DateDebutContrat,
            DateFinContrat = source.DateFinContrat,
            Statut = RefCodes.StatutContrat.Brouillon,
            ModeleFacturation = source.ModeleFacturation,
            FrequenceFacturation = source.FrequenceFacturation,
            ClausesSpecifiques = source.ClausesSpecifiques,
            ConditionsParticulieres = source.ConditionsParticulieres,
            Notes = source.Notes,
            MontantHT = source.MontantHT,
            MontantTVA = source.MontantTVA,
            MontantTTC = source.MontantTTC,
            MontantRemise = source.MontantRemise,
            ServicesActivesJson = source.ServicesActivesJson,
            Version = 1,
            CreatedBy = userId
        };

        foreach (var l in source.Lignes)
        {
            copie.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = l.MontantHT,
                MontantTVA = l.MontantTVA,
                MontantTTC = l.MontantTTC,
                ModeFacturation = l.ModeFacturation,
                CreatedBy = userId
            });
        }

        _db.ContratsCommerciaux.Add(copie);

        AddHistoriqueContrat("Contrat", copie, "Création", null, RefCodes.StatutContrat.Brouillon, userId, $"Dupliqué depuis {source.Reference}");
        await _db.SaveChangesAsync();

        return await GetContratByIdAsync(copie.Id, cabinetId);
    }

    public async Task<ApiResponse<ContratCommercialDto>> CreateRectificatifContratAsync(
        int id, int cabinetId, string userId, string? motif = null)
    {
        var source = await _db.ContratsCommerciaux
            .Where(c => c.Id == id && c.CabinetId == cabinetId)
            .Include(c => c.Lignes.OrderBy(l => l.Ordre))
            .Include(c => c.Membres)
            .FirstOrDefaultAsync();

        if (source == null)
            return ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable.");

        var allowed = source.Statut is RefCodes.StatutContrat.Valide
            or RefCodes.StatutContrat.Actif
            or RefCodes.StatutContrat.Signe
            or RefCodes.StatutContrat.Suspendu;
        if (!allowed || source.RemplaceParContratId != null)
            return ApiResponse<ContratCommercialDto>.Fail(
                "Seul un contrat signé, validé, actif ou suspendu (non déjà remplacé) peut être rectifié.");

        // Reuse open draft rectificatif if one already exists
        var existingDraft = await _db.ContratsCommerciaux
            .Where(c => c.ParentContratId == source.Id && c.CabinetId == cabinetId
                && (c.Statut == RefCodes.StatutContrat.Brouillon
                    || c.Statut == RefCodes.StatutContrat.EnCours
                    || c.Statut == RefCodes.StatutContrat.Signe))
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();
        if (existingDraft != null)
            return await GetContratByIdAsync(existingDraft.Id, cabinetId);

        var reference = await GenerateReferenceAsync("CTR", cabinetId);
        var nextVersion = source.Version + 1;
        var motifClean = string.IsNullOrWhiteSpace(motif) ? null : motif.Trim();

        var rectif = new ContratCommercial
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = source.ClientId,
            BonCommandeId = source.BonCommandeId,
            DevisId = source.DevisId,
            Objet = source.Objet != null ? $"{source.Objet} (rectificatif)" : "Rectificatif",
            Description = source.Description,
            Devise = source.Devise,
            TauxChange = source.TauxChange,
            DateEmission = DateTime.UtcNow,
            DateDebutContrat = source.DateDebutContrat,
            DateFinContrat = source.DateFinContrat,
            Statut = RefCodes.StatutContrat.Brouillon,
            EstSigne = false,
            ModeleFacturation = source.ModeleFacturation,
            FrequenceFacturation = source.FrequenceFacturation,
            ClausesSpecifiques = source.ClausesSpecifiques,
            ConditionsParticulieres = source.ConditionsParticulieres,
            Notes = source.Notes,
            MontantHT = source.MontantHT,
            MontantTVA = source.MontantTVA,
            MontantTTC = source.MontantTTC,
            MontantRemise = source.MontantRemise,
            ServicesActivesJson = source.ServicesActivesJson,
            ParentContratId = source.Id,
            MotifRectificatif = motifClean,
            Version = nextVersion,
            ResponsableId = source.ResponsableId,
            CreatedBy = userId
        };

        foreach (var l in source.Lignes)
        {
            rectif.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = l.MontantHT,
                MontantTVA = l.MontantTVA,
                MontantTTC = l.MontantTTC,
                ModeFacturation = l.ModeFacturation,
                CreatedBy = userId
            });
        }

        foreach (var m in source.Membres)
        {
            rectif.Membres.Add(new ContratMembre
            {
                CabinetId = cabinetId,
                UserId = m.UserId,
                RoleLabel = m.RoleLabel,
                CreatedBy = userId
            });
        }

        _db.ContratsCommerciaux.Add(rectif);
        AddHistoriqueContrat("Contrat", rectif, "Rectificatif", null, RefCodes.StatutContrat.Brouillon, userId,
            $"Rectificatif de {source.Reference}" + (motifClean != null ? $" — {motifClean}" : ""));
        AddHistoriqueContrat("Contrat", source, "RectificatifCréé", source.Statut, source.Statut, userId,
            $"Rectificatif {reference} créé (v{nextVersion})");

        await _db.SaveChangesAsync();
        return await GetContratByIdAsync(rectif.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FACTURES
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<FactureDto>>> GetFacturesAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Factures
            .Where(f => f.CabinetId == cabinetId)
            .Include(f => f.Client)
            .Include(f => f.Contrat)
            .Include(f => f.Lignes)
                .ThenInclude(l => l.ServiceContrat)
            .Include(f => f.Paiements)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(f => f.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(f => f.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<FactureDto>>.Ok(new PagedResult<FactureDto>
        {
            Items = _mapper.Map<List<FactureDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<FactureDto>> GetFactureByIdAsync(int id, int cabinetId)
    {
        var facture = await _db.Factures
            .Where(f => f.Id == id && f.CabinetId == cabinetId)
            .Include(f => f.Client)
            .Include(f => f.Contrat)
            .Include(f => f.Lignes.OrderBy(l => l.Ordre))
                .ThenInclude(l => l.ServiceContrat)
            .Include(f => f.Paiements.OrderByDescending(p => p.DatePaiement))
            .Include(f => f.Historique.OrderByDescending(h => h.DateAction))
            .FirstOrDefaultAsync();

        if (facture == null)
            return ApiResponse<FactureDto>.Fail("Facture introuvable.");

        return ApiResponse<FactureDto>.Ok(_mapper.Map<FactureDto>(facture));
    }

    public async Task<ApiResponse<FactureDto>> CreateFactureAsync(FactureCreateDto dto, int cabinetId, string userId)
    {
        ContratCommercial? contrat = null;
        int clientId;
        string devise = "MAD";
        decimal? tauxChange = null;
        string commentaire;

        if (dto.ContratId.HasValue && dto.ContratId.Value > 0)
        {
            // ── Facture depuis un contrat ──
            contrat = await _db.ContratsCommerciaux
                .Where(c => c.Id == dto.ContratId.Value && c.CabinetId == cabinetId)
                .Include(c => c.Lignes)
                    .ThenInclude(l => l.ServiceContrat)
                .FirstOrDefaultAsync();

            if (contrat == null)
                return ApiResponse<FactureDto>.Fail("Contrat introuvable.");
            if (contrat.Statut != RefCodes.StatutContrat.Valide && contrat.Statut != RefCodes.StatutContrat.Actif)
                return ApiResponse<FactureDto>.Fail("Le contrat doit être validé ou actif pour générer une facture.");

            clientId = contrat.ClientId;
            devise = contrat.Devise;
            tauxChange = contrat.TauxChange;
            commentaire = $"Créée depuis le Contrat {contrat.Reference}";
        }
        else if (dto.ClientId.HasValue && dto.ClientId.Value > 0)
        {
            // ── Facture manuelle (sans contrat) ──
            var clientExists = await _db.Clients.AnyAsync(c => c.Id == dto.ClientId.Value && c.CabinetId == cabinetId);
            if (!clientExists)
                return ApiResponse<FactureDto>.Fail("Client introuvable.");
            clientId = dto.ClientId.Value;
            commentaire = "Facture créée manuellement";
        }
        else
        {
            return ApiResponse<FactureDto>.Fail("Un contrat ou un client doit être spécifié.");
        }

        var reference = await GenerateReferenceAsync("INV", cabinetId);

        var facture = new Facture
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = clientId,
            ContratId = contrat?.Id,
            Objet = dto.Objet ?? contrat?.Objet,
            Devise = devise,
            TauxChange = tauxChange,
            PeriodeDebut = dto.PeriodeDebut,
            PeriodeFin = dto.PeriodeFin,
            DateEmission = DateTime.UtcNow,
            DateEcheance = dto.DateEcheance ?? DateTime.UtcNow.AddDays(30),
            Statut = RefCodes.StatutFacture.Brouillon,
            Version = 1,
            EmetteurId = userId,
            Notes = dto.Notes,
            CreatedBy = userId
        };

        // Ajouter les lignes depuis le contrat (si contrat et LigneContratIds spécifiés)
        if (contrat != null)
        {
            var lignesContrat = dto.LigneContratIds != null && dto.LigneContratIds.Any()
                ? contrat.Lignes.Where(l => dto.LigneContratIds.Contains(l.Id)).ToList()
                : contrat.Lignes.ToList();

            foreach (var ligneSource in lignesContrat)
            {
                facture.Lignes.Add(new LigneDocumentCommercial
                {
                    CabinetId = cabinetId,
                    TypeLigne = ligneSource.TypeLigne,
                    TypeService = ligneSource.TypeService,
                    CodeService = ligneSource.CodeService,
                    ServiceContratId = ligneSource.ServiceContratId,
                    Ordre = ligneSource.Ordre,
                    Description = ligneSource.Description,
                    DescriptionDetaillee = ligneSource.DescriptionDetaillee,
                    Quantite = ligneSource.Quantite,
                    Unite = ligneSource.Unite,
                    PrixUnitaireHT = ligneSource.PrixUnitaireHT,
                    RemisePourcentage = ligneSource.RemisePourcentage,
                    RemiseMontant = ligneSource.RemiseMontant,
                    TauxTVA = ligneSource.TauxTVA,
                    MontantTVA = ligneSource.MontantTVA,
                    MontantHT = ligneSource.MontantHT,
                    MontantTTC = ligneSource.MontantTTC,
                    LigneSourceId = ligneSource.Id,
                    CreatedBy = userId
                });
            }
        }

        // Ajouter les lignes manuelles
        if (dto.Lignes.Any())
        {
            foreach (var ligneDto in dto.Lignes)
            {
                var ligne = _mapper.Map<LigneDocumentCommercial>(ligneDto);
                ligne.CabinetId = cabinetId;
                ligne.CreatedBy = userId;
                CalculerLigne(ligne);
                facture.Lignes.Add(ligne);
            }
        }

        RecalculerTotauxFacture(facture);

        _db.Factures.Add(facture);

        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = cabinetId,
            TypeDocument = "Facture",
            DocumentId = 0,
            Action = "Création",
            NouveauStatut = RefCodes.StatutFacture.Brouillon,
            UserId = userId,
            Commentaire = commentaire,
            DateAction = DateTime.UtcNow,
            VersionDocument = 1,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        // Mettre à jour l'historique avec le bon ID
        var lastHist = await _db.DocumentsCommerciauxHistorique
            .Where(h => h.TypeDocument == "Facture" && h.DocumentId == 0 && h.CabinetId == cabinetId)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync();
        if (lastHist != null)
        {
            lastHist.DocumentId = facture.Id;
            lastHist.FactureId = facture.Id;
            await _db.SaveChangesAsync();
        }

        return await GetFactureByIdAsync(facture.Id, cabinetId);
    }

    public async Task<ApiResponse<FactureDto>> EnvoyerFactureAsync(int id, int cabinetId, string userId)
    {
        var facture = await _db.Factures
            .Where(f => f.Id == id && f.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (facture == null)
            return ApiResponse<FactureDto>.Fail("Facture introuvable.");
        if (facture.Statut != RefCodes.StatutFacture.Brouillon && facture.Statut != RefCodes.StatutFacture.Generee)
            return ApiResponse<FactureDto>.Fail("La facture ne peut pas être envoyée dans cet état.");

        var ancienStatut = facture.Statut;
        facture.Statut = RefCodes.StatutFacture.Envoyee;
        facture.DateEnvoi = DateTime.UtcNow;
        facture.UpdatedBy = userId;
        facture.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueFacture("Facture", facture, "Envoi", ancienStatut, RefCodes.StatutFacture.Envoyee, userId);

        await _db.SaveChangesAsync();
        return await GetFactureByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<FactureDto>> AnnulerFactureAsync(int id, int cabinetId, string userId)
    {
        var facture = await _db.Factures
            .Where(f => f.Id == id && f.CabinetId == cabinetId)
            .FirstOrDefaultAsync();

        if (facture == null)
            return ApiResponse<FactureDto>.Fail("Facture introuvable.");
        if (facture.Statut == RefCodes.StatutFacture.Payee || facture.Statut == RefCodes.StatutFacture.Annulee)
            return ApiResponse<FactureDto>.Fail("Cette facture ne peut pas être annulée.");

        var ancienStatut = facture.Statut;
        facture.Statut = RefCodes.StatutFacture.Annulee;
        facture.UpdatedBy = userId;
        facture.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueFacture("Facture", facture, "Annulation", ancienStatut, RefCodes.StatutFacture.Annulee, userId);

        await _db.SaveChangesAsync();
        return await GetFactureByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<FactureDto>> DupliquerFactureAsync(int id, int cabinetId, string userId)
    {
        var source = await _db.Factures
            .Where(f => f.Id == id && f.CabinetId == cabinetId)
            .Include(f => f.Lignes.OrderBy(l => l.Ordre))
            .FirstOrDefaultAsync();

        if (source == null)
            return ApiResponse<FactureDto>.Fail("Facture introuvable.");

        var reference = await GenerateReferenceAsync("INV", cabinetId);

        var copie = new Facture
        {
            CabinetId = cabinetId,
            Reference = reference,
            ClientId = source.ClientId,
            ContratId = source.ContratId,
            Objet = source.Objet != null ? $"{source.Objet} (copie)" : "Copie",
            Devise = source.Devise,
            DateEmission = DateTime.UtcNow,
            DateEcheance = DateTime.UtcNow.AddDays(30),
            Statut = RefCodes.StatutFacture.Brouillon,
            MontantHT = source.MontantHT,
            MontantTVA = source.MontantTVA,
            MontantTTC = source.MontantTTC,
            MontantRemise = source.MontantRemise,
            MontantRestant = source.MontantTTC,
            Notes = source.Notes,
            Version = 1,
            CreatedBy = userId
        };

        foreach (var l in source.Lignes)
        {
            copie.Lignes.Add(new LigneDocumentCommercial
            {
                CabinetId = cabinetId,
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                MontantHT = l.MontantHT,
                MontantTVA = l.MontantTVA,
                MontantTTC = l.MontantTTC,
                CreatedBy = userId
            });
        }

        _db.Factures.Add(copie);
        AddHistoriqueFacture("Facture", copie, "Création", null, RefCodes.StatutFacture.Brouillon, userId, $"Dupliquée depuis {source.Reference}");
        await _db.SaveChangesAsync();

        return await GetFactureByIdAsync(copie.Id, cabinetId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PAIEMENTS
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<PaiementDto>> EnregistrerPaiementAsync(PaiementCreateDto dto, int cabinetId, string userId)
    {
        var facture = await _db.Factures
            .Where(f => f.Id == dto.FactureId && f.CabinetId == cabinetId)
            .Include(f => f.Paiements)
            .FirstOrDefaultAsync();

        if (facture == null)
            return ApiResponse<PaiementDto>.Fail("Facture introuvable.");
        if (facture.Statut == RefCodes.StatutFacture.Annulee || facture.Statut == RefCodes.StatutFacture.Remboursee)
            return ApiResponse<PaiementDto>.Fail("Impossible d'enregistrer un paiement sur cette facture.");
        if (dto.Montant <= 0)
            return ApiResponse<PaiementDto>.Fail("Le montant du paiement doit être positif.");
        if (dto.Montant > facture.MontantRestant)
            return ApiResponse<PaiementDto>.Fail($"Le montant dépasse le restant dû ({facture.MontantRestant:N2} {facture.Devise}).");

        var reference = await GenerateReferenceAsync("PAY", cabinetId);

        var paiement = _mapper.Map<Paiement>(dto);
        paiement.CabinetId = cabinetId;
        paiement.Reference = reference;
        paiement.DatePaiement = dto.DatePaiement ?? DateTime.UtcNow;
        paiement.Devise = facture.Devise;
        paiement.Statut = RefCodes.StatutPaiement.Valide;
        paiement.EnregistreParId = userId;
        paiement.CreatedBy = userId;

        _db.Paiements.Add(paiement);

        // Mettre à jour les montants de la facture
        facture.MontantPaye += paiement.Montant;
        facture.MontantRestant = facture.MontantTTC - facture.MontantPaye;

        // Mettre à jour le statut de la facture
        var ancienStatut = facture.Statut;
        if (facture.MontantRestant <= 0)
        {
            facture.Statut = RefCodes.StatutFacture.Payee;
            facture.MontantRestant = 0;
        }
        else
        {
            facture.Statut = "PartiellementPayee";
        }

        facture.UpdatedBy = userId;
        facture.UpdatedAt = DateTime.UtcNow;

        AddHistoriqueFacture("Facture", facture, "Paiement", ancienStatut, facture.Statut, userId,
            $"Paiement de {paiement.Montant:N2} {paiement.Devise} par {paiement.ModePaiement}");

        await _db.SaveChangesAsync();

        var result = _mapper.Map<PaiementDto>(paiement);
        result.FactureReference = facture.Reference;
        return ApiResponse<PaiementDto>.Ok(result);
    }

    public async Task<ApiResponse<List<PaiementDto>>> GetPaiementsFactureAsync(int factureId, int cabinetId)
    {
        var paiements = await _db.Paiements
            .Where(p => p.FactureId == factureId && p.CabinetId == cabinetId)
            .Include(p => p.Facture)
            .OrderByDescending(p => p.DatePaiement)
            .ToListAsync();

        return ApiResponse<List<PaiementDto>>.Ok(_mapper.Map<List<PaiementDto>>(paiements));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DASHBOARD COMMERCIAL
    // ════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<DashboardCommercialDto>> GetDashboardAsync(int cabinetId)
    {
        var now = DateTime.UtcNow;
        var debutMois = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var dashboard = new DashboardCommercialDto
        {
            TotalDevis = await _db.Devis.CountAsync(d => d.CabinetId == cabinetId),
            DevisEnAttente = await _db.Devis.CountAsync(d => d.CabinetId == cabinetId && d.Statut == RefCodes.StatutDevis.Soumis),
            TotalContrats = await _db.ContratsCommerciaux.CountAsync(c => c.CabinetId == cabinetId),
            ContratsActifs = await _db.ContratsCommerciaux.CountAsync(c => c.CabinetId == cabinetId && (c.Statut == RefCodes.StatutContrat.Valide || c.Statut == RefCodes.StatutContrat.Actif)),
            TotalFactures = await _db.Factures.CountAsync(f => f.CabinetId == cabinetId),
            FacturesEnRetard = await _db.Factures.CountAsync(f => f.CabinetId == cabinetId && f.Statut != RefCodes.StatutFacture.Payee && f.Statut != RefCodes.StatutFacture.Annulee && f.DateEcheance < now),
            ChiffreAffairesMois = await _db.Factures
                .Where(f => f.CabinetId == cabinetId && f.Statut == RefCodes.StatutFacture.Payee && f.DateEmission >= debutMois)
                .SumAsync(f => f.MontantTTC),
            MontantImpaye = await _db.Factures
                .Where(f => f.CabinetId == cabinetId && f.Statut != RefCodes.StatutFacture.Payee && f.Statut != RefCodes.StatutFacture.Annulee && f.Statut != RefCodes.StatutFacture.Remboursee)
                .SumAsync(f => f.MontantRestant),
        };

        // Taux de conversion Devis → Contrat
        var totalDevis = await _db.Devis.CountAsync(d => d.CabinetId == cabinetId && d.Statut != RefCodes.StatutDevis.Brouillon);
        var devisConvertis = await _db.Devis.CountAsync(d => d.CabinetId == cabinetId && d.Statut == RefCodes.StatutDevis.Approuve && d.BonCommande != null);
        dashboard.TauxConversion = totalDevis > 0 ? Math.Round((decimal)devisConvertis / totalDevis * 100, 1) : 0;

        // Dernières factures
        var dernieresFactures = await _db.Factures
            .Where(f => f.CabinetId == cabinetId)
            .Include(f => f.Client)
            .Include(f => f.Contrat)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToListAsync();
        dashboard.DernieresFactures = _mapper.Map<List<FactureDto>>(dernieresFactures);

        // Derniers devis
        var derniersDevis = await _db.Devis
            .Where(d => d.CabinetId == cabinetId)
            .Include(d => d.Client)
            .Include(d => d.Commercial)
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .ToListAsync();
        dashboard.DerniersDevis = _mapper.Map<List<DevisDto>>(derniersDevis);

        return ApiResponse<DashboardCommercialDto>.Ok(dashboard);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MÉTHODES PRIVÉES — Calculs, Références, Historique, Auto-activation
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Génère une référence unique : PREFIX-YYYY-XXXX</summary>
    private async Task<string> GenerateReferenceAsync(string prefix, int cabinetId)
    {
        var year = DateTime.UtcNow.Year;
        var pattern = $"{prefix}-{year}-";

        int lastNumber = 0;

        // Chercher le dernier numéro dans chaque table selon le préfixe (excluant les soft-deleted)
        if (prefix == "DEV")
            lastNumber = await GetMaxReferenceNumber(_db.Devis.IgnoreQueryFilters().Where(d => d.CabinetId == cabinetId), pattern);
        else if (prefix == "PO")
            lastNumber = await GetMaxReferenceNumber(_db.BonsCommande.IgnoreQueryFilters().Where(b => b.CabinetId == cabinetId), pattern);
        else if (prefix == "CTR")
            lastNumber = await GetMaxReferenceNumber(_db.ContratsCommerciaux.IgnoreQueryFilters().Where(c => c.CabinetId == cabinetId), pattern);
        else if (prefix == "INV")
            lastNumber = await GetMaxReferenceNumber(_db.Factures.IgnoreQueryFilters().Where(f => f.CabinetId == cabinetId), pattern);
        else if (prefix == "PAY")
            lastNumber = await GetMaxReferenceNumber(_db.Paiements.IgnoreQueryFilters().Where(p => p.CabinetId == cabinetId), pattern);

        return $"{prefix}-{year}-{(lastNumber + 1):D4}";
    }

    private async Task<int> GetMaxReferenceNumber<T>(IQueryable<T> query, string pattern) where T : class
    {
        // On utilise la réflexion pour accéder à la propriété Reference
        var references = await query
            .Select(e => EF.Property<string>(e, "Reference"))
            .Where(r => r.StartsWith(pattern))
            .ToListAsync();

        if (!references.Any()) return 0;

        return references
            .Select(r => int.TryParse(r.Substring(pattern.Length), out var n) ? n : 0)
            .Max();
    }

    /// <summary>Calcule les montants d'une ligne (HT, TVA, TTC) en appliquant la remise</summary>
    private void CalculerLigne(LigneDocumentCommercial ligne)
    {
        var montantBrut = ligne.Quantite * ligne.PrixUnitaireHT;

        // Appliquer la remise
        decimal remise = 0;
        if (ligne.RemisePourcentage.HasValue && ligne.RemisePourcentage > 0)
            remise = montantBrut * ligne.RemisePourcentage.Value / 100;
        else if (ligne.RemiseMontant.HasValue && ligne.RemiseMontant > 0)
            remise = ligne.RemiseMontant.Value;

        ligne.MontantHT = montantBrut - remise;
        ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
        ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
    }

    /// <summary>Recalcule les totaux d'un devis à partir de ses lignes</summary>
    private void RecalculerTotaux(Devis devis)
    {
        devis.MontantHT = devis.Lignes.Sum(l => l.MontantHT);
        devis.MontantTVA = devis.Lignes.Sum(l => l.MontantTVA);
        devis.MontantTTC = devis.Lignes.Sum(l => l.MontantTTC);
        devis.MontantRemise = devis.Lignes.Sum(l =>
        {
            var brut = l.Quantite * l.PrixUnitaireHT;
            return brut - l.MontantHT;
        });
    }

    private void RecalculerTotauxFacture(Facture facture)
    {
        facture.MontantHT = facture.Lignes.Sum(l => l.MontantHT);
        facture.MontantTVA = facture.Lignes.Sum(l => l.MontantTVA);
        facture.MontantTTC = facture.Lignes.Sum(l => l.MontantTTC);
        facture.MontantRemise = facture.Lignes.Sum(l =>
        {
            var brut = l.Quantite * l.PrixUnitaireHT;
            return brut - l.MontantHT;
        });
        facture.MontantRestant = facture.MontantTTC - facture.MontantPaye;
    }

    /// <summary>
    /// ⚠️ SINGLE SOURCE OF TRUTH — Auto-activate services when a contract is validated.
    /// This is the ONLY entry point for creating/updating services on a Client.
    /// Services are NEVER manually created/edited during Client create/update.
    ///
    /// Logic:
    /// 1. Deserialize ServicesActivesJson from the contract
    /// 2. For each checked service → create or reactivate ServiceContrat with smart defaults
    /// 3. Per-service Honoraires from the JSON override the default
    /// </summary>
    private static List<ContratMembreInputDto> ResolveMembresInput(ContratCommercialCreateDto dto)
    {
        if (dto.Membres != null && dto.Membres.Count > 0)
            return NormalizeMembresInput(dto.Membres);

        return NormalizeMembresInput(
            (dto.MembreIds ?? new List<string>())
                .Select(id => new ContratMembreInputDto { UserId = id })
                .ToList());
    }

    private static List<ContratMembreInputDto> NormalizeMembresInput(IEnumerable<ContratMembreInputDto>? membres)
    {
        var result = new List<ContratMembreInputDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in membres ?? Enumerable.Empty<ContratMembreInputDto>())
        {
            if (string.IsNullOrWhiteSpace(m.UserId) || !seen.Add(m.UserId))
                continue;
            result.Add(new ContratMembreInputDto
            {
                UserId = m.UserId.Trim(),
                RoleLabel = string.IsNullOrWhiteSpace(m.RoleLabel) ? null : m.RoleLabel.Trim()
            });
        }
        return result;
    }

    private static void AddContratMembres(ContratCommercial contrat, List<ContratMembreInputDto>? membres, int cabinetId, string userId)
    {
        foreach (var m in NormalizeMembresInput(membres))
        {
            contrat.Membres.Add(new ContratMembre
            {
                CabinetId = cabinetId,
                UserId = m.UserId,
                RoleLabel = m.RoleLabel,
                CreatedBy = userId
            });
        }
    }

    private async Task SyncContratMembresAsync(int contratId, List<ContratMembreInputDto>? membres, int cabinetId, string userId)
    {
        var existing = await _db.ContratMembres.Where(m => m.ContratId == contratId).ToListAsync();
        _db.ContratMembres.RemoveRange(existing);

        foreach (var m in NormalizeMembresInput(membres))
        {
            _db.ContratMembres.Add(new ContratMembre
            {
                CabinetId = cabinetId,
                ContratId = contratId,
                UserId = m.UserId,
                RoleLabel = m.RoleLabel,
                CreatedBy = userId
            });
        }
    }

    /// <returns>Error message if activation cannot proceed; null on success/skip.</returns>
    private async Task<string?> ActiverServicesFromContratAsync(ContratCommercial contrat, int cabinetId, string userId)
    {
        // Deserialize the checked services from the contract JSON
        if (string.IsNullOrWhiteSpace(contrat.ServicesActivesJson))
            return null;

        List<ContratServiceActiveLine> serviceLines;
        try
        {
            serviceLines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(
                contrat.ServicesActivesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ContratServiceActiveLine>();
        }
        catch
        {
            return null; // Malformed JSON — skip silently
        }

        if (!serviceLines.Any())
            return null;

        // Contract validation is independent of exercice. Bind to open exo when present;
        // otherwise activate services unbound (ExerciceComptableId null) for dossier menu.
        var openExo = await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == contrat.ClientId && e.CabinetId == cabinetId && e.Statut == "Ouvert")
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();

        var client = await _db.Clients.AsNoTracking()
            .Where(c => c.Id == contrat.ClientId)
            .Select(c => new { c.TypeTVA, c.MoisClotureExercice })
            .FirstOrDefaultAsync();

        var typeTVA = client?.TypeTVA ?? RefCodes.TypeTva.Mensuelle;
        var moisCloture = client?.MoisClotureExercice ?? 12;
        var exercice = openExo?.Annee ?? (contrat.DateDebutContrat?.Year ?? DateTime.UtcNow.Year);

        var servicesExistants = await _db.ServiceContrats
            .Where(s => s.ClientId == contrat.ClientId
                        && (openExo == null
                            || s.ExerciceComptableId == openExo.Id
                            || s.ExerciceComptableId == null
                            || s.ExerciceFiscal == exercice
                            || s.ExerciceFiscal == null))
            .ToListAsync();

        foreach (var svcLine in serviceLines)
        {
            if (string.IsNullOrWhiteSpace(svcLine.TypeService)) continue;

            var serviceExistant = servicesExistants
                .FirstOrDefault(s => string.Equals(s.TypeService, svcLine.TypeService, StringComparison.OrdinalIgnoreCase));

            if (serviceExistant != null)
            {
                serviceExistant.Actif = true;
                serviceExistant.LibelleService = !string.IsNullOrWhiteSpace(svcLine.LibelleService)
                    ? svcLine.LibelleService!
                    : GetServiceLabel(svcLine.TypeService);
                serviceExistant.Categorie = GetServiceCategory(svcLine.TypeService);
                serviceExistant.Frequence = GetSmartFrequency(svcLine.TypeService, typeTVA);
                if (string.Equals(svcLine.TypeService, "DOMICILIATION", StringComparison.OrdinalIgnoreCase))
                    serviceExistant.AutoGenererDeclarations = false;
                serviceExistant.DateDebut = contrat.DateDebutContrat ?? DateTime.UtcNow;
                serviceExistant.DateFin = contrat.DateFinContrat;
                serviceExistant.AutoGenererDeclarations = svcLine.AutoGenererDeclarations;
                if (openExo != null)
                    serviceExistant.ExerciceComptableId = openExo.Id;
                serviceExistant.ExerciceFiscal = exercice;
                serviceExistant.UpdatedBy = userId;
                serviceExistant.UpdatedAt = DateTime.UtcNow;
                ApplySmartDeadlines(serviceExistant);
                ComputeExerciceDates(serviceExistant, moisCloture);
            }
            else
            {
                var newService = new ServiceContrat
                {
                    ClientId = contrat.ClientId,
                    TypeService = svcLine.TypeService,
                    LibelleService = !string.IsNullOrWhiteSpace(svcLine.LibelleService)
                        ? svcLine.LibelleService!
                        : GetServiceLabel(svcLine.TypeService),
                    Categorie = GetServiceCategory(svcLine.TypeService),
                    Frequence = GetSmartFrequency(svcLine.TypeService, typeTVA),
                    ExerciceFiscal = exercice,
                    ExerciceComptableId = openExo?.Id,
                    Actif = true,
                    AutoGenererDeclarations = string.Equals(svcLine.TypeService, "DOMICILIATION", StringComparison.OrdinalIgnoreCase)
                        ? false
                        : svcLine.AutoGenererDeclarations,
                    DateDebut = contrat.DateDebutContrat ?? DateTime.UtcNow,
                    DateFin = contrat.DateFinContrat,
                    HonorairesMensuels = null,
                    CreatedBy = userId
                };
                ApplySmartDeadlines(newService);
                ComputeExerciceDates(newService, moisCloture);
                _db.ServiceContrats.Add(newService);
            }
        }

        // NOTE: We do NOT deactivate services not in this contract.
        // A client may have multiple contracts covering different services.
        // Service deactivation should only happen via explicit contract termination.

        // When DOMICILIATION is activated: attach Builder template + auto-start client WF
        if (serviceLines.Any(s => string.Equals(s.TypeService, RefCodes.TypeService.Domiciliation, StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                await _workflow.EnsureServiceWorkflowAsync(contrat.ClientId, RefCodes.TypeService.Domiciliation, cabinetId, userId);
            }
            catch
            {
                // Non-blocking: contract activation must not fail if WF seed/start is unavailable
            }
        }

        return null;
    }

    /// <summary>
    /// When a contract is suspended or terminated, set Actif=false on all ServiceContrat rows
    /// for this client whose TypeService appears in this contract's ServicesActivesJson,
    /// UNLESS another currently valid contract (Valide/Actif + dates) also covers that service.
    /// </summary>
    private async Task DesactiverServicesFromContratAsync(ContratCommercial contrat)
    {
        if (string.IsNullOrWhiteSpace(contrat.ServicesActivesJson))
            return;

        List<ContratServiceActiveLine> serviceLines;
        try
        {
            serviceLines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(
                contrat.ServicesActivesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ContratServiceActiveLine>();
        }
        catch { return; }

        if (!serviceLines.Any()) return;

        var today = DateTime.UtcNow;
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;

        // Find service codes still covered by ANOTHER active contract for this client
        var autresContratsActifs = await _db.ContratsCommerciaux
            .Where(c => c.ClientId == contrat.ClientId && c.Id != contrat.Id
                && activeStatuses.Contains(c.Statut) && !c.IsDeleted
                && (c.DateDebutContrat == null || c.DateDebutContrat <= today)
                && (c.DateFinContrat == null || c.DateFinContrat >= today)
                && c.ServicesActivesJson != null)
            .Select(c => c.ServicesActivesJson!)
            .ToListAsync();

        var codesEncouverts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var json in autresContratsActifs)
        {
            try
            {
                var lines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (lines != null)
                    foreach (var l in lines)
                        if (!string.IsNullOrWhiteSpace(l.TypeService))
                            codesEncouverts.Add(l.TypeService);
            }
            catch { /* ignore malformed JSON */ }
        }

        // Deactivate only services NOT covered by another active contract
        var codesToDeactivate = serviceLines
            .Select(l => l.TypeService)
            .Where(c => !string.IsNullOrWhiteSpace(c) && !codesEncouverts.Contains(c!))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!codesToDeactivate.Any()) return;

        var rows = await _db.ServiceContrats
            .Where(s => s.ClientId == contrat.ClientId && s.Actif
                && codesToDeactivate.Contains(s.TypeService))
            .ToListAsync();

        foreach (var row in rows)
        {
            row.Actif = false;
            row.UpdatedAt = today;
        }
    }

    // ── Smart Default Helpers (Contract → Service mapping) ───────

    private static string? ValidateContratValidity(ContratCommercialCreateDto dto)
    {
        if (dto.ClientId <= 0)
            return "Le client est requis.";
        if (!dto.DateDebutContrat.HasValue)
            return "La date de début du contrat est obligatoire.";
        if (!dto.DateFinContrat.HasValue)
            return "La date de fin du contrat est obligatoire.";
        if (dto.DateFinContrat.Value.Date < dto.DateDebutContrat.Value.Date)
            return "La date de fin doit être postérieure ou égale à la date de début.";
        return null;
    }

    /// <summary>
    /// Extracts TypeService codes from ServicesActivesJson and/or line items.
    /// </summary>
    private static HashSet<string> GetContratServiceCodes(ContratCommercial contrat)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(contrat.ServicesActivesJson))
        {
            try
            {
                var lines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(
                    contrat.ServicesActivesJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (lines != null)
                {
                    foreach (var l in lines)
                        if (!string.IsNullOrWhiteSpace(l.TypeService))
                            codes.Add(l.TypeService.Trim());
                }
            }
            catch { /* ignore malformed JSON */ }
        }

        if (contrat.Lignes != null)
        {
            foreach (var l in contrat.Lignes)
                if (!string.IsNullOrWhiteSpace(l.TypeService))
                    codes.Add(l.TypeService.Trim());
        }

        return codes;
    }

    private static HashSet<string> ParseServiceCodesFromJson(string? json)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json)) return codes;
        try
        {
            var lines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (lines == null) return codes;
            foreach (var l in lines)
                if (!string.IsNullOrWhiteSpace(l.TypeService))
                    codes.Add(l.TypeService.Trim());
        }
        catch { /* ignore malformed JSON */ }
        return codes;
    }

    /// <summary>
    /// Conflict only when another Valide/Actif contract overlaps the period AND shares at least one service.
    /// Different services on the same client/period are allowed.
    /// </summary>
    private async Task<string?> FindOverlappingActiveContratConflictAsync(
        int clientId,
        int cabinetId,
        DateTime? debut,
        DateTime? fin,
        ISet<string> serviceCodes,
        int? excludeContratId = null,
        IEnumerable<int>? excludeContratIds = null)
    {
        if (!debut.HasValue || !fin.HasValue)
            return null;
        if (serviceCodes == null || serviceCodes.Count == 0)
            return null; // no services to conflict on

        var start = debut.Value.Date;
        var end = fin.Value.Date;
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;
        var excludes = new HashSet<int>();
        if (excludeContratId.HasValue) excludes.Add(excludeContratId.Value);
        if (excludeContratIds != null)
            foreach (var x in excludeContratIds) excludes.Add(x);

        var candidates = await _db.ContratsCommerciaux
            .AsNoTracking()
            .Where(c => c.ClientId == clientId
                && c.CabinetId == cabinetId
                && !c.IsDeleted
                && activeStatuses.Contains(c.Statut)
                && c.DateDebutContrat != null
                && c.DateFinContrat != null
                && c.DateDebutContrat.Value.Date <= end
                && c.DateFinContrat.Value.Date >= start)
            .Select(c => new { c.Id, c.Reference, c.ServicesActivesJson, c.DateValidation })
            .ToListAsync();

        if (excludes.Count > 0)
            candidates = candidates.Where(c => !excludes.Contains(c.Id)).ToList();

        // Also load TypeService from lines for candidates (covers older contracts without JSON)
        var candidateIds = candidates.Select(c => c.Id).ToList();
        var ligneServices = candidateIds.Count == 0
            ? new List<(int ContratId, string TypeService)>()
            : (await _db.LignesDocumentsCommerciaux
                .AsNoTracking()
                .Where(l => l.ContratId != null
                    && candidateIds.Contains(l.ContratId.Value)
                    && l.TypeService != null
                    && l.TypeService != "")
                .Select(l => new { ContratId = l.ContratId!.Value, TypeService = l.TypeService! })
                .ToListAsync())
                .Select(x => (x.ContratId, x.TypeService))
                .ToList();

        var ligneByContrat = ligneServices
            .GroupBy(x => x.ContratId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.TypeService).ToList());

        foreach (var c in candidates.OrderByDescending(x => x.DateValidation))
        {
            var otherCodes = ParseServiceCodesFromJson(c.ServicesActivesJson);
            if (ligneByContrat.TryGetValue(c.Id, out var fromLignes))
                foreach (var s in fromLignes)
                    otherCodes.Add(s.Trim());

            var shared = serviceCodes
                .Where(s => otherCodes.Contains(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            if (shared.Count == 0) continue;

            var labels = string.Join(", ", shared.Select(GetServiceLabel));
            return $"Un contrat actif ({c.Reference}) couvre déjà le(s) service(s) {labels} sur cette période pour ce client.";
        }

        return null;
    }

    /// <summary>
    /// Fiduciary fee line: Net HT = PU HT × Qty × (1 − Remise%), TTC = Net HT × (1 + TVA%).
    /// Quantity defaults to 1 for freeform retainer/fee lines.
    /// </summary>
    private static (decimal MontantHT, decimal MontantTVA, decimal MontantTTC, decimal Remise) CalculerLigneHonoraires(LigneDocumentCommercialCreateDto l)
    {
        var qty = l.Quantite <= 0 ? 1m : l.Quantite;
        var brut = qty * l.PrixUnitaireHT;
        var remisePct = Math.Clamp(l.RemisePourcentage ?? 0m, 0m, 100m);
        var remise = brut * remisePct / 100m + (l.RemiseMontant ?? 0m);
        if (remise > brut) remise = brut;
        var montantHT = Math.Round(brut - remise, 2, MidpointRounding.AwayFromZero);
        var tauxTva = l.TauxTVA == 0m ? 0m : l.TauxTVA; // 20% or 0% in Moroccan fiduciary deals
        var montantTVA = Math.Round(montantHT * tauxTva / 100m, 2, MidpointRounding.AwayFromZero);
        var montantTTC = montantHT + montantTVA;
        return (montantHT, montantTVA, montantTTC, Math.Round(remise, 2, MidpointRounding.AwayFromZero));
    }

    private static string GetServiceLabel(string typeService)
    {
        return typeService?.ToUpperInvariant() switch
        {
            "TVA" => "TVA – Taxe sur la Valeur Ajoutée",
            "CM" => "CM – Cotisation Minimale",
            "DRF" => "D.R.F – Déclaration du Résultat Fiscal",
            "ACOMPTES" => "Acomptes IS (trimestriels)",
            "REG_IS" => "Régularisation / Solde IS",
            "IR_SAL" => "IR / Salaires – Retenue à la source",
            "RAS_FONC" => "RAS / Foncier",
            "RAS_DIV" => "RAS / Dividendes",
            "EG" => "E.G – État Descriptif du Gasoil",
            "CNSS" => "CNSS – Déclarations Damancom",
            "DTS" => "D.T.S / État 9421",
            "PAIE" => "Bulletins de paie & traitement de la paie",
            "PV_AGO" => "PV AGO & Rapport de gestion",
            "DEPOT_RC" => "Dépôt des comptes au greffe du tribunal de commerce",
            "COMPTABILITE" => "Tenue comptable, saisie & rapprochements",
            "TRESORERIE" => "Trésorerie & rapprochements bancaires",
            "BILAN" => "Bilan, CPC & états de synthèse",
            "ETATSFISCAUX" => "États fiscaux (CPC, ESG, TFT)",
            "LIASSEFISCALE" => "Liasse fiscale",
            "JURIDIQUE" => "Dossiers Juridiques",
            "DOMICILIATION" => "Domiciliation – Siège social et courrier",
            _ => typeService ?? ""
        };
    }

    private static string GetServiceCategory(string typeService)
    {
        return typeService?.ToUpperInvariant() switch
        {
            "TVA" or "CM" or "DRF" or "ACOMPTES" or "REG_IS" or "IR_SAL" or "RAS_FONC" or "RAS_DIV"
                or "EG" or "DT" or "DEI_TP" or "DRVT" or "CSS_SOL" or "ETATSFISCAUX" or "LIASSEFISCALE"
                => RefCodes.CategorieService.Fiscal,
            "CNSS" or "DTS" or "PAIE" or "DCH_ETS" => RefCodes.CategorieService.Social,
            "COMPTABILITE" or "TRESORERIE" or "IMMOBILISATION" or "BILAN" => RefCodes.CategorieService.Comptable,
            "PV_AGO" or "DEPOT_RC" or "JURIDIQUE" or "DOMICILIATION" => RefCodes.CategorieService.Juridique,
            "AUDITCOMPTABLE" or "CONSEILFISCAL" => RefCodes.CategorieService.Conseil,
            _ => RefCodes.CategorieService.Autre
        };
    }

    private static string GetSmartFrequency(string typeService, string typeTVA)
    {
        return typeService?.ToUpperInvariant() switch
        {
            "TVA" => typeTVA == RefCodes.Frequence.Trimestrielle ? RefCodes.Frequence.Trimestrielle : RefCodes.Frequence.Mensuelle,
            "IR_SAL" or "CNSS" or "PAIE" or "COMPTABILITE" or "TRESORERIE" or "DOMICILIATION" => RefCodes.Frequence.Mensuelle,
            "ACOMPTES" => RefCodes.Frequence.Trimestrielle,
            "CM" or "DRF" or "REG_IS" or "BILAN" or "LIASSEFISCALE" or "ETATSFISCAUX" or "DTS" or "PV_AGO" or "DEPOT_RC" or "EG"
                => RefCodes.Frequence.Annuelle,
            "RAS_FONC" or "RAS_DIV" => RefCodes.Frequence.Mensuelle,
            _ => RefCodes.Frequence.Mensuelle
        };
    }

    private static void ApplySmartDeadlines(ServiceContrat sc)
    {
        switch (sc.TypeService?.ToUpperInvariant())
        {
            case "TVA":
                sc.JourLimiteDeclaration = 26; // working deadline: 26th of following month
                sc.DelaiDeclarationMois = 1;
                break;
            case "IR_SAL":
                sc.JourLimiteDeclaration = 30;
                sc.DelaiDeclarationMois = 1;
                break;
            case "CNSS":
                sc.JourLimiteDeclaration = 10; // 10th of following month (Damancom)
                sc.DelaiDeclarationMois = 1;
                break;
            case "PAIE":
                sc.JourLimiteDeclaration = 5;
                sc.DelaiDeclarationMois = 1;
                break;
            case "DTS":
                sc.JourLimiteDeclaration = 31; // 31 March N+1 (align ResolvePeriod)
                sc.DelaiDeclarationMois = 3;
                break;
            case "ACOMPTES":
                sc.JourLimiteDeclaration = 31; // Mar/Jun/Sep/Dec
                sc.DelaiDeclarationMois = 0;
                break;
            case "CM":
            case "DRF":
            case "REG_IS":
            case "BILAN":
            case "LIASSEFISCALE":
            case "ETATSFISCAUX":
                sc.JourLimiteDeclaration = 31; // March 31
                sc.DelaiDeclarationMois = 3;
                break;
            case "PV_AGO":
                sc.JourLimiteDeclaration = 30; // June 30
                sc.DelaiDeclarationMois = 6;
                break;
            case "DEPOT_RC":
                sc.JourLimiteDeclaration = 31; // July 31
                sc.DelaiDeclarationMois = 7;
                break;
            case "EG":
                sc.JourLimiteDeclaration = 31;
                sc.DelaiDeclarationMois = 3;
                break;
        }
    }

    private static void ComputeExerciceDates(ServiceContrat sc, int moisCloture)
    {
        if (!sc.ExerciceFiscal.HasValue) return;
        var exerciceYear = sc.ExerciceFiscal.Value;
        DateTime debut, fin;
        if (moisCloture == 12)
        {
            debut = new DateTime(exerciceYear, 1, 1);
            fin = new DateTime(exerciceYear, 12, 31);
        }
        else
        {
            var startYear = exerciceYear - 1;
            var startMonth = moisCloture + 1;
            debut = new DateTime(startYear, startMonth, 1);
            fin = new DateTime(exerciceYear, moisCloture, DateTime.DaysInMonth(exerciceYear, moisCloture));
        }
        sc.ExerciceDebut = debut;
        sc.ExerciceFin = fin;
    }

    // ── Historique helpers ────────────────────────────────────────

    private void AddHistorique(string typeDoc, Devis devis, string action, string? ancienStatut, string? nouveauStatut, string userId, string? commentaire = null)
    {
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = devis.CabinetId,
            TypeDocument = typeDoc,
            DocumentId = devis.Id,
            DevisId = devis.Id > 0 ? devis.Id : null,
            Action = action,
            AncienStatut = ancienStatut,
            NouveauStatut = nouveauStatut,
            UserId = userId,
            Commentaire = commentaire,
            DateAction = DateTime.UtcNow,
            VersionDocument = devis.Version,
            CreatedBy = userId
        });
    }

    private void AddHistoriqueBc(string typeDoc, BonCommande bc, string action, string? ancienStatut, string? nouveauStatut, string userId, string? commentaire = null)
    {
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = bc.CabinetId,
            TypeDocument = typeDoc,
            DocumentId = bc.Id,
            BonCommandeId = bc.Id > 0 ? bc.Id : null,
            Action = action,
            AncienStatut = ancienStatut,
            NouveauStatut = nouveauStatut,
            UserId = userId,
            Commentaire = commentaire,
            DateAction = DateTime.UtcNow,
            VersionDocument = bc.Version,
            CreatedBy = userId
        });
    }

    private void AddHistoriqueContrat(string typeDoc, ContratCommercial contrat, string action, string? ancienStatut, string? nouveauStatut, string userId, string? commentaire = null)
    {
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = contrat.CabinetId,
            TypeDocument = typeDoc,
            DocumentId = contrat.Id,
            ContratId = contrat.Id > 0 ? contrat.Id : null,
            Action = action,
            AncienStatut = ancienStatut,
            NouveauStatut = nouveauStatut,
            UserId = userId,
            Commentaire = commentaire,
            DateAction = DateTime.UtcNow,
            VersionDocument = contrat.Version,
            CreatedBy = userId
        });
    }

    private void AddHistoriqueFacture(string typeDoc, Facture facture, string action, string? ancienStatut, string? nouveauStatut, string userId, string? commentaire = null)
    {
        _db.DocumentsCommerciauxHistorique.Add(new DocumentCommercialHistorique
        {
            CabinetId = facture.CabinetId,
            TypeDocument = typeDoc,
            DocumentId = facture.Id,
            FactureId = facture.Id > 0 ? facture.Id : null,
            Action = action,
            AncienStatut = ancienStatut,
            NouveauStatut = nouveauStatut,
            UserId = userId,
            Commentaire = commentaire,
            DateAction = DateTime.UtcNow,
            VersionDocument = facture.Version,
            CreatedBy = userId
        });
    }
}
