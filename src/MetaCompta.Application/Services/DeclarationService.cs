using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class DeclarationService : IDeclarationService
{
    private readonly MetaComptaDbContext _db;
    private readonly IMapper _mapper;
    private readonly IReferenceDataService _refData;
    private readonly INotificationService _notifications;

    public DeclarationService(MetaComptaDbContext db, IMapper mapper, IReferenceDataService refData, INotificationService notifications)
    {
        _db = db;
        _mapper = mapper;
        _refData = refData;
        _notifications = notifications;
    }

    // ─── CONTRACT GUARD ────────────────────────────────────────────
    private static readonly string[] ActiveContractStatuses = RefCodes.StatutContrat.ActiveStatuses;

    /// <summary>
    /// Checks whether the client has at least one contract with status Valide or Actif.
    /// All declaration create/edit/status-change operations require this.
    /// </summary>
    private async Task<bool> HasActiveContractAsync(int clientId, int cabinetId)
    {
        return await _db.Set<ContratCommercial>()
            .AnyAsync(c => c.ClientId == clientId && c.CabinetId == cabinetId
                && ActiveContractStatuses.Contains(c.Statut) && !c.IsDeleted);
    }

    // ─── LIST / GET ────────────────────────────────────────────────
    public async Task<ApiResponse<PagedResult<DeclarationDto>>> GetAllAsync(int cabinetId, int? clientId = null, string? type = null, string? statut = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Declarations
            .Where(d => d.CabinetId == cabinetId)
            .Include(d => d.Client)
            .Include(d => d.Assistant)
            .Include(d => d.Validateur)
            .AsQueryable();

        if (clientId.HasValue) query = query.Where(d => d.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(d => d.Type == type);
        if (!string.IsNullOrWhiteSpace(statut)) query = query.Where(d => d.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.DateLimite)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<DeclarationDto>>.Ok(new PagedResult<DeclarationDto>
        {
            Items = _mapper.Map<List<DeclarationDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<DeclarationDto>> GetByIdAsync(int id, int cabinetId)
    {
        var dec = await _db.Declarations
            .Include(d => d.Client).Include(d => d.Assistant).Include(d => d.Validateur)
            .FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<DeclarationDto>.Fail("Déclaration non trouvée.");
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(dec));
    }

    // ─── GENERIC CREATE ────────────────────────────────────────────
    public async Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
    {
        // ── Contract guard: client must have an active/valid contract ──
        if (!await HasActiveContractAsync(dto.ClientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration : le client n'a aucun contrat actif ou validé.");

        return dto.Type.ToUpper() switch
        {
            "TVA" => await CreateTVAAsync(dto.ClientId, cabinetId, userId),
            "IS" => await CreateISAsync(dto.ClientId, dto.AnneeFiscale ?? DateTime.UtcNow.Year, cabinetId, userId),
            "IR" => await CreateIRAsync(dto.ClientId, dto.AnneeFiscale ?? DateTime.UtcNow.Year, cabinetId, userId),
            "CNSS" => await CreateCNSSAsync(dto.ClientId, cabinetId, userId),
            _ => ApiResponse<DeclarationDto>.Fail($"Type de déclaration '{dto.Type}' non supporté.")
        };
    }

    // ─── DELETE ────────────────────────────────────────────────────
    public async Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId)
    {
        var dec = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<bool>.Fail("Déclaration non trouvée.");
        if (dec.Statut == RefCodes.StatutDeclaration.Soumise || dec.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<bool>.Fail("Impossible de supprimer une déclaration soumise ou validée.");

        dec.IsDeleted = true;
        dec.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Déclaration supprimée.");
    }

    // ═══════════════════════════════════════════════════════════════
    //  TVA
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<DeclarationDto>> CreateTVAAsync(int clientId, int cabinetId, string userId)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration TVA : le client n'a aucun contrat actif ou validé.");

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var now = DateTime.UtcNow;
        var periodeDebut = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var periodeFin = periodeDebut.AddMonths(1).AddDays(-1);
        var dateLimite = periodeFin.AddMonths(1);

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            Type = "TVA",
            Periode = periodeDebut.ToString("yyyy-MM"),
            DateDebut = periodeDebut,
            DateFin = periodeFin,
            DateLimite = new DateTime(dateLimite.Year, dateLimite.Month, Math.Min(30, DateTime.DaysInMonth(dateLimite.Year, dateLimite.Month))),
            Statut = RefCodes.StatutDeclaration.ACompleter,
            AssistantId = userId,
            CreatedBy = userId
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();

        var previousCredit = await _db.DeclarationsTVA
            .Where(t => t.Declaration!.ClientId == clientId && t.Declaration.Type == "TVA" && t.Declaration.Statut == RefCodes.StatutDeclaration.Soumise)
            .OrderByDescending(t => t.Declaration!.DateFin)
            .Select(t => t.CreditTVA)
            .FirstOrDefaultAsync();

        _db.DeclarationsTVA.Add(new DeclarationTVA
        {
            DeclarationId = declaration.Id,
            CabinetId = cabinetId,
            CreditPrecedent = previousCredit,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration TVA créée.");
    }

    public async Task<ApiResponse<DeclarationTVADto>> GetTVADataAsync(int declarationId, int cabinetId)
    {
        var tva = await _db.DeclarationsTVA
            .Include(t => t.LignesFactures.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null) return ApiResponse<DeclarationTVADto>.Fail("Données TVA non trouvées.");
        return ApiResponse<DeclarationTVADto>.Ok(_mapper.Map<DeclarationTVADto>(tva));
    }

    public async Task<ApiResponse<DeclarationTVADto>> CalculateTVAAsync(DeclarationTVADto dto)
    {
        var taux20 = await _refData.GetNumericValueAsync("TVA_RATE", "TVA_20") / 100m;
        var taux14 = await _refData.GetNumericValueAsync("TVA_RATE", "TVA_14") / 100m;
        var taux10 = await _refData.GetNumericValueAsync("TVA_RATE", "TVA_10") / 100m;
        var taux7 = await _refData.GetNumericValueAsync("TVA_RATE", "TVA_7") / 100m;

        dto.TVA_20 = Math.Round(dto.CAHT_20 * taux20, 2);
        dto.TVA_14 = Math.Round(dto.CAHT_14 * taux14, 2);
        dto.TVA_10 = Math.Round(dto.CAHT_10 * taux10, 2);
        dto.TVA_7 = Math.Round(dto.CAHT_7 * taux7, 2);
        dto.TVACollecteeTotale = dto.TVA_20 + dto.TVA_14 + dto.TVA_10 + dto.TVA_7;
        dto.TVARecuperableTotale = dto.TVARecuperableCharges + dto.TVARecuperableImmobilisations;
        dto.TVANette = dto.TVACollecteeTotale - dto.TVARecuperableTotale - dto.CreditPrecedent;
        dto.TVAaPayer = Math.Max(0, dto.TVANette);
        dto.CreditTVA = Math.Max(0, -dto.TVANette);
        return ApiResponse<DeclarationTVADto>.Ok(dto);
    }

    public async Task<ApiResponse<DeclarationTVADto>> SaveTVADataAsync(int declarationId, DeclarationTVADto dto, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationTVADto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationTVADto>.Fail("Modification impossible : le client n'a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationTVADto>.Fail("Impossible de modifier une déclaration validée ou soumise.");

        var calcResult = await CalculateTVAAsync(dto);
        var calculated = calcResult.Data!;

        var tvaData = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId);
        if (tvaData == null)
        {
            tvaData = new DeclarationTVA { DeclarationId = declarationId, CabinetId = cabinetId, CreatedBy = userId };
            _db.DeclarationsTVA.Add(tvaData);
        }

        tvaData.CAHT_20 = calculated.CAHT_20; tvaData.CAHT_14 = calculated.CAHT_14;
        tvaData.CAHT_10 = calculated.CAHT_10; tvaData.CAHT_7 = calculated.CAHT_7;
        tvaData.CAExonere = calculated.CAExonere;
        tvaData.TVA_20 = calculated.TVA_20; tvaData.TVA_14 = calculated.TVA_14;
        tvaData.TVA_10 = calculated.TVA_10; tvaData.TVA_7 = calculated.TVA_7;
        tvaData.TVACollecteeTotale = calculated.TVACollecteeTotale;
        tvaData.TVARecuperableCharges = calculated.TVARecuperableCharges;
        tvaData.TVARecuperableImmobilisations = calculated.TVARecuperableImmobilisations;
        tvaData.TVARecuperableTotale = calculated.TVARecuperableTotale;
        tvaData.CreditPrecedent = calculated.CreditPrecedent;
        tvaData.TVANette = calculated.TVANette;
        tvaData.TVAaPayer = calculated.TVAaPayer;
        tvaData.CreditTVA = calculated.CreditTVA;

        declaration.MontantDu = calculated.TVAaPayer;
        declaration.MontantCredit = calculated.CreditTVA;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationTVADto>.Ok(calculated, "Données TVA sauvegardées.");
    }

    /// <summary>
    /// Saves only the manual adjustment fields (Immobilisations + Crédit Précédent),
    /// then recalculates all TVA totals from invoice lines. Never overwrites line-calculated totals.
    /// </summary>
    public async Task<ApiResponse<DeclarationTVADto>> SaveTVAAjustementsAsync(int declarationId, decimal immobilisations, decimal creditPrecedent, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationTVADto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationTVADto>.Fail("Modification impossible : le client n’a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationTVADto>.Fail("Impossible de modifier une déclaration validée ou soumise.");

        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null)
        {
            tva = new DeclarationTVA { DeclarationId = declarationId, CabinetId = cabinetId, CreatedBy = userId };
            _db.DeclarationsTVA.Add(tva);
            await _db.SaveChangesAsync();
        }

        // Only update the two manual fields
        tva.TVARecuperableImmobilisations = immobilisations;
        tva.CreditPrecedent = creditPrecedent;
        await _db.SaveChangesAsync();

        // Recalculate everything from lines (uses the updated Immobilisations + CreditPrecedent)
        await RecalculateTVATotalsFromLignesAsync(tva.Id, declarationId, cabinetId);

        // Reload to get final calculated values
        await _db.Entry(tva).ReloadAsync();
        var result = _mapper.Map<DeclarationTVADto>(tva);
        result.DeclarationId = declarationId;
        return ApiResponse<DeclarationTVADto>.Ok(result, "Ajustements TVA enregistrés et totaux recalculés.");
    }


    // ═══════════════════════════════════════════════════════════════
    //  TVA INVOICE LINES (Lignes Factures)
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<TVALigneFactureDto>> AddTVALigneAsync(int declarationId, TVALigneFactureDto dto, int cabinetId, string userId)
    {
        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null) return ApiResponse<TVALigneFactureDto>.Fail("Déclaration TVA non trouvée.");

        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<TVALigneFactureDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<TVALigneFactureDto>.Fail("Modification impossible : aucun contrat actif.");

        var ligne = new TVALigneFacture
        {
            DeclarationTVAId = tva.Id,
            CabinetId = cabinetId,
            TypePiece = dto.TypePiece ?? "Facture",
            NumeroFacture = dto.NumeroFacture,
            DateFacture = dto.DateFacture,
            FournisseurClient = dto.FournisseurClient,
            ICE = dto.ICE,
            IdentifiantFiscal = dto.IdentifiantFiscal,
            Description = dto.Description,
            MontantHT = dto.MontantHT,
            TauxTVA = dto.TauxTVA,
            MontantTVA = Math.Round(dto.MontantHT * dto.TauxTVA / 100m, 2),
            MontantTTC = dto.MontantHT + Math.Round(dto.MontantHT * dto.TauxTVA / 100m, 2),
            TypeOperation = dto.TypeOperation ?? "Vente",
            IsProrata = dto.IsProrata,
            ProrataPercent = dto.ProrataPercent,
            Regime = dto.Regime,
            CreatedBy = userId
        };
        _db.Set<TVALigneFacture>().Add(ligne);
        await _db.SaveChangesAsync();

        // Auto-recalculate TVA totals from all active lines
        await RecalculateTVATotalsFromLignesAsync(tva.Id, declarationId, cabinetId);

        return ApiResponse<TVALigneFactureDto>.Ok(_mapper.Map<TVALigneFactureDto>(ligne), "Ligne ajoutée et totaux recalculés.");
    }

    public async Task<ApiResponse<bool>> DeleteTVALigneAsync(int declarationId, int ligneId, int cabinetId, string userId)
    {
        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null) return ApiResponse<bool>.Fail("Déclaration TVA non trouvée.");

        var ligne = await _db.Set<TVALigneFacture>().FirstOrDefaultAsync(l => l.Id == ligneId && l.DeclarationTVAId == tva.Id && l.CabinetId == cabinetId);
        if (ligne == null) return ApiResponse<bool>.Fail("Ligne non trouvée.");

        ligne.IsDeleted = true;
        ligne.DeletedAt = DateTime.UtcNow;
        ligne.DeletedBy = userId;
        await _db.SaveChangesAsync();

        // Auto-recalculate TVA totals from remaining active lines
        await RecalculateTVATotalsFromLignesAsync(tva.Id, declarationId, cabinetId);

        return ApiResponse<bool>.Ok(true, "Ligne supprimée.");
    }

    /// <summary>
    /// Recalculates all TVA summary totals (CAHT_20, TVACollectee, TVARecuperable, etc.)
    /// from the active invoice lines. Called after every Add/Delete/Import of lines.
    /// This is the SINGLE SOURCE OF TRUTH for TVA totals.
    /// </summary>
    private async Task RecalculateTVATotalsFromLignesAsync(int tvaId, int declarationId, int cabinetId)
    {
        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.Id == tvaId && t.CabinetId == cabinetId);
        if (tva == null) return;

        var lignes = await _db.Set<TVALigneFacture>()
            .Where(l => l.DeclarationTVAId == tvaId && l.CabinetId == cabinetId && !l.IsDeleted)
            .ToListAsync();

        // Vente (Collectée) - aggregate by taux
        var ventes = lignes.Where(l => l.TypeOperation == "Vente").ToList();
        tva.CAHT_20 = ventes.Where(l => l.TauxTVA == 20).Sum(l => l.MontantHT);
        tva.CAHT_14 = ventes.Where(l => l.TauxTVA == 14).Sum(l => l.MontantHT);
        tva.CAHT_10 = ventes.Where(l => l.TauxTVA == 10).Sum(l => l.MontantHT);
        tva.CAHT_7  = ventes.Where(l => l.TauxTVA == 7).Sum(l => l.MontantHT);
        tva.CAExonere = ventes.Where(l => l.TauxTVA == 0).Sum(l => l.MontantHT);

        tva.TVA_20 = Math.Round(tva.CAHT_20 * 0.20m, 2);
        tva.TVA_14 = Math.Round(tva.CAHT_14 * 0.14m, 2);
        tva.TVA_10 = Math.Round(tva.CAHT_10 * 0.10m, 2);
        tva.TVA_7  = Math.Round(tva.CAHT_7  * 0.07m, 2);
        tva.TVACollecteeTotale = tva.TVA_20 + tva.TVA_14 + tva.TVA_10 + tva.TVA_7;

        // Achat (Déductible / Récupérable)
        var achats = lignes.Where(l => l.TypeOperation == "Achat").ToList();
        tva.TVARecuperableCharges = achats.Sum(l => l.MontantTVA);
        tva.TVARecuperableTotale = tva.TVARecuperableCharges + tva.TVARecuperableImmobilisations;

        // Net calculation
        tva.TVANette = tva.TVACollecteeTotale - tva.TVARecuperableTotale - tva.CreditPrecedent;
        tva.TVAaPayer = Math.Max(0, tva.TVANette);
        tva.CreditTVA = Math.Max(0, -tva.TVANette);

        // Update declaration montants
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration != null)
        {
            declaration.MontantDu = tva.TVAaPayer;
            declaration.MontantCredit = tva.CreditTVA;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ApiResponse<List<TVALigneFactureDto>>> ImportTVALignesAsync(int declarationId, List<TVALigneFactureDto> lignes, int cabinetId, string userId)
    {
        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null) return ApiResponse<List<TVALigneFactureDto>>.Fail("Déclaration TVA non trouvée.");

        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<List<TVALigneFactureDto>>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<List<TVALigneFactureDto>>.Fail("Modification impossible : aucun contrat actif.");

        var result = new List<TVALigneFacture>();
        foreach (var d in lignes)
        {
            var ligne = new TVALigneFacture
            {
                DeclarationTVAId = tva.Id,
                CabinetId = cabinetId,
                TypePiece = d.TypePiece ?? "Facture",
                NumeroFacture = d.NumeroFacture,
                DateFacture = d.DateFacture,
                FournisseurClient = d.FournisseurClient,
                ICE = d.ICE,
                IdentifiantFiscal = d.IdentifiantFiscal,
                Description = d.Description,
                MontantHT = d.MontantHT,
                TauxTVA = d.TauxTVA,
                MontantTVA = Math.Round(d.MontantHT * d.TauxTVA / 100m, 2),
                MontantTTC = d.MontantHT + Math.Round(d.MontantHT * d.TauxTVA / 100m, 2),
                TypeOperation = d.TypeOperation ?? "Vente",
                IsProrata = d.IsProrata,
                ProrataPercent = d.ProrataPercent,
                Regime = d.Regime,
                CreatedBy = userId
            };
            _db.Set<TVALigneFacture>().Add(ligne);
            result.Add(ligne);
        }
        await _db.SaveChangesAsync();

        // Auto-recalculate TVA totals from all lines (including imported ones)
        await RecalculateTVATotalsFromLignesAsync(tva.Id, declarationId, cabinetId);

        return ApiResponse<List<TVALigneFactureDto>>.Ok(_mapper.Map<List<TVALigneFactureDto>>(result), $"{result.Count} ligne(s) importée(s).");
    }


    // ═══════════════════════════════════════════════════════════════
    //  IS (Impôt sur les Sociétés)
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<DeclarationDto>> CreateISAsync(int clientId, int anneeFiscale, int cabinetId, string userId)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration IS : le client n'a aucun contrat actif ou validé.");

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            Type = "IS",
            Periode = anneeFiscale.ToString(),
            DateDebut = new DateTime(anneeFiscale, 1, 1),
            DateFin = new DateTime(anneeFiscale, 12, 31),
            DateLimite = new DateTime(anneeFiscale + 1, 3, 31), // 31 March N+1
            Statut = RefCodes.StatutDeclaration.ACompleter,
            AssistantId = userId,
            CreatedBy = userId
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();

        _db.DeclarationsIS.Add(new DeclarationIS
        {
            DeclarationId = declaration.Id,
            CabinetId = cabinetId,
            AnneeFiscale = anneeFiscale,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration IS créée.");
    }

    public async Task<ApiResponse<DeclarationISDto>> GetISDataAsync(int declarationId, int cabinetId)
    {
        var isData = await _db.DeclarationsIS.FirstOrDefaultAsync(i => i.DeclarationId == declarationId && i.CabinetId == cabinetId);
        if (isData == null) return ApiResponse<DeclarationISDto>.Fail("Données IS non trouvées.");
        return ApiResponse<DeclarationISDto>.Ok(_mapper.Map<DeclarationISDto>(isData));
    }

    public Task<ApiResponse<DeclarationISDto>> CalculateISAsync(DeclarationISDto dto)
    {
        dto.ResultatFiscal = dto.ResultatComptable + dto.Reintegrations - dto.Deductions;
        dto.BaseImposable = Math.Max(0, dto.ResultatFiscal - dto.DeficitsAnterieurs);

        // Moroccan IS progressive rates 2026
        decimal isDu;
        if (dto.BaseImposable <= 300_000m) { dto.TauxIS = 10m; isDu = dto.BaseImposable * 0.10m; }
        else if (dto.BaseImposable <= 1_000_000m) { dto.TauxIS = 20m; isDu = dto.BaseImposable * 0.20m; }
        else { dto.TauxIS = 31m; isDu = dto.BaseImposable * 0.31m; }

        dto.ISDu = Math.Round(isDu, 2);
        dto.CotisationMinimale = Math.Max(dto.ResultatComptable * 0.005m, 3000m); // 0.5% min or 3000 MAD
        dto.ISDu = Math.Max(dto.ISDu, dto.CotisationMinimale);
        dto.ISNet = dto.ISDu - dto.AcomptesVerses;

        return Task.FromResult(ApiResponse<DeclarationISDto>.Ok(dto));
    }

    public async Task<ApiResponse<DeclarationISDto>> SaveISDataAsync(int declarationId, DeclarationISDto dto, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationISDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationISDto>.Fail("Modification impossible : le client n'a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationISDto>.Fail("Impossible de modifier une déclaration validée ou soumise.");

        var calcResult = await CalculateISAsync(dto);
        var calculated = calcResult.Data!;

        var isData = await _db.DeclarationsIS.FirstOrDefaultAsync(i => i.DeclarationId == declarationId);
        if (isData == null)
        {
            isData = new DeclarationIS { DeclarationId = declarationId, CabinetId = cabinetId, CreatedBy = userId };
            _db.DeclarationsIS.Add(isData);
        }

        isData.AnneeFiscale = calculated.AnneeFiscale;
        isData.ResultatComptable = calculated.ResultatComptable;
        isData.Reintegrations = calculated.Reintegrations;
        isData.Deductions = calculated.Deductions;
        isData.ResultatFiscal = calculated.ResultatFiscal;
        isData.DeficitsAnterieurs = calculated.DeficitsAnterieurs;
        isData.BaseImposable = calculated.BaseImposable;
        isData.TauxIS = calculated.TauxIS;
        isData.ISDu = calculated.ISDu;
        isData.AcomptesVerses = calculated.AcomptesVerses;
        isData.ISNet = calculated.ISNet;
        isData.Acompte1 = calculated.Acompte1; isData.Acompte2 = calculated.Acompte2;
        isData.Acompte3 = calculated.Acompte3; isData.Acompte4 = calculated.Acompte4;

        declaration.MontantDu = Math.Max(0, calculated.ISNet);
        declaration.MontantCredit = calculated.ISNet < 0 ? Math.Abs(calculated.ISNet) : 0;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationISDto>.Ok(calculated, "Données IS sauvegardées.");
    }

    // ═══════════════════════════════════════════════════════════════
    //  IR (Impôt sur le Revenu)
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<DeclarationDto>> CreateIRAsync(int clientId, int anneeFiscale, int cabinetId, string userId)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration IR : le client n'a aucun contrat actif ou validé.");

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            Type = "IR",
            Periode = anneeFiscale.ToString(),
            DateDebut = new DateTime(anneeFiscale, 1, 1),
            DateFin = new DateTime(anneeFiscale, 12, 31),
            DateLimite = new DateTime(anneeFiscale + 1, 4, 30), // 30 April N+1
            Statut = RefCodes.StatutDeclaration.ACompleter,
            AssistantId = userId,
            CreatedBy = userId
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration IR créée.");
    }

    public async Task<ApiResponse<DeclarationIRDto>> GetIRDataAsync(int declarationId, int cabinetId)
    {
        // IR data stored in Declaration.DonneesJson
        var dec = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<DeclarationIRDto>.Fail("Déclaration non trouvée.");
        if (!string.IsNullOrEmpty(dec.DonneesJson))
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<DeclarationIRDto>(dec.DonneesJson);
            if (data != null) { data.DeclarationId = declarationId; return ApiResponse<DeclarationIRDto>.Ok(data); }
        }
        return ApiResponse<DeclarationIRDto>.Ok(new DeclarationIRDto { DeclarationId = declarationId });
    }

    public Task<ApiResponse<DeclarationIRDto>> CalculateIRAsync(DeclarationIRDto dto)
    {
        dto.FraisProfessionnels = Math.Min(dto.RevenuBrutGlobal * 0.20m, 30_000m); // 20% plafonné 30 000
        dto.RevenuNetImposable = Math.Max(0, dto.RevenuBrutGlobal - dto.FraisProfessionnels - dto.Deductions);

        // Moroccan IR progressive brackets 2026
        decimal irBrut;
        decimal sommeADeduire;
        if (dto.RevenuNetImposable <= 30_000m) { dto.TauxIR = 0; irBrut = 0; sommeADeduire = 0; }
        else if (dto.RevenuNetImposable <= 50_000m) { dto.TauxIR = 10; irBrut = dto.RevenuNetImposable * 0.10m; sommeADeduire = 3_000m; }
        else if (dto.RevenuNetImposable <= 60_000m) { dto.TauxIR = 20; irBrut = dto.RevenuNetImposable * 0.20m; sommeADeduire = 8_000m; }
        else if (dto.RevenuNetImposable <= 80_000m) { dto.TauxIR = 30; irBrut = dto.RevenuNetImposable * 0.30m; sommeADeduire = 14_000m; }
        else if (dto.RevenuNetImposable <= 180_000m) { dto.TauxIR = 34; irBrut = dto.RevenuNetImposable * 0.34m; sommeADeduire = 17_200m; }
        else { dto.TauxIR = 38; irBrut = dto.RevenuNetImposable * 0.38m; sommeADeduire = 24_400m; }

        dto.SommeADeduire = sommeADeduire;
        dto.IRBrut = Math.Round(irBrut - sommeADeduire, 2);

        dto.DeductionPersonnesACharge = Math.Min(dto.NombrePersonnesACharge * 360m, 2160m); // 360 MAD/person max 6
        dto.ChargesFamille = dto.DeductionPersonnesACharge;
        dto.IRNet = Math.Max(0, dto.IRBrut - dto.ChargesFamille);

        return Task.FromResult(ApiResponse<DeclarationIRDto>.Ok(dto));
    }

    public async Task<ApiResponse<DeclarationIRDto>> SaveIRDataAsync(int declarationId, DeclarationIRDto dto, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationIRDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationIRDto>.Fail("Modification impossible : le client n'a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationIRDto>.Fail("Impossible de modifier une déclaration validée ou soumise.");

        var calcResult = await CalculateIRAsync(dto);
        var calculated = calcResult.Data!;
        calculated.DeclarationId = declarationId;

        declaration.DonneesJson = System.Text.Json.JsonSerializer.Serialize(calculated);
        declaration.MontantDu = calculated.IRNet;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationIRDto>.Ok(calculated, "Données IR sauvegardées.");
    }

    // ═══════════════════════════════════════════════════════════════
    //  CNSS
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<DeclarationDto>> CreateCNSSAsync(int clientId, int cabinetId, string userId)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration CNSS : le client n'a aucun contrat actif ou validé.");

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var now = DateTime.UtcNow;
        var periodeDebut = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var periodeFin = periodeDebut.AddMonths(1).AddDays(-1);

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            Type = "CNSS",
            Periode = periodeDebut.ToString("yyyy-MM"),
            DateDebut = periodeDebut,
            DateFin = periodeFin,
            DateLimite = periodeFin.AddDays(10), // 10th of following month
            Statut = RefCodes.StatutDeclaration.ACompleter,
            AssistantId = userId,
            CreatedBy = userId
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();

        _db.DeclarationsCNSS.Add(new DeclarationCNSS
        {
            DeclarationId = declaration.Id,
            CabinetId = cabinetId,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration CNSS créée.");
    }

    public async Task<ApiResponse<DeclarationCNSSDto>> GetCNSSDataAsync(int declarationId, int cabinetId)
    {
        var cnss = await _db.DeclarationsCNSS.FirstOrDefaultAsync(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId);
        if (cnss == null) return ApiResponse<DeclarationCNSSDto>.Fail("Données CNSS non trouvées.");
        return ApiResponse<DeclarationCNSSDto>.Ok(_mapper.Map<DeclarationCNSSDto>(cnss));
    }

    public async Task<ApiResponse<DeclarationCNSSDto>> CalculateCNSSAsync(int declarationId, int cabinetId)
    {
        var dec = await _db.Declarations.Include(d => d.Client).FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<DeclarationCNSSDto>.Fail("Déclaration non trouvée.");

        var plafond = await _refData.GetNumericValueAsync("CNSS", "PLAFOND");
        var tauxSalarial = await _refData.GetNumericValueAsync("CNSS", "TAUX_SALARIAL") / 100m;
        var tauxPatronal = await _refData.GetNumericValueAsync("CNSS", "TAUX_PATRONAL") / 100m;

        // Get employees for this client in the declaration period
        var employees = await _db.Employees.Where(e => e.ClientId == dec.ClientId && !e.IsDeleted).ToListAsync();

        decimal totalBruts = 0, totalCotSal = 0, totalCotPat = 0;
        decimal totalAmoSal = 0, totalAmoPat = 0, totalFp = 0;

        foreach (var emp in employees)
        {
            var salaire = emp.SalaireBase + (emp.PrimeAnciennete ?? 0) + (emp.PrimeResponsabilite ?? 0) + (emp.PrimePresence ?? 0);
            var salairePlafonne = Math.Min(salaire, plafond);
            totalBruts += salaire;
            totalCotSal += salairePlafonne * tauxSalarial;
            totalCotPat += salairePlafonne * tauxPatronal;
            totalAmoSal += salaire * 0.0226m; // AMO salariale 2.26%
            totalAmoPat += salaire * 0.0452m; // AMO patronale 4.52%
            totalFp += salaire * 0.0162m; // Formation professionnelle 1.62%
        }

        var dto = new DeclarationCNSSDto
        {
            DeclarationId = declarationId,
            TotalSalairesBruts = Math.Round(totalBruts, 2),
            TotalCotisationSalariale = Math.Round(totalCotSal, 2),
            TotalCotisationPatronale = Math.Round(totalCotPat, 2),
            TotalAMOSalariale = Math.Round(totalAmoSal, 2),
            TotalAMOPatronale = Math.Round(totalAmoPat, 2),
            TotalFormationProfessionnelle = Math.Round(totalFp, 2),
            NombreEmployes = employees.Count
        };
        dto.MontantTotal = dto.TotalCotisationSalariale + dto.TotalCotisationPatronale
                         + dto.TotalAMOSalariale + dto.TotalAMOPatronale + dto.TotalFormationProfessionnelle;

        return ApiResponse<DeclarationCNSSDto>.Ok(dto);
    }

    public async Task<ApiResponse<DeclarationCNSSDto>> SaveCNSSDataAsync(int declarationId, DeclarationCNSSDto dto, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationCNSSDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationCNSSDto>.Fail("Modification impossible : le client n'a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationCNSSDto>.Fail("Impossible de modifier une déclaration validée ou soumise.");

        var cnssData = await _db.DeclarationsCNSS.FirstOrDefaultAsync(c => c.DeclarationId == declarationId);
        if (cnssData == null)
        {
            cnssData = new DeclarationCNSS { DeclarationId = declarationId, CabinetId = cabinetId, CreatedBy = userId };
            _db.DeclarationsCNSS.Add(cnssData);
        }

        cnssData.TotalSalairesBruts = dto.TotalSalairesBruts;
        cnssData.TotalCotisationSalariale = dto.TotalCotisationSalariale;
        cnssData.TotalCotisationPatronale = dto.TotalCotisationPatronale;
        cnssData.TotalAMOSalariale = dto.TotalAMOSalariale;
        cnssData.TotalAMOPatronale = dto.TotalAMOPatronale;
        cnssData.TotalFormationProfessionnelle = dto.TotalFormationProfessionnelle;
        cnssData.MontantTotal = dto.MontantTotal;
        cnssData.NombreEmployes = dto.NombreEmployes;
        cnssData.DetailEmployesJson = dto.DetailEmployesJson;

        declaration.MontantDu = dto.MontantTotal;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationCNSSDto>.Ok(dto, "Données CNSS sauvegardées.");
    }

    // ─── STATUS ────────────────────────────────────────────────────
    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId, string? comment = null)
    {
        var dec = await _db.Declarations.Include(d => d.Client).FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<bool>.Fail("Déclaration non trouvée.");

        // Contract guard: modifications require an active contract
        if (!await HasActiveContractAsync(dec.ClientId, cabinetId))
            return ApiResponse<bool>.Fail("Impossible de modifier le statut : le client n'a aucun contrat actif ou validé. Consultation uniquement.");

        var oldStatut = dec.Statut;
        var validTransitions = new Dictionary<string, string[]>
        {
            [RefCodes.StatutDeclaration.Brouillon] = [RefCodes.StatutDeclaration.ACompleter],
            [RefCodes.StatutDeclaration.ACompleter] = [RefCodes.StatutDeclaration.Completee],
            [RefCodes.StatutDeclaration.Completee] = [RefCodes.StatutDeclaration.EnRevision, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.EnRevision] = [RefCodes.StatutDeclaration.Validee, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.Validee] = [RefCodes.StatutDeclaration.Soumise, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.Refusee] = [RefCodes.StatutDeclaration.Brouillon, RefCodes.StatutDeclaration.ACompleter],
        };

        if (!validTransitions.ContainsKey(oldStatut) || !validTransitions[oldStatut].Contains(newStatut))
            return ApiResponse<bool>.Fail($"Transition de '{oldStatut}' vers '{newStatut}' non autorisée.");

        dec.Statut = newStatut;
        if (newStatut == RefCodes.StatutDeclaration.Soumise) dec.DateSoumission = DateTime.UtcNow;
        if (newStatut == RefCodes.StatutDeclaration.Refusee) dec.MotifRefus = comment;

        _db.DeclarationHistories.Add(new DeclarationHistory
        {
            DeclarationId = id,
            CabinetId = cabinetId,
            AncienStatut = oldStatut,
            NouveauStatut = newStatut,
            UserId = userId,
            Commentaire = comment,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync();

        if (newStatut == RefCodes.StatutDeclaration.Completee && dec.Client != null)
        {
            var managers = await _db.Users.Where(u => u.CabinetId == cabinetId).ToListAsync();
            foreach (var mgr in managers)
            {
                await _notifications.CreateAsync(mgr.Id, $"Déclaration {dec.Type} prête pour validation",
                    $"Déclaration {dec.Type} {dec.Periode} - {dec.Client.RaisonSociale} prête pour validation.",
                    "Warning", "Declaration", $"/declarations/{id}");
            }
        }

        return ApiResponse<bool>.Ok(true, $"Statut changé vers '{newStatut}'.");
    }
}