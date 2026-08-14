using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class ExerciceClotureService : IExerciceClotureService
{
    private static readonly HashSet<string> FinalDeclarationStatuts = new(StringComparer.OrdinalIgnoreCase)
    {
        RefCodes.StatutDeclaration.Validee,
        RefCodes.StatutDeclaration.Soumise,
        "Cloturee", "Clôturée", "Annulee", "Annulée"
    };

    private static readonly HashSet<string> FinalPaieStatuts = new(StringComparer.OrdinalIgnoreCase)
    {
        RefCodes.StatutPaie.Validee,
        RefCodes.StatutPaie.Cloturee
    };

    private readonly MetaComptaDbContext _db;
    private readonly IComptabiliteService _compta;
    private readonly ILogger<ExerciceClotureService> _logger;

    public ExerciceClotureService(MetaComptaDbContext db, IComptabiliteService compta, ILogger<ExerciceClotureService> logger)
    {
        _db = db;
        _compta = compta;
        _logger = logger;
    }

    public async Task<ApiResponse<ExerciceCloturePrecheckDto>> PrecheckAsync(int exerciceId, int cabinetId)
    {
        var exo = await _db.ExercicesComptables
            .FirstOrDefaultAsync(e => e.Id == exerciceId && e.CabinetId == cabinetId);
        if (exo == null) return ApiResponse<ExerciceCloturePrecheckDto>.Fail("Exercice introuvable.");
        if (!string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<ExerciceCloturePrecheckDto>.Fail("Seuls les exercices ouverts peuvent être pré-contrôlés.");

        var checks = new List<ExerciceClotureCheckItemDto>();

        var brouillons = await _db.EcrituresComptables.CountAsync(e =>
            e.ExerciceComptableId == exo.Id && e.CabinetId == cabinetId && e.Statut == "Brouillon");
        checks.Add(Check("EcrituresBrouillon", "Écritures en brouillon", brouillons == 0,
            brouillons == 0 ? "Aucune écriture brouillon." : $"{brouillons} écriture(s) brouillon — validez ou supprimez."));

        var decls = await _db.Declarations.AsNoTracking()
            .Where(d => d.ClientId == exo.ClientId && d.CabinetId == cabinetId
                        && (d.ExerciceComptableId == exo.Id
                            || (d.ExerciceComptableId == null && d.DateDebut >= exo.DateDebut && d.DateFin <= exo.DateFin)))
            .Select(d => new { d.Id, d.Type, d.Periode, d.Statut })
            .ToListAsync();
        var openDecls = decls.Where(d => !FinalDeclarationStatuts.Contains(d.Statut)).ToList();
        checks.Add(Check("Declarations", "Déclarations non finalisées", openDecls.Count == 0,
            openDecls.Count == 0 ? "Toutes les déclarations sont finalisées."
                : $"{openDecls.Count} déclaration(s) ouvertes: " + string.Join(", ", openDecls.Take(5).Select(d => $"{d.Type}/{d.Periode}"))));

        var paies = await _db.PayrollPeriods.AsNoTracking()
            .Where(p => p.ClientId == exo.ClientId && p.CabinetId == cabinetId
                        && (p.ExerciceComptableId == exo.Id
                            || (p.ExerciceComptableId == null && p.Annee == exo.Annee)))
            .Select(p => new { p.Mois, p.Statut })
            .ToListAsync();
        var openPaie = paies.Where(p => !FinalPaieStatuts.Contains(p.Statut)).ToList();
        checks.Add(Check("Paie", "Périodes de paie non clôturées", openPaie.Count == 0,
            openPaie.Count == 0 ? "Paie OK." : $"{openPaie.Count} période(s) paie non clôturée(s)."));

        var rappro = await _db.RapprochementsBancaires.CountAsync(r =>
            r.ClientId == exo.ClientId && r.CabinetId == cabinetId && !r.EstRapproche
            && (r.ExerciceComptableId == exo.Id || r.ExerciceComptableId == null));
        checks.Add(Check("Rapprochements", "Rapprochements bancaires ouverts", rappro == 0,
            rappro == 0 ? "Aucun rapprochement ouvert." : $"{rappro} rapprochement(s) en cours."));

        var wfRunning = await _db.WorkflowInstances.CountAsync(w =>
            w.ClientId == exo.ClientId && w.CabinetId == cabinetId
            && w.Status == WorkflowInstanceStatus.Active
            && (w.ExerciceComptableId == exo.Id
                || (w.DeclarationId != null && decls.Select(d => d.Id).Contains(w.DeclarationId.Value))));
        checks.Add(Check("Workflows", "Workflows actifs", wfRunning == 0,
            wfRunning == 0 ? "Aucun workflow actif." : $"{wfRunning} workflow(s) encore actif(s)."));

        var bal = await _compta.GetBalanceAsync(exo.ClientId, cabinetId, exo.DateDebut, exo.DateFin, exo.Id);
        var totD = bal.Data?.Sum(x => x.TotalDebit) ?? 0;
        var totC = bal.Data?.Sum(x => x.TotalCredit) ?? 0;
        var balanced = Math.Round(totD - totC, 2) == 0;
        checks.Add(Check("Balance", "Équilibre balance (écritures validées)", balanced,
            balanced ? $"Balance équilibrée (D={totD:N2} C={totC:N2})."
                : $"Écart balance D={totD:N2} C={totC:N2}."));

        var unlocked = await _db.PeriodesComptables.CountAsync(p =>
            p.ExerciceComptableId == exo.Id && !p.EstVerrouillee);
        checks.Add(Check("Periodes", "Périodes mensuelles", true,
            unlocked == 0
                ? "Toutes les périodes sont verrouillées."
                : $"{unlocked} période(s) encore ouvertes — elles seront verrouillées à la clôture."));

        var anPreview = await BuildAnPreviewAsync(exo, cabinetId);
        var dto = new ExerciceCloturePrecheckDto
        {
            ExerciceId = exo.Id,
            ClientId = exo.ClientId,
            Annee = exo.Annee,
            CanCloturer = checks.All(c => c.Ok),
            Checks = checks,
            AnPreview = anPreview
        };
        return ApiResponse<ExerciceCloturePrecheckDto>.Ok(dto);
    }

    public async Task<ApiResponse<ExerciceClotureResultDto>> CloturerAsync(ExerciceClotureRequestDto dto, int cabinetId, string userId)
    {
        var pre = await PrecheckAsync(dto.ExerciceId, cabinetId);
        if (!pre.Success || pre.Data == null)
            return ApiResponse<ExerciceClotureResultDto>.Fail(pre.Message ?? "Pré-contrôle échoué.");
        if (!pre.Data.CanCloturer && !dto.ForceWithReason)
            return ApiResponse<ExerciceClotureResultDto>.Fail("Contrôles bloquants. Corrigez les points rouges ou fournissez une justification ForceWithReason.");
        if (!pre.Data.CanCloturer && string.IsNullOrWhiteSpace(dto.ForceReason))
            return ApiResponse<ExerciceClotureResultDto>.Fail("Justification obligatoire pour forcer la clôture.");

        var exo = await _db.ExercicesComptables
            .Include(e => e.Periodes)
            .FirstOrDefaultAsync(e => e.Id == dto.ExerciceId && e.CabinetId == cabinetId);
        if (exo == null) return ApiResponse<ExerciceClotureResultDto>.Fail("Exercice introuvable.");

        // Lock periods + close current BEFORE opening N+1 (one-open rule)
        foreach (var p in exo.Periodes)
            p.EstVerrouillee = true;

        exo.Statut = "Cloture";
        exo.UpdatedAt = DateTime.UtcNow;
        exo.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        ExerciceComptable? next = null;
        int? anEcritureId = null;

        if (dto.OuvrirExerciceSuivant)
        {
            var nextYear = exo.Annee + 1;
            next = await _db.ExercicesComptables
                .FirstOrDefaultAsync(e => e.ClientId == exo.ClientId && e.CabinetId == cabinetId && e.Annee == nextYear);
            if (next == null)
            {
                var client = await _db.Clients.AsNoTracking().FirstAsync(c => c.Id == exo.ClientId);
                var (d1, d2) = ComputeExerciceDates(nextYear, client.MoisClotureExercice);
                var created = await _compta.CreateExerciceAsync(new ExerciceCreateDto
                {
                    ClientId = exo.ClientId,
                    Annee = nextYear,
                    DateDebut = d1,
                    DateFin = d2
                }, cabinetId, userId);
                if (!created.Success || created.Data == null)
                    return ApiResponse<ExerciceClotureResultDto>.Fail(created.Message ?? "Impossible de créer l'exercice suivant.");
                next = await _db.ExercicesComptables.FirstAsync(e => e.Id == created.Data.Id);
            }
            else if (!string.Equals(next.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            {
                next.Statut = "Ouvert";
                next.UpdatedAt = DateTime.UtcNow;
                next.UpdatedBy = userId;
                await _db.SaveChangesAsync();
            }
        }

        if (dto.ApprouverANouveaux && next != null)
        {
            var lines = dto.AnLignes?.Count > 0
                ? dto.AnLignes
                : pre.Data.AnPreview;
            if (lines.Count >= 2)
            {
                var posted = await PostAnAsync(next, lines, cabinetId, userId);
                if (!posted.Success)
                    return ApiResponse<ExerciceClotureResultDto>.Fail(
                        $"Exercice clôturé mais AN non posté: {posted.Message}");
                anEcritureId = posted.Data;
            }
        }

        if (dto.RolloverServices && next != null)
            await RolloverServicesAsync(exo, next, cabinetId, userId);

        await WriteAuditAsync(cabinetId, userId, "ExerciceCloture",
            $"Clôture exercice {exo.Annee} client {exo.ClientId}. Force={dto.ForceWithReason}. Reason={dto.ForceReason}");

        _logger.LogInformation("Exercice {ExoId} clôturé by {User}", exo.Id, userId);

        return ApiResponse<ExerciceClotureResultDto>.Ok(new ExerciceClotureResultDto
        {
            ExerciceId = exo.Id,
            Statut = exo.Statut,
            NextExerciceId = next?.Id,
            NextExerciceAnnee = next?.Annee,
            AnEcritureId = anEcritureId
        }, $"Exercice {exo.Annee} clôturé.");
    }

    public async Task<ApiResponse<ExerciceComptableDto>> ReouvrirAsync(int exerciceId, int cabinetId, string userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ApiResponse<ExerciceComptableDto>.Fail("Motif de réouverture obligatoire.");

        var exo = await _db.ExercicesComptables.Include(e => e.Periodes)
            .FirstOrDefaultAsync(e => e.Id == exerciceId && e.CabinetId == cabinetId);
        if (exo == null) return ApiResponse<ExerciceComptableDto>.Fail("Exercice introuvable.");
        if (string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<ExerciceComptableDto>.Fail("L'exercice est déjà ouvert.");

        // Multiple open exercices are allowed (réouverture without closing the current one).

        exo.Statut = "Ouvert";
        exo.UpdatedAt = DateTime.UtcNow;
        exo.UpdatedBy = userId;
        foreach (var p in exo.Periodes)
            p.EstVerrouillee = false;
        await _db.SaveChangesAsync();
        await WriteAuditAsync(cabinetId, userId, "ExerciceReouverture",
            $"Réouverture exercice {exo.Annee}: {reason}");

        return ApiResponse<ExerciceComptableDto>.Ok(new ExerciceComptableDto
        {
            Id = exo.Id,
            ClientId = exo.ClientId,
            Annee = exo.Annee,
            DateDebut = exo.DateDebut,
            DateFin = exo.DateFin,
            Statut = exo.Statut
        }, "Exercice réouvert.");
    }

    private async Task<List<AnLignePreviewDto>> BuildAnPreviewAsync(ExerciceComptable exo, int cabinetId)
    {
        var bal = await _compta.GetBalanceAsync(exo.ClientId, cabinetId, exo.DateDebut, exo.DateFin, exo.Id);
        var lines = new List<AnLignePreviewDto>();
        if (bal.Data == null) return lines;

        // Carry bilan accounts (classes 1–5) to next year; skip 6/7 (P&L closed to résultat)
        foreach (var b in bal.Data.Where(x => x.NumeroCompte.Length > 0 && x.NumeroCompte[0] is >= '1' and <= '5'))
        {
            if (b.SoldeDebiteur == 0 && b.SoldeCrediteur == 0) continue;
            lines.Add(new AnLignePreviewDto
            {
                NumCompte = b.NumeroCompte,
                Libelle = b.LibelleCompte,
                SoldeDebiteur = b.SoldeDebiteur,
                SoldeCrediteur = b.SoldeCrediteur
            });
        }
        return lines;
    }

    private async Task<ApiResponse<int>> PostAnAsync(
        ExerciceComptable next, List<AnLignePreviewDto> lines, int cabinetId, string userId)
    {
        var totalD = Math.Round(lines.Sum(l => l.SoldeDebiteur), 2);
        var totalC = Math.Round(lines.Sum(l => l.SoldeCrediteur), 2);
        if (lines.Count < 2 || totalD != totalC || totalD <= 0)
            return ApiResponse<int>.Fail($"AN non équilibré (D={totalD:N2} C={totalC:N2}).");

        var piece = $"AN-{next.Annee}";
        if (await _db.EcrituresComptables.AnyAsync(e =>
                e.ClientId == next.ClientId && e.CabinetId == cabinetId && e.NumeroPiece == piece))
            return ApiResponse<int>.Fail($"Pièce {piece} déjà existante.");

        var created = await _compta.CreateEcritureAsync(new EcritureCreateDto
        {
            ClientId = next.ClientId,
            ExerciceComptableId = next.Id,
            Journal = "AN",
            DateEcriture = next.DateDebut,
            Libelle = "À-nouveaux / report exercice précédent",
            NumeroPiece = piece,
            Reference = "CLOTURE",
            Valider = true,
            Lignes = lines.Select(l => new LigneEcritureDto
            {
                NumCompte = l.NumCompte,
                LibelleCompte = l.Libelle,
                Debit = l.SoldeDebiteur,
                Credit = l.SoldeCrediteur
            }).ToList()
        }, cabinetId, userId);

        if (!created.Success || created.Data == null)
            return ApiResponse<int>.Fail(created.Message ?? "Échec création AN.");
        return ApiResponse<int>.Ok(created.Data.Id);
    }

    private async Task RolloverServicesAsync(ExerciceComptable from, ExerciceComptable to, int cabinetId, string userId)
    {
        var services = await _db.ServiceContrats
            .Where(s => s.ClientId == from.ClientId && s.Actif
                        && (s.ExerciceComptableId == from.Id || s.ExerciceFiscal == from.Annee))
            .ToListAsync();
        foreach (var s in services)
        {
            var exists = await _db.ServiceContrats.AnyAsync(x =>
                x.ClientId == s.ClientId && x.TypeService == s.TypeService
                && (x.ExerciceComptableId == to.Id || x.ExerciceFiscal == to.Annee));
            if (exists) continue;
            _db.ServiceContrats.Add(new ServiceContrat
            {
                ClientId = s.ClientId,
                TypeService = s.TypeService,
                LibelleService = s.LibelleService,
                Categorie = s.Categorie,
                DateDebut = to.DateDebut,
                DateFin = to.DateFin,
                ExerciceFiscal = to.Annee,
                ExerciceComptableId = to.Id,
                ExerciceDebut = to.DateDebut,
                ExerciceFin = to.DateFin,
                Frequence = s.Frequence,
                JourLimiteDeclaration = s.JourLimiteDeclaration,
                DelaiDeclarationMois = s.DelaiDeclarationMois,
                ConfigurationJson = s.ConfigurationJson,
                AutoGenererDeclarations = s.AutoGenererDeclarations,
                Actif = true,
                ResponsableId = s.ResponsableId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task WriteAuditAsync(int cabinetId, string userId, string action, string details)
    {
        try
        {
            _db.AuditLogs.Add(new AuditLog
            {
                CabinetId = cabinetId,
                UserId = userId,
                Action = action,
                EntityType = "ExerciceComptable",
                NewValues = details,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log failed for {Action}", action);
        }
    }

    public static (DateTime debut, DateTime fin) ComputeExerciceDates(int annee, int moisCloture)
    {
        moisCloture = Math.Clamp(moisCloture <= 0 ? 12 : moisCloture, 1, 12);
        if (moisCloture == 12)
            return (new DateTime(annee, 1, 1), new DateTime(annee, 12, 31));
        var debut = new DateTime(annee - 1, moisCloture + 1, 1);
        var fin = new DateTime(annee, moisCloture, DateTime.DaysInMonth(annee, moisCloture));
        return (debut, fin);
    }

    private static ExerciceClotureCheckItemDto Check(string code, string label, bool ok, string message) => new()
    {
        Code = code,
        Label = label,
        Ok = ok,
        Message = message
    };
}
