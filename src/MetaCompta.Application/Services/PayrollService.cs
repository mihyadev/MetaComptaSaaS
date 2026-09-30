using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly MetaComptaDbContext _db;
    private readonly IMapper _mapper;
    private readonly IReferenceDataService _refData;
    private readonly PayrollEngine _engine;
    private readonly IExerciceContextService _exerciceContext;
    private readonly IComptabiliteService _compta;

    public PayrollService(
        MetaComptaDbContext db,
        IMapper mapper,
        IReferenceDataService refData,
        IExerciceContextService exerciceContext,
        IComptabiliteService compta)
    {
        _db = db;
        _mapper = mapper;
        _refData = refData;
        _engine = new PayrollEngine(db);
        _exerciceContext = exerciceContext;
        _compta = compta;
    }

    // ═══════════════════════════════════════════════════════════
    //  PÉRIODES
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<PayrollPeriodDto>>> GetAllPeriodsAsync(
        int cabinetId, int? clientId = null, int? exerciceComptableId = null)
    {
        var query = _db.PayrollPeriods
            .Include(p => p.Client)
            .Where(p => p.Client!.CabinetId == cabinetId);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        if (exerciceComptableId is > 0)
        {
            var exoId = exerciceComptableId.Value;
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exoId && e.CabinetId == cabinetId
                    && (!clientId.HasValue || e.ClientId == clientId.Value));
            if (exo != null)
            {
                var annee = exo.Annee;
                query = query.Where(p =>
                    p.ExerciceComptableId == exoId
                    || (p.ExerciceComptableId == null && p.Annee == annee));
            }
            else
            {
                query = query.Where(p => p.ExerciceComptableId == exoId);
            }
        }

        var periods = await query
            .OrderByDescending(p => p.Annee * 100 + p.Mois)
            .Select(p => new PayrollPeriodDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = p.Client!.RaisonSociale,
                Year = p.Annee,
                Month = p.Mois,
                Status = p.Statut,
                TotalEmployees = p.PaySlips.Count,
                TotalBrutGlobal = p.PaySlips.Sum(s => s.SalaireBrutGlobal),
                TotalNetPay = p.PaySlips.Sum(s => s.SalaireNetAPayer),
                TotalChargesPatronales = p.PaySlips.Sum(s => s.CoutTotalEmployeur),
                DateValidation = p.DateValidation
            })
            .ToListAsync();

        return ApiResponse<List<PayrollPeriodDto>>.Ok(periods);
    }

    public async Task<ApiResponse<PayrollPeriodDto>> InitializeAsync(int clientId, int annee, int mois, int cabinetId, string userId)
    {
        var existing = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.ClientId == clientId && p.Annee == annee && p.Mois == mois);
        if (existing != null) return ApiResponse<PayrollPeriodDto>.Fail("Paie déjà initialisée pour cette période.");

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<PayrollPeriodDto>.Fail("Client non trouvé.");

        var exoGate = await _exerciceContext.RequireOpenExerciceAsync(clientId, cabinetId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<PayrollPeriodDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var employees = await _db.Employees.Where(e => e.ClientId == clientId && e.Statut == RefCodes.StatutEmploye.Actif).ToListAsync();

        var period = new PayrollPeriod
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Annee = annee,
            Mois = mois,
            Statut = RefCodes.StatutPaie.Brouillon,
            CreatedBy = userId
        };
        _db.PayrollPeriods.Add(period);
        await _db.SaveChangesAsync();

        foreach (var emp in employees)
        {
            var primesFixesTotales = (emp.PrimeAnciennete ?? 0) + (emp.PrimeResponsabilite ?? 0) +
                (emp.PrimePresence ?? 0) + (emp.IndemniteTransport ?? 0) + (emp.IndemniteRepresentation ?? 0);
            var paySlip = new PaySlip
            {
                PayrollPeriodId = period.Id,
                EmployeeId = emp.Id,
                CabinetId = cabinetId,
                SalaireBase = emp.SalaireBase,
                PrimesFixesTotales = primesFixesTotales,
                CreatedBy = userId
            };
            _db.PaySlips.Add(paySlip);
        }
        await _db.SaveChangesAsync();

        var result = await _db.PayrollPeriods.Include(p => p.Client).Include(p => p.PaySlips).FirstAsync(p => p.Id == period.Id);
        return ApiResponse<PayrollPeriodDto>.Ok(_mapper.Map<PayrollPeriodDto>(result), $"Paie initialisée pour {employees.Count} employés.");
    }

    public async Task<ApiResponse<PayrollPeriodDto>> GetPeriodAsync(int id, int cabinetId)
    {
        var period = await _db.PayrollPeriods.Include(p => p.Client).Include(p => p.PaySlips).FirstOrDefaultAsync(p => p.Id == id && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<PayrollPeriodDto>.Fail("Période non trouvée.");
        return ApiResponse<PayrollPeriodDto>.Ok(_mapper.Map<PayrollPeriodDto>(period));
    }

    public async Task<ApiResponse<List<PaySlipDto>>> GetPaySlipsAsync(int periodId, int cabinetId)
    {
        var slips = await _db.PaySlips
            .Include(s => s.Employee)
            .Include(s => s.BulletinLignes)
            .Where(s => s.PayrollPeriodId == periodId && s.CabinetId == cabinetId)
            .OrderBy(s => s.Employee!.Nom)
            .ToListAsync();

        var dtos = _mapper.Map<List<PaySlipDto>>(slips);
        for (int i = 0; i < slips.Count; i++)
        {
            dtos[i].Lignes = _mapper.Map<List<BulletinLigneDto>>(
                slips[i].BulletinLignes.OrderBy(l => l.OrdreAffichage).ToList());
        }
        return ApiResponse<List<PaySlipDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PaySlipDto>> UpdateVariablesAsync(int periodId, PaySlipVariablesDto dto, int cabinetId, string userId)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<PaySlipDto>.Fail("P??riode non trouv??e.");

        var slip = await _db.PaySlips.Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.PayrollPeriodId == periodId && s.EmployeeId == dto.EmployeeId && s.CabinetId == cabinetId);
        if (slip == null) return ApiResponse<PaySlipDto>.Fail("Fiche de paie non trouv??e.");

        // Keep legacy PaySlip fields in sync
        slip.HeuresSupp25 = dto.HeuresSupp25;
        slip.HeuresSupp50 = dto.HeuresSupp50;
        slip.HeuresSupp100 = dto.HeuresSupp100;
        slip.PrimesExceptionnelles = dto.PrimesExceptionnelles;
        slip.IndemnitesExceptionnelles = dto.IndemnitesExceptionnelles;
        slip.AvancesSalaire = dto.AvancesSalaire;
        slip.AbsencesNonRemunerees = dto.AbsencesNonRemunerees;
        slip.UpdatedBy = userId;

        // SYNC TO VariablesEmployes - this is what PayrollEngine reads!
        var referenceDate = new DateTime(period.Annee, period.Mois, 1);
        var endOfMonth = new DateTime(period.Annee, period.Mois, DateTime.DaysInMonth(period.Annee, period.Mois));
        var mapping = new Dictionary<string, decimal>
        {
            { "HS_25", dto.HeuresSupp25 },
            { "HS_50", dto.HeuresSupp50 },
            { "HS_100", dto.HeuresSupp100 },
            { "PRIME_EXCEPTIONNELLE", dto.PrimesExceptionnelles },
            { "AVANCE_SALAIRE", dto.AvancesSalaire },
            { "ABSENCE_JOURS", dto.AbsencesNonRemunerees },
        };
        foreach (var (code, val) in mapping)
        {
            var existing = await _db.VariablesEmployes.FirstOrDefaultAsync(v =>
                v.EmployeeId == dto.EmployeeId && v.RubriqueCode == code && v.CabinetId == cabinetId
                && v.DateEffet.Year == period.Annee && v.DateEffet.Month == period.Mois);
            if (existing != null) { existing.Valeur = val; existing.Actif = val != 0; existing.UpdatedBy = userId; }
            else if (val != 0)
            {
                _db.VariablesEmployes.Add(new VariableEmploye
                {
                    CabinetId = cabinetId, EmployeeId = dto.EmployeeId,
                    RubriqueCode = code, Valeur = val, Actif = true,
                    DateEffet = referenceDate, DateFin = endOfMonth, CreatedBy = userId
                });
            }
        }

        await _db.SaveChangesAsync();
        return ApiResponse<PaySlipDto>.Ok(_mapper.Map<PaySlipDto>(slip), "Variables mises ?? jour.");
    }

    public async Task<ApiResponse<List<VariableEmployeDto>>> GetVariablesForPeriodAsync(
        int employeeId, int annee, int mois, int cabinetId)
    {
        var referenceDate = new DateTime(annee, mois, 1);
        var endOfMonth = new DateTime(annee, mois, DateTime.DaysInMonth(annee, mois));
        var rubriques = await _db.Rubriques
            .Where(r => r.CabinetId == cabinetId && r.Actif)
            .OrderBy(r => r.OrdreAffichage).ToListAsync();

        // Load affectations to filter by assigned rubriques
        var affectedIds = await _db.AffectationsRubriques
            .Where(a => a.EmployeeId == employeeId && a.CabinetId == cabinetId && !a.IsDeleted && a.Actif)
            .Select(a => a.RubriqueId)
            .ToListAsync();
        var affectedIdSet = new HashSet<int>(affectedIds);

        var existingVars = await _db.VariablesEmployes
            .Where(v => v.EmployeeId == employeeId && v.CabinetId == cabinetId
                && v.DateEffet <= endOfMonth && (v.DateFin == null || v.DateFin >= referenceDate))
            .ToListAsync();
        // Load affectation default values for fallback
        var affectationDefaults = await _db.AffectationsRubriques
            .Where(a => a.EmployeeId == employeeId && a.CabinetId == cabinetId && !a.IsDeleted && a.Actif)
            .ToDictionaryAsync(a => a.RubriqueCode, a => a.ValeurDefaut);

        var result = new List<VariableEmployeDto>();
        foreach (var r in rubriques)
        {
            // Only show variables for obligatoire rubriques OR assigned ones
            if (!r.Obligatoire && !affectedIdSet.Contains(r.Id)) continue;

            bool usesVar = (r.FormuleBase?.Contains("VAR.") ?? false)
                || (r.FormuleTaux?.Contains("VAR.") ?? false)
                || (r.FormuleMontant?.Contains("VAR.") ?? false)
                || (r.FormuleGain?.Contains("VAR.") ?? false)
                || (r.FormuleRetenue?.Contains("VAR.") ?? false);
            if (!usesVar) continue;
            var ev = existingVars.FirstOrDefault(v => v.RubriqueCode == r.Code);
            var defaultVal = affectationDefaults.GetValueOrDefault(r.Code, 0);
            result.Add(new VariableEmployeDto
            {
                Id = ev?.Id ?? 0, EmployeeId = employeeId,
                RubriqueCode = r.Code, RubriqueLibelle = r.Libelle,
                Valeur = ev?.Valeur ?? defaultVal, Actif = ev?.Actif ?? true,
                DateEffet = ev?.DateEffet ?? referenceDate, DateFin = ev?.DateFin
            });
        }
        return ApiResponse<List<VariableEmployeDto>>.Ok(result);
    }

    public async Task<ApiResponse<bool>> UpdateVariablesBulkAsync(
        int employeeId, int annee, int mois, List<VariableEmployeCreateDto> variables, int cabinetId, string userId)
    {
        var referenceDate = new DateTime(annee, mois, 1);
        var endOfMonth = new DateTime(annee, mois, DateTime.DaysInMonth(annee, mois));
        foreach (var v in variables)
        {
            var existing = await _db.VariablesEmployes.FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId && x.RubriqueCode == v.RubriqueCode && x.CabinetId == cabinetId
                && x.DateEffet.Year == annee && x.DateEffet.Month == mois);
            if (existing != null) { existing.Valeur = v.Valeur; existing.Actif = v.Valeur != 0; existing.UpdatedBy = userId; }
            else if (v.Valeur != 0)
            {
                _db.VariablesEmployes.Add(new VariableEmploye
                {
                    CabinetId = cabinetId, EmployeeId = employeeId,
                    RubriqueCode = v.RubriqueCode, Valeur = v.Valeur, Actif = true,
                    DateEffet = referenceDate, DateFin = endOfMonth, CreatedBy = userId
                });
            }
        }
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.Annee == annee && p.Mois == mois && p.CabinetId == cabinetId);
        if (period != null)
        {
            var slip = await _db.PaySlips.FirstOrDefaultAsync(s => s.PayrollPeriodId == period.Id && s.EmployeeId == employeeId);
            if (slip != null)
            {
                var dict = variables.ToDictionary(x => x.RubriqueCode, x => x.Valeur);
                slip.HeuresSupp25 = dict.GetValueOrDefault("HS_25");
                slip.HeuresSupp50 = dict.GetValueOrDefault("HS_50");
                slip.HeuresSupp100 = dict.GetValueOrDefault("HS_100");
                slip.PrimesExceptionnelles = dict.GetValueOrDefault("PRIME_EXCEPTIONNELLE");
                slip.AvancesSalaire = dict.GetValueOrDefault("AVANCE_SALAIRE");
                slip.AbsencesNonRemunerees = dict.GetValueOrDefault("ABSENCE_JOURS");
                slip.UpdatedBy = userId;
            }
        }
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Variables mises ?? jour.");
    }


    // ===============================================================
    //  AFFECTATION RUBRIQUES (ERP-style: assign rubriques to employees)
    // ===============================================================

    public async Task<ApiResponse<List<AffectationRubriqueDto>>> GetAffectationsAsync(int employeeId, int cabinetId)
    {
        var affectations = await _db.AffectationsRubriques
            .Include(a => a.Rubrique)
            .Where(a => a.EmployeeId == employeeId && a.CabinetId == cabinetId && !a.IsDeleted)
            .OrderBy(a => a.Rubrique!.OrdreAffichage)
            .ToListAsync();
        return ApiResponse<List<AffectationRubriqueDto>>.Ok(_mapper.Map<List<AffectationRubriqueDto>>(affectations));
    }

    public async Task<ApiResponse<List<RubriqueDto>>> GetRubriquesDisponiblesAsync(int employeeId, int cabinetId, int? clientId = null)
    {
        var assignedIds = await _db.AffectationsRubriques
            .Where(a => a.EmployeeId == employeeId && a.CabinetId == cabinetId && !a.IsDeleted)
            .Select(a => a.RubriqueId)
            .ToListAsync();
        var disponibles = await _db.Rubriques
            .Where(r => r.CabinetId == cabinetId && r.Actif && !r.Obligatoire
                && (r.ClientId == null || r.ClientId == clientId)
                && !assignedIds.Contains(r.Id))
            .OrderBy(r => r.Section).ThenBy(r => r.OrdreAffichage)
            .ToListAsync();
        return ApiResponse<List<RubriqueDto>>.Ok(_mapper.Map<List<RubriqueDto>>(disponibles));
    }

    public async Task<ApiResponse<AffectationRubriqueDto>> AddAffectationAsync(AffectationRubriqueCreateDto dto, int cabinetId, string userId)
    {
        var exists = await _db.AffectationsRubriques.AnyAsync(a =>
            a.EmployeeId == dto.EmployeeId && a.RubriqueId == dto.RubriqueId
            && a.CabinetId == cabinetId && !a.IsDeleted);
        if (exists) return ApiResponse<AffectationRubriqueDto>.Fail("Cette rubrique est deja affectee a cet employe.");

        var rubrique = await _db.Rubriques.FindAsync(dto.RubriqueId);
        if (rubrique == null) return ApiResponse<AffectationRubriqueDto>.Fail("Rubrique non trouvee.");

        var affectation = new AffectationRubrique
        {
            CabinetId = cabinetId, EmployeeId = dto.EmployeeId,
            RubriqueId = dto.RubriqueId, RubriqueCode = rubrique.Code,
            ValeurDefaut = dto.ValeurDefaut, Actif = dto.Actif,
            DateEffet = dto.DateEffet ?? DateTime.UtcNow, DateFin = dto.DateFin, CreatedBy = userId
        };
        _db.AffectationsRubriques.Add(affectation);
        await _db.SaveChangesAsync();
        await _db.Entry(affectation).Reference(a => a.Rubrique).LoadAsync();
        return ApiResponse<AffectationRubriqueDto>.Ok(_mapper.Map<AffectationRubriqueDto>(affectation), "Rubrique affectee avec succes.");
    }

    public async Task<ApiResponse<AffectationRubriqueDto>> UpdateAffectationAsync(int id, AffectationRubriqueCreateDto dto, int cabinetId, string userId)
    {
        var affectation = await _db.AffectationsRubriques.Include(a => a.Rubrique)
            .FirstOrDefaultAsync(a => a.Id == id && a.CabinetId == cabinetId && !a.IsDeleted);
        if (affectation == null) return ApiResponse<AffectationRubriqueDto>.Fail("Affectation non trouvee.");
        affectation.ValeurDefaut = dto.ValeurDefaut;
        affectation.Actif = dto.Actif;
        affectation.DateEffet = dto.DateEffet ?? affectation.DateEffet;
        affectation.DateFin = dto.DateFin;
        affectation.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<AffectationRubriqueDto>.Ok(_mapper.Map<AffectationRubriqueDto>(affectation), "Affectation mise a jour.");
    }

    public async Task<ApiResponse<bool>> RemoveAffectationAsync(int id, int cabinetId, string userId)
    {
        var affectation = await _db.AffectationsRubriques
            .FirstOrDefaultAsync(a => a.Id == id && a.CabinetId == cabinetId && !a.IsDeleted);
        if (affectation == null) return ApiResponse<bool>.Fail("Affectation non trouvee.");
        affectation.IsDeleted = true;
        affectation.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Affectation supprimee.");
    }

    //  CALCUL (Engine-based)
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<PayrollPeriodDto>> CalculateAllAsync(int periodId, int cabinetId, string userId)
    {
        var period = await _db.PayrollPeriods
            .Include(p => p.PaySlips).ThenInclude(s => s.Employee)
            .Include(p => p.PaySlips).ThenInclude(s => s.BulletinLignes)
            .FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<PayrollPeriodDto>.Fail("Période non trouvée.");

        foreach (var slip in period.PaySlips)
        {
            var emp = slip.Employee!;

            // Clear previous bulletin lines
            _db.BulletinLignes.RemoveRange(slip.BulletinLignes);

            // Run engine
            var results = await _engine.CalculateAsync(emp, cabinetId, period.Annee, period.Mois, period.ClientId);

            // Create bulletin lines
            foreach (var r in results)
            {
                var ligne = new BulletinLigne
                {
                    PaySlipId = slip.Id,
                    RubriqueId = r.RubriqueId,
                    RubriqueCode = r.Code,
                    Libelle = r.Libelle,
                    Section = r.Section,
                    Type = r.Type,
                    OrdreAffichage = r.OrdreAffichage,
                    Base = r.Base,
                    Taux = r.Taux,
                    Gain = r.Gain,
                    Retenue = r.Retenue,
                    AfficherSurBulletin = r.AfficherSurBulletin
                };
                _db.BulletinLignes.Add(ligne);
            }

            // Update legacy PaySlip summary fields for backward compatibility
            var gains = results.Where(r => r.Gain > 0);
            var retenues = results.Where(r => r.Retenue > 0);

            slip.SalaireBrutGlobal = results.FirstOrDefault(r => r.Code == "BRUT_GLOBAL")?.Montant
                ?? gains.Sum(r => r.Gain);

            slip.CotisationCNSS = results.FirstOrDefault(r => r.Code == "CNSS_SAL")?.Retenue ?? 0;
            slip.CotisationAMO = results.FirstOrDefault(r => r.Code == "AMO_SAL")?.Retenue ?? 0;
            slip.CotisationCIMR = results.FirstOrDefault(r => r.Code == "CIMR_SAL")?.Retenue ?? 0;

            slip.SalaireNetImposable = results.FirstOrDefault(r => r.Code == "NET_IMPOSABLE")?.Montant
                ?? (slip.SalaireBrutGlobal - slip.CotisationCNSS - slip.CotisationAMO - slip.CotisationCIMR);

            slip.IRRetenu = results.FirstOrDefault(r => r.Code == "IR")?.Retenue ?? 0;

            slip.SalaireNetAPayer = results.FirstOrDefault(r => r.Code == "NET_A_PAYER")?.Montant
                ?? (slip.SalaireBrutGlobal - retenues.Sum(r => r.Retenue));

            slip.CotisationPatronaleCNSS = results.FirstOrDefault(r => r.Code == "CNSS_PAT")?.Montant ?? 0;
            slip.CotisationPatronaleAMO = results.FirstOrDefault(r => r.Code == "AMO_PAT")?.Montant ?? 0;
            slip.FormationProfessionnelle = results.FirstOrDefault(r => r.Code == "FP_PAT")?.Montant ?? 0;
            slip.CoutTotalEmployeur = slip.SalaireBrutGlobal + slip.CotisationPatronaleCNSS +
                slip.CotisationPatronaleAMO + slip.FormationProfessionnelle;

            // Update cumuls
            await UpdateCumulsAsync(emp.Id, cabinetId, period.Annee, period.Mois, results);
        }

        // Update period totals
        period.TotalSalairesBruts = period.PaySlips.Sum(s => s.SalaireBrutGlobal);
        period.TotalCotisationsSalariales = period.PaySlips.Sum(s => s.CotisationCNSS + s.CotisationAMO + s.CotisationCIMR);
        period.TotalCotisationsPatronales = period.PaySlips.Sum(s => s.CotisationPatronaleCNSS + s.CotisationPatronaleAMO + s.FormationProfessionnelle);
        period.TotalIRRetenu = period.PaySlips.Sum(s => s.IRRetenu);
        period.TotalNetAPayer = period.PaySlips.Sum(s => s.SalaireNetAPayer);
        period.TotalCoutEmployeur = period.PaySlips.Sum(s => s.CoutTotalEmployeur);
        period.Statut = RefCodes.StatutPaie.EnCours;

        await _db.SaveChangesAsync();

        var result = await _db.PayrollPeriods.Include(p => p.Client).Include(p => p.PaySlips).FirstAsync(p => p.Id == periodId);
        return ApiResponse<PayrollPeriodDto>.Ok(_mapper.Map<PayrollPeriodDto>(result), "Calculs effectués avec succès.");
    }

    private async Task UpdateCumulsAsync(int employeeId, int cabinetId, int annee, int mois, List<RubriqueResult> results)
    {
        // Remove existing cumuls for this month
        var existing = await _db.CumulsEmployes
            .Where(c => c.EmployeeId == employeeId && c.Annee == annee && c.Mois == mois)
            .ToListAsync();
        _db.CumulsEmployes.RemoveRange(existing);

        // Add new cumuls
        foreach (var r in results.Where(r => r.Montant != 0))
        {
            _db.CumulsEmployes.Add(new CumulEmploye
            {
                EmployeeId = employeeId,
                CabinetId = cabinetId,
                RubriqueCode = r.Code,
                Annee = annee,
                Mois = mois,
                Valeur = r.Montant
            });
        }
    }

    public async Task<ApiResponse<bool>> ValidateAsync(int periodId, int cabinetId, string userId)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<bool>.Fail("Période non trouvée.");
        if (period.Statut == RefCodes.StatutPaie.Validee) return ApiResponse<bool>.Fail("Paie déjà validée.");

        period.Statut = RefCodes.StatutPaie.Validee;
        period.ValidateurId = userId;
        period.DateValidation = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Pont paie → journal SA (idempotent)
        var post = await _compta.PostPayrollPeriodAsync(periodId, cabinetId, userId);
        if (post.Success)
            return ApiResponse<bool>.Ok(true, "Paie validée — écriture journal SA générée.");
        return ApiResponse<bool>.Ok(true, $"Paie validée. Écriture comptable : {post.Message}");
    }

    public async Task<ApiResponse<string>> GetLivrePaieCsvAsync(int periodId, int cabinetId)
    {
        var period = await _db.PayrollPeriods
            .Include(p => p.Client)
            .Include(p => p.PaySlips).ThenInclude(s => s.Employee)
            .FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<string>.Fail("Période non trouvée.");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("CIN;Nom;Prenom;Brut;CotisationsSalariales;IR;NetAPayer;CotisationsPatronales");
        foreach (var s in period.PaySlips.OrderBy(x => x.Employee?.Nom))
        {
            var emp = s.Employee;
            sb.Append(emp?.CIN ?? emp?.Id.ToString() ?? "").Append(';')
                .Append(emp?.Nom ?? "").Append(';')
                .Append(emp?.Prenom ?? "").Append(';')
                .Append(s.SalaireBrutGlobal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                .Append(s.CotisationCNSS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                .Append(s.IRRetenu.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                .Append(s.SalaireNetAPayer.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                .Append(s.CotisationPatronaleCNSS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                .AppendLine();
        }
        period.LivrePaieUrl = $"livre-paie-{period.Annee}-{period.Mois:D2}.csv";
        period.JournalComptableUrl = $"PAIE-{period.Id}";
        await _db.SaveChangesAsync();
        return ApiResponse<string>.Ok(sb.ToString(), "Livre de paie généré.");
    }

    // ═══════════════════════════════════════════════════════════
    //  BULLETIN COMPLET
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<BulletinCompletDto>> GetBulletinAsync(int paySlipId, int cabinetId)
    {
        var slip = await _db.PaySlips
            .Include(s => s.Employee)
            .Include(s => s.BulletinLignes).ThenInclude(l => l.Rubrique)
            .Include(s => s.PayrollPeriod).ThenInclude(p => p.Client)
            .FirstOrDefaultAsync(s => s.Id == paySlipId && s.CabinetId == cabinetId);
        if (slip == null) return ApiResponse<BulletinCompletDto>.Fail("Bulletin non trouvé.");

        var emp = slip.Employee!;
        var client = slip.PayrollPeriod!.Client!;

        var bulletin = new BulletinCompletDto
        {
            EmployeurRaisonSociale = client.RaisonSociale,
            EmployeurICE = client.ICE,
            EmployeurIF = client.IF,
            EmployeurCNSS = client.NumeroCNSS,
            EmployeurAdresse = $"{client.AdresseSiegeSocial}, {client.VilleSiegeSocial}",

            EmployeeId = emp.Id,
            EmployeeNom = emp.FullName,
            EmployeeCIN = emp.CIN,
            EmployeeMatricule = emp.MatriculeEmployeur,
            EmployeeCNSS = emp.NumeroCNSS,
            EmployeePoste = emp.Poste,
            EmployeeDateEmbauche = emp.DateEmbauche,
            EmployeeSituationFamiliale = emp.SituationFamiliale,
            EmployeeNbEnfants = emp.NombreEnfantsACharge,

            Mois = slip.PayrollPeriod.Mois,
            Annee = slip.PayrollPeriod.Annee,
            PeriodeLabel = $"{slip.PayrollPeriod.Mois:D2}/{slip.PayrollPeriod.Annee}",

            TotalGains = slip.BulletinLignes.Where(l => l.Gain > 0).Sum(l => l.Gain),
            TotalRetenues = slip.BulletinLignes.Where(l => l.Retenue > 0).Sum(l => l.Retenue),
            SalaireBrut = slip.SalaireBrutGlobal,
            SalaireNetImposable = slip.SalaireNetImposable,
            SalaireNet = slip.SalaireNetAPayer,
            TotalChargesPatronales = slip.CotisationPatronaleCNSS + slip.CotisationPatronaleAMO + slip.FormationProfessionnelle,
            CoutTotalEmployeur = slip.CoutTotalEmployeur,

            CumulBrutAnnuel = slip.CumulBrutAnnuel,
            CumulNetImposableAnnuel = slip.CumulNetImposableAnnuel,
            CumulIRAnnuel = slip.CumulIRAnnuel,
            CumulCNSSAnnuel = slip.CumulCNSSAnnuel
        };

        // Group lines by section
        var sectionLabels = new Dictionary<string, string>
        {
            { "Salaire", "Salaire de base" },
            { "Prime", "Primes & Indemnités" },
            { "Avantage", "Avantages en nature" },
            { "Cotisation", "Cotisations sociales" },
            { "Impot", "Impôt sur le revenu" },
            { "Net", "Net à payer" },
            { "Patronal", "Charges patronales" }
        };

        var visibleLignes = slip.BulletinLignes
            .Where(l => l.AfficherSurBulletin)
            .OrderBy(l => l.OrdreAffichage)
            .ToList();

        var sectionOrder = new[] { "Salaire", "Prime", "Avantage", "Cotisation", "Impot", "Net", "Patronal" };
        foreach (var section in sectionOrder)
        {
            var lignes = visibleLignes.Where(l => l.Section == section).ToList();
            if (lignes.Any())
            {
                bulletin.Sections.Add(new BulletinSectionDto
                {
                    Section = section,
                    SectionLabel = sectionLabels.GetValueOrDefault(section, section),
                    Lignes = _mapper.Map<List<BulletinLigneDto>>(lignes),
                    TotalGains = lignes.Sum(l => l.Gain),
                    TotalRetenues = lignes.Sum(l => l.Retenue)
                });
            }
        }

        return ApiResponse<BulletinCompletDto>.Ok(bulletin);
    }

    // ═══════════════════════════════════════════════════════════
    //  RUBRIQUES CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<RubriqueDto>>> GetRubriquesAsync(int cabinetId, int? clientId = null)
    {
        var query = _db.Rubriques.Where(r => r.CabinetId == cabinetId);
        if (clientId.HasValue)
            query = query.Where(r => r.ClientId == null || r.ClientId == clientId.Value);
        else
            query = query.Where(r => r.ClientId == null);

        var rubriques = await query.OrderBy(r => r.OrdreExecution).ToListAsync();
        return ApiResponse<List<RubriqueDto>>.Ok(_mapper.Map<List<RubriqueDto>>(rubriques));
    }

    public async Task<ApiResponse<RubriqueDto>> GetRubriqueAsync(int id, int cabinetId)
    {
        var rubrique = await _db.Rubriques.FirstOrDefaultAsync(r => r.Id == id && r.CabinetId == cabinetId);
        if (rubrique == null) return ApiResponse<RubriqueDto>.Fail("Rubrique non trouvée.");
        return ApiResponse<RubriqueDto>.Ok(_mapper.Map<RubriqueDto>(rubrique));
    }

    public async Task<ApiResponse<RubriqueDto>> CreateRubriqueAsync(RubriqueCreateDto dto, int cabinetId, string userId)
    {
        var exists = await _db.Rubriques.AnyAsync(r => r.CabinetId == cabinetId && r.Code == dto.Code);
        if (exists) return ApiResponse<RubriqueDto>.Fail($"Une rubrique avec le code '{dto.Code}' existe déjà.");

        var rubrique = _mapper.Map<Rubrique>(dto);
        rubrique.CabinetId = cabinetId;
        rubrique.CreatedBy = userId;
        _db.Rubriques.Add(rubrique);
        await _db.SaveChangesAsync();

        return ApiResponse<RubriqueDto>.Ok(_mapper.Map<RubriqueDto>(rubrique), "Rubrique créée.");
    }

    public async Task<ApiResponse<RubriqueDto>> UpdateRubriqueAsync(int id, RubriqueCreateDto dto, int cabinetId, string userId)
    {
        var rubrique = await _db.Rubriques.FirstOrDefaultAsync(r => r.Id == id && r.CabinetId == cabinetId);
        if (rubrique == null) return ApiResponse<RubriqueDto>.Fail("Rubrique non trouvée.");
        if (rubrique.Systeme) return ApiResponse<RubriqueDto>.Fail("Impossible de modifier une rubrique système.");

        rubrique.Libelle = dto.Libelle;
        rubrique.Type = dto.Type;
        rubrique.Section = dto.Section;
        rubrique.OrdreExecution = dto.OrdreExecution;
        rubrique.OrdreAffichage = dto.OrdreAffichage;
        rubrique.FormuleBase = dto.FormuleBase;
        rubrique.FormuleTaux = dto.FormuleTaux;
        rubrique.FormuleMontant = dto.FormuleMontant;
        rubrique.FormuleGain = dto.FormuleGain;
        rubrique.FormuleRetenue = dto.FormuleRetenue;
        rubrique.DependsDe = dto.DependsDe;
        rubrique.AfficherSurBulletin = dto.AfficherSurBulletin;
        rubrique.Obligatoire = dto.Obligatoire;
        rubrique.Description = dto.Description;
        rubrique.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        return ApiResponse<RubriqueDto>.Ok(_mapper.Map<RubriqueDto>(rubrique), "Rubrique mise à jour.");
    }

    public async Task<ApiResponse<bool>> ToggleRubriqueAsync(int id, int cabinetId, string userId)
    {
        var rubrique = await _db.Rubriques.FirstOrDefaultAsync(r => r.Id == id && r.CabinetId == cabinetId);
        if (rubrique == null) return ApiResponse<bool>.Fail("Rubrique non trouvée.");
        if (rubrique.Obligatoire) return ApiResponse<bool>.Fail("Impossible de désactiver une rubrique obligatoire.");

        rubrique.Actif = !rubrique.Actif;
        rubrique.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, rubrique.Actif ? "Rubrique activée." : "Rubrique désactivée.");
    }

    public async Task<ApiResponse<bool>> DeleteRubriqueAsync(int id, int cabinetId, string userId)
    {
        var rubrique = await _db.Rubriques.FirstOrDefaultAsync(r => r.Id == id && r.CabinetId == cabinetId);
        if (rubrique == null) return ApiResponse<bool>.Fail("Rubrique non trouvée.");
        if (rubrique.Systeme) return ApiResponse<bool>.Fail("Impossible de supprimer une rubrique système.");

        _db.Rubriques.Remove(rubrique);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Rubrique supprimée.");
    }

    // ═══════════════════════════════════════════════════════════
    //  CONSTANTES GLOBALES CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<ConstanteGlobaleDto>>> GetConstantesAsync(int cabinetId, string? categorie = null)
    {
        var query = _db.ConstantesGlobales.Where(c => c.CabinetId == cabinetId && c.Actif);
        if (!string.IsNullOrEmpty(categorie))
            query = query.Where(c => c.Categorie == categorie);

        var constantes = await query.OrderBy(c => c.Categorie).ThenBy(c => c.Code).ToListAsync();
        return ApiResponse<List<ConstanteGlobaleDto>>.Ok(_mapper.Map<List<ConstanteGlobaleDto>>(constantes));
    }

    public async Task<ApiResponse<ConstanteGlobaleDto>> CreateConstanteAsync(ConstanteGlobaleDto dto, int cabinetId, string userId)
    {
        var constante = new ConstanteGlobale
        {
            CabinetId = cabinetId,
            Code = dto.Code,
            Libelle = dto.Libelle,
            Categorie = dto.Categorie,
            Valeur = dto.Valeur,
            DateEffet = dto.DateEffet,
            DateFin = dto.DateFin,
            Actif = true,
            Description = dto.Description,
            CreatedBy = userId
        };
        _db.ConstantesGlobales.Add(constante);
        await _db.SaveChangesAsync();

        return ApiResponse<ConstanteGlobaleDto>.Ok(_mapper.Map<ConstanteGlobaleDto>(constante), "Constante créée.");
    }

    public async Task<ApiResponse<ConstanteGlobaleDto>> UpdateConstanteAsync(int id, ConstanteGlobaleDto dto, int cabinetId, string userId)
    {
        var constante = await _db.ConstantesGlobales.FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);
        if (constante == null) return ApiResponse<ConstanteGlobaleDto>.Fail("Constante non trouvée.");

        constante.Libelle = dto.Libelle;
        constante.Categorie = dto.Categorie;
        constante.Valeur = dto.Valeur;
        constante.DateEffet = dto.DateEffet;
        constante.DateFin = dto.DateFin;
        constante.Description = dto.Description;
        constante.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        return ApiResponse<ConstanteGlobaleDto>.Ok(_mapper.Map<ConstanteGlobaleDto>(constante), "Constante mise à jour.");
    }

    public async Task<ApiResponse<bool>> DeleteConstanteAsync(int id, int cabinetId, string userId)
    {
        var constante = await _db.ConstantesGlobales.FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);
        if (constante == null) return ApiResponse<bool>.Fail("Constante non trouvée.");

        constante.Actif = false;
        constante.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Constante désactivée.");
    }

    // ═══════════════════════════════════════════════════════════
    //  VARIABLES EMPLOYE CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<VariableEmployeDto>>> GetVariablesEmployeAsync(int employeeId, int cabinetId)
    {
        var variables = await _db.VariablesEmployes
            .Include(v => v.Employee)
            .Where(v => v.EmployeeId == employeeId && v.CabinetId == cabinetId && v.Actif)
            .OrderBy(v => v.RubriqueCode)
            .ToListAsync();

        var dtos = _mapper.Map<List<VariableEmployeDto>>(variables);

        // Enrich with rubrique libelle
        var codes = variables.Select(v => v.RubriqueCode).Distinct().ToList();
        var rubriques = await _db.Rubriques
            .Where(r => r.CabinetId == cabinetId && codes.Contains(r.Code))
            .ToDictionaryAsync(r => r.Code, r => r.Libelle);
        foreach (var d in dtos)
            d.RubriqueLibelle = rubriques.GetValueOrDefault(d.RubriqueCode);

        return ApiResponse<List<VariableEmployeDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<VariableEmployeDto>> CreateVariableAsync(VariableEmployeCreateDto dto, int cabinetId, string userId)
    {
        var variable = _mapper.Map<VariableEmploye>(dto);
        variable.CabinetId = cabinetId;
        variable.DateEffet = dto.DateEffet ?? DateTime.UtcNow;
        variable.CreatedBy = userId;
        _db.VariablesEmployes.Add(variable);
        await _db.SaveChangesAsync();

        return ApiResponse<VariableEmployeDto>.Ok(_mapper.Map<VariableEmployeDto>(variable), "Variable créée.");
    }

    public async Task<ApiResponse<VariableEmployeDto>> UpdateVariableAsync(int id, VariableEmployeCreateDto dto, int cabinetId, string userId)
    {
        var variable = await _db.VariablesEmployes.FirstOrDefaultAsync(v => v.Id == id && v.CabinetId == cabinetId);
        if (variable == null) return ApiResponse<VariableEmployeDto>.Fail("Variable non trouvée.");

        variable.RubriqueCode = dto.RubriqueCode;
        variable.Valeur = dto.Valeur;
        variable.LibellePersonnalise = dto.LibellePersonnalise;
        variable.Actif = dto.Actif;
        variable.DateEffet = dto.DateEffet ?? variable.DateEffet;
        variable.DateFin = dto.DateFin;
        variable.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        return ApiResponse<VariableEmployeDto>.Ok(_mapper.Map<VariableEmployeDto>(variable), "Variable mise à jour.");
    }

    public async Task<ApiResponse<bool>> DeleteVariableAsync(int id, int cabinetId, string userId)
    {
        var variable = await _db.VariablesEmployes.FirstOrDefaultAsync(v => v.Id == id && v.CabinetId == cabinetId);
        if (variable == null) return ApiResponse<bool>.Fail("Variable non trouvée.");

        variable.Actif = false;
        variable.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Variable désactivée.");
    }

    // ═══════════════════════════════════════════════════════════
    //  CUMULS
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<CumulEmployeDto>>> GetCumulsAsync(int employeeId, int annee, int cabinetId)
    {
        var cumuls = await _db.CumulsEmployes
            .Where(c => c.EmployeeId == employeeId && c.Annee == annee && c.CabinetId == cabinetId)
            .OrderBy(c => c.Mois).ThenBy(c => c.RubriqueCode)
            .ToListAsync();

        return ApiResponse<List<CumulEmployeDto>>.Ok(_mapper.Map<List<CumulEmployeDto>>(cumuls));
    }

    // ═══════════════════════════════════════════════════════════
    //  SEED DATA
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<bool>> SeedDefaultRubriquesAsync(int cabinetId, string userId)
    {
        var existing = await _db.Rubriques.AnyAsync(r => r.CabinetId == cabinetId);
        if (existing) return ApiResponse<bool>.Fail("Des rubriques existent déjà pour ce cabinet.");

        var rubriques = GetDefaultRubriques(cabinetId, userId);
        _db.Rubriques.AddRange(rubriques);
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"{rubriques.Count} rubriques standards créées.");
    }

    public async Task<ApiResponse<bool>> SeedDefaultConstantesAsync(int cabinetId, string userId)
    {
        var existing = await _db.ConstantesGlobales.AnyAsync(c => c.CabinetId == cabinetId);
        if (existing) return ApiResponse<bool>.Fail("Des constantes existent déjà pour ce cabinet.");

        var constantes = GetDefaultConstantes(cabinetId, userId);
        _db.ConstantesGlobales.AddRange(constantes);
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"{constantes.Count} constantes standards créées.");
    }


    // ═══════════════════════════════════════════════════════════
    //  SEED DÉMO 2025 — 3 Employés × 12 mois
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<bool>> SeedDemo2025Async(int clientId, int cabinetId, string userId)
    {
        try
        {
            // 1. Seed rubriques & constantes if not present
            if (!await _db.Rubriques.AnyAsync(r => r.CabinetId == cabinetId))
                await SeedDefaultRubriquesAsync(cabinetId, userId);
            if (!await _db.ConstantesGlobales.AnyAsync(c => c.CabinetId == cabinetId))
                await SeedDefaultConstantesAsync(cabinetId, userId);

            // 2. Create 3 demo employees if they don't exist
            var employees = new[]
            {
                new { CIN = "D123456", First = "Youssef", Last = "BENALI", Salaire = 4500m, Situation = "Celibataire", Enfants = 0, Poste = "Ouvrier", MatCNSS = "CNSS-D123" },
                new { CIN = "E789012", First = "Fatima", Last = "EL IDRISSI", Salaire = 9000m, Situation = "Marie", Enfants = 2, Poste = "Cadre Moyen", MatCNSS = "CNSS-E789" },
                new { CIN = "F345678", First = "Karim", Last = "AMRANI", Salaire = 25000m, Situation = "Marie", Enfants = 3, Poste = "Directeur", MatCNSS = "CNSS-F345" },
            };

            var empIds = new List<int>();
            foreach (var e in employees)
            {
                var existing = await _db.Employees.FirstOrDefaultAsync(x => x.CIN == e.CIN && x.ClientId == clientId);
                if (existing != null) { empIds.Add(existing.Id); continue; }

                var emp = new Employee
                {
                    CabinetId = cabinetId, ClientId = clientId,
                    Prenom = e.First, Nom = e.Last, CIN = e.CIN,
                    NumeroCNSS = e.MatCNSS,
                    SituationFamiliale = e.Situation,
                    NombreEnfantsACharge = e.Enfants,
                    Poste = e.Poste,
                    DateEmbauche = new DateTime(2020, 1, 1),
                    SalaireBase = e.Salaire,
                    Statut = "Actif",
                    CreatedBy = userId
                };
                _db.Employees.Add(emp);
                await _db.SaveChangesAsync();
                empIds.Add(emp.Id);
            }

            // 3. Create Affectation Rubriques for each employee (ERP-style assignment)
            var allRubriques = await _db.Rubriques.Where(r => r.CabinetId == cabinetId && r.Actif).ToListAsync();

            var affectationProfiles = new Dictionary<int, List<(string code, decimal valDefaut)>>
            {
                [0] = new() { ("PRIME_TRANSPORT", 500m) },
                [1] = new() { ("PRIME_TRANSPORT", 700m), ("PRIME_ANCIENNETE", 5m), ("PRIME_RESPONSABILITE", 1000m), ("PRIME_EXCEPTIONNELLE", 0m) },
                [2] = new() { ("PRIME_TRANSPORT", 1500m), ("PRIME_ANCIENNETE", 5m), ("PRIME_RESPONSABILITE", 3000m), ("HS_25", 0m), ("PRIME_EXCEPTIONNELLE", 0m), ("PRIME_PRESENCE", 0m) },
            };

            for (int ei = 0; ei < 3; ei++)
            {
                var empId = empIds[ei];
                foreach (var (code, valDefaut) in affectationProfiles[ei])
                {
                    var rubrique = allRubriques.FirstOrDefault(r => r.Code == code);
                    if (rubrique == null) continue;
                    var exists = await _db.AffectationsRubriques.AnyAsync(a =>
                        a.EmployeeId == empId && a.RubriqueId == rubrique.Id && a.CabinetId == cabinetId && !a.IsDeleted);
                    if (!exists)
                    {
                        _db.AffectationsRubriques.Add(new AffectationRubrique
                        {
                            CabinetId = cabinetId, EmployeeId = empId,
                            RubriqueId = rubrique.Id, RubriqueCode = code,
                            ValeurDefaut = valDefaut, Actif = true,
                            DateEffet = new DateTime(2025, 1, 1),
                            CreatedBy = userId
                        });
                    }
                }
            }
            await _db.SaveChangesAsync();

            // 4. Create VariableEmploye entries for each employee and month
            var rubriques = await _db.Rubriques.Where(r => r.CabinetId == cabinetId && r.Actif).ToListAsync();

            for (int m = 1; m <= 12; m++)
            {
                for (int ei = 0; ei < 3; ei++)
                {
                    var empId = empIds[ei];
                    var sal = employees[ei].Salaire;

                    // Base variables for this employee/month
                    var vars = new List<(string code, decimal val)>
                    {
                        ("SAL_BASE", sal),
                        ("PRIME_TRANSPORT", ei == 0 ? 500m : ei == 1 ? 700m : 1500m),
                    };

                    // Medium: ancienneté + responsabilité
                    if (ei >= 1)
                    {
                        vars.Add(("PRIME_ANCIENNETE", 5m)); // 5% — formula: SAL_BASE * VAR / 100
                        vars.Add(("PRIME_RESPONSABILITE", ei == 1 ? 1000m : 3000m));
                    }

                    // Complex: heures sup varies by month, bonus June + Dec
                    if (ei == 2)
                    {
                        decimal hSup = m switch { 1 or 2 => 20, 3 or 4 => 15, 5 or 6 => 25, 7 or 8 => 10, 9 or 10 => 30, _ => 18 };
                        vars.Add(("HS_25", hSup)); // matches rubrique code HS_25
                        if (m == 6) vars.Add(("PRIME_EXCEPTIONNELLE", 5000m));
                        if (m == 12) vars.Add(("PRIME_EXCEPTIONNELLE", 10000m));
                    }
                    // Medium: December bonus
                    if (ei == 1 && m == 12) vars.Add(("PRIME_EXCEPTIONNELLE", 3000m));

                    // Only seed if not already present
                    foreach (var (code, val) in vars)
                    {
                        var exists = await _db.VariablesEmployes.AnyAsync(v =>
                            v.EmployeeId == empId && v.RubriqueCode == code && v.CabinetId == cabinetId
                            && v.DateEffet.Year == 2025 && v.DateEffet.Month == m);
                        if (!exists)
                        {
                            _db.VariablesEmployes.Add(new VariableEmploye
                            {
                                CabinetId = cabinetId, EmployeeId = empId,
                                RubriqueCode = code, Valeur = val, Actif = true,
                                DateEffet = new DateTime(2025, m, 1),
                                DateFin = new DateTime(2025, m, DateTime.DaysInMonth(2025, m)),
                                CreatedBy = userId
                            });
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }

            // 5. Initialize, calculate, and validate 12 periods
            for (int m = 1; m <= 12; m++)
            {
                var existing = await _db.PayrollPeriods.FirstOrDefaultAsync(p =>
                    p.ClientId == clientId && p.Annee == 2025 && p.Mois == m && p.CabinetId == cabinetId);
                if (existing == null)
                {
                    await InitializeAsync(clientId, 2025, m, cabinetId, userId);
                    existing = await _db.PayrollPeriods.FirstOrDefaultAsync(p =>
                        p.ClientId == clientId && p.Annee == 2025 && p.Mois == m && p.CabinetId == cabinetId);
                }
                if (existing != null && existing.Statut != "Validee")
                {
                    await CalculateAllAsync(existing.Id, cabinetId, userId);
                    await ValidateAsync(existing.Id, cabinetId, userId);
                }
            }

            return ApiResponse<bool>.Ok(true, "Démo 2025 créée: 3 employés × 12 mois calculés et validés.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Erreur démo: {ex.Message}");
        }
    }
    // ═══════════════════════════════════════════════════════════
    //  DEFAULT SEED DATA — RUBRIQUES MAROCAINES STANDARDS
    // ═══════════════════════════════════════════════════════════

    private static List<Rubrique> GetDefaultRubriques(int cabinetId, string userId)
    {
        int exec = 0, aff = 0;
        return new List<Rubrique>
        {
            // --- GAINS : Salaire ---
            new Rubrique { CabinetId = cabinetId, Code = "SAL_BASE", Libelle = "Salaire de base", Type = "Gain", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleBase = "EMP.SALAIRE_BASE", FormuleTaux = "", FormuleMontant = "EMP.SALAIRE_BASE", FormuleGain = "EMP.SALAIRE_BASE",
                CreatedBy = userId },

            // --- GAINS : Primes fixes ---
            new Rubrique { CabinetId = cabinetId, Code = "PRIME_ANCIENNETE", Libelle = "Prime d'ancienneté", Type = "Gain", Section = "Prime",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "EMP.SALAIRE_BASE", FormuleTaux = "VAR.PRIME_ANCIENNETE", FormuleMontant = "EMP.SALAIRE_BASE * VAR.PRIME_ANCIENNETE / 100", FormuleGain = "EMP.SALAIRE_BASE * VAR.PRIME_ANCIENNETE / 100",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "PRIME_TRANSPORT", Libelle = "Indemnité de transport", Type = "Gain", Section = "Prime",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.PRIME_TRANSPORT", FormuleMontant = "VAR.PRIME_TRANSPORT", FormuleGain = "VAR.PRIME_TRANSPORT",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "PRIME_RESPONSABILITE", Libelle = "Prime de responsabilité", Type = "Gain", Section = "Prime",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.PRIME_RESPONSABILITE", FormuleMontant = "VAR.PRIME_RESPONSABILITE", FormuleGain = "VAR.PRIME_RESPONSABILITE",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "PRIME_PRESENCE", Libelle = "Prime de présence", Type = "Gain", Section = "Prime",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.PRIME_PRESENCE", FormuleMontant = "VAR.PRIME_PRESENCE", FormuleGain = "VAR.PRIME_PRESENCE",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "PRIME_EXCEPTIONNELLE", Libelle = "Prime exceptionnelle", Type = "Gain", Section = "Prime",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.PRIME_EXCEPTIONNELLE", FormuleMontant = "VAR.PRIME_EXCEPTIONNELLE", FormuleGain = "VAR.PRIME_EXCEPTIONNELLE",
                CreatedBy = userId },

            // --- GAINS : Heures supplémentaires ---
            new Rubrique { CabinetId = cabinetId, Code = "HS_25", Libelle = "Heures supp. 25%", Type = "Gain", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.HS_25", FormuleTaux = "125", FormuleMontant = "VAR.HS_25 * (EMP.SALAIRE_BASE / 191) * 1.25", FormuleGain = "VAR.HS_25 * (EMP.SALAIRE_BASE / 191) * 1.25",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "HS_50", Libelle = "Heures supp. 50%", Type = "Gain", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.HS_50", FormuleTaux = "150", FormuleMontant = "VAR.HS_50 * (EMP.SALAIRE_BASE / 191) * 1.50", FormuleGain = "VAR.HS_50 * (EMP.SALAIRE_BASE / 191) * 1.50",
                CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "HS_100", Libelle = "Heures supp. 100%", Type = "Gain", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.HS_100", FormuleTaux = "200", FormuleMontant = "VAR.HS_100 * (EMP.SALAIRE_BASE / 191) * 2.00", FormuleGain = "VAR.HS_100 * (EMP.SALAIRE_BASE / 191) * 2.00",
                CreatedBy = userId },

            // --- GAINS : Avantages ---
            new Rubrique { CabinetId = cabinetId, Code = "AVANTAGE_NATURE", Libelle = "Avantages en nature", Type = "Gain", Section = "Avantage",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "EMP.AVANTAGES_NATURE", FormuleMontant = "EMP.AVANTAGES_NATURE", FormuleGain = "EMP.AVANTAGES_NATURE",
                CreatedBy = userId },

            // --- RETENUE : Absences ---
            new Rubrique { CabinetId = cabinetId, Code = "ABSENCE", Libelle = "Absences non rémunérées", Type = "Retenue", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.ABSENCE_JOURS", FormuleTaux = "EMP.SALAIRE_BASE / 26", FormuleMontant = "VAR.ABSENCE_JOURS * (EMP.SALAIRE_BASE / 26)", FormuleRetenue = "VAR.ABSENCE_JOURS * (EMP.SALAIRE_BASE / 26)",
                CreatedBy = userId },

            // --- CALCULE : Brut global ---
            new Rubrique { CabinetId = cabinetId, Code = "BRUT_GLOBAL", Libelle = "Salaire brut global", Type = "Calcule", Section = "Salaire",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleMontant = "SUM_GAINS() - SUM_RETENUES()", FormuleGain = "SUM_GAINS() - SUM_RETENUES()",
                CreatedBy = userId },

            // --- COTISATIONS SALARIALES ---
            new Rubrique { CabinetId = cabinetId, Code = "CNSS_SAL", Libelle = "CNSS salariale", Type = "Cotisation", Section = "Cotisation",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleBase = "MIN(BRUT_GLOBAL, CONST.CNSS_PLAFOND)", FormuleTaux = "CONST.CNSS_TAUX_SAL",
                FormuleMontant = "MIN(BRUT_GLOBAL, CONST.CNSS_PLAFOND) * CONST.CNSS_TAUX_SAL / 100",
                FormuleRetenue = "MIN(BRUT_GLOBAL, CONST.CNSS_PLAFOND) * CONST.CNSS_TAUX_SAL / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "AMO_SAL", Libelle = "AMO salariale", Type = "Cotisation", Section = "Cotisation",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleBase = "BRUT_GLOBAL", FormuleTaux = "CONST.AMO_TAUX_SAL",
                FormuleMontant = "BRUT_GLOBAL * CONST.AMO_TAUX_SAL / 100",
                FormuleRetenue = "BRUT_GLOBAL * CONST.AMO_TAUX_SAL / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "CIMR_SAL", Libelle = "CIMR salariale", Type = "Cotisation", Section = "Cotisation",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "BRUT_GLOBAL", FormuleTaux = "CONST.CIMR_TAUX_SAL",
                FormuleMontant = "BRUT_GLOBAL * CONST.CIMR_TAUX_SAL / 100",
                FormuleRetenue = "BRUT_GLOBAL * CONST.CIMR_TAUX_SAL / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            // --- CALCULE : Net imposable ---
            new Rubrique { CabinetId = cabinetId, Code = "NET_IMPOSABLE", Libelle = "Salaire net imposable", Type = "Calcule", Section = "Cotisation",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleMontant = "BRUT_GLOBAL - CNSS_SAL - AMO_SAL - CIMR_SAL",
                DependsDe = "BRUT_GLOBAL,CNSS_SAL,AMO_SAL,CIMR_SAL", CreatedBy = userId },

            // --- IMPOT ---
            new Rubrique { CabinetId = cabinetId, Code = "IR", Libelle = "IR (Impôt sur le revenu)", Type = "Impot", Section = "Impot",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleBase = "NET_IMPOSABLE", FormuleMontant = "IR_PROGRESSIF(NET_IMPOSABLE)",
                FormuleRetenue = "IR_PROGRESSIF(NET_IMPOSABLE)",
                DependsDe = "NET_IMPOSABLE", CreatedBy = userId },

            // --- RETENUE : Avances ---
            new Rubrique { CabinetId = cabinetId, Code = "AVANCE_SALAIRE", Libelle = "Avances sur salaire", Type = "Retenue", Section = "Net",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = true,
                FormuleBase = "VAR.AVANCE_SALAIRE", FormuleMontant = "VAR.AVANCE_SALAIRE", FormuleRetenue = "VAR.AVANCE_SALAIRE",
                CreatedBy = userId },

            // --- NET A PAYER ---
            new Rubrique { CabinetId = cabinetId, Code = "NET_A_PAYER", Libelle = "Net à payer", Type = "Calcule", Section = "Net",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = true,
                FormuleMontant = "BRUT_GLOBAL - SUM_RETENUES()",
                DependsDe = "BRUT_GLOBAL,CNSS_SAL,AMO_SAL,CIMR_SAL,IR,AVANCE_SALAIRE", CreatedBy = userId },

            // --- CHARGES PATRONALES ---
            new Rubrique { CabinetId = cabinetId, Code = "CNSS_PAT", Libelle = "CNSS patronale", Type = "Cotisation", Section = "Patronal",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = false,
                FormuleBase = "MIN(BRUT_GLOBAL, CONST.CNSS_PLAFOND)", FormuleTaux = "CONST.CNSS_TAUX_PAT",
                FormuleMontant = "MIN(BRUT_GLOBAL, CONST.CNSS_PLAFOND) * CONST.CNSS_TAUX_PAT / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "AMO_PAT", Libelle = "AMO patronale", Type = "Cotisation", Section = "Patronal",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = false,
                FormuleBase = "BRUT_GLOBAL", FormuleTaux = "CONST.AMO_TAUX_PAT",
                FormuleMontant = "BRUT_GLOBAL * CONST.AMO_TAUX_PAT / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "FP_PAT", Libelle = "Formation professionnelle", Type = "Cotisation", Section = "Patronal",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = true, Obligatoire = true, AfficherSurBulletin = false,
                FormuleBase = "BRUT_GLOBAL", FormuleTaux = "CONST.FP_TAUX",
                FormuleMontant = "BRUT_GLOBAL * CONST.FP_TAUX / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },

            new Rubrique { CabinetId = cabinetId, Code = "CIMR_PAT", Libelle = "CIMR patronale", Type = "Cotisation", Section = "Patronal",
                OrdreExecution = ++exec * 10, OrdreAffichage = ++aff * 10, Actif = true, Systeme = false, Obligatoire = false, AfficherSurBulletin = false,
                FormuleBase = "BRUT_GLOBAL", FormuleTaux = "CONST.CIMR_TAUX_PAT",
                FormuleMontant = "BRUT_GLOBAL * CONST.CIMR_TAUX_PAT / 100",
                DependsDe = "BRUT_GLOBAL", CreatedBy = userId },
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  DEFAULT SEED DATA — CONSTANTES MAROCAINES 2024
    // ═══════════════════════════════════════════════════════════

    private static List<ConstanteGlobale> GetDefaultConstantes(int cabinetId, string userId)
    {
        var dateEffet = new DateTime(2024, 1, 1);
        return new List<ConstanteGlobale>
        {
            // CNSS
            new ConstanteGlobale { CabinetId = cabinetId, Code = "CNSS_PLAFOND", Libelle = "Plafond CNSS mensuel", Categorie = "CNSS", Valeur = 6000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "CNSS_TAUX_SAL", Libelle = "Taux CNSS salarial (%)", Categorie = "CNSS", Valeur = 4.48m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "CNSS_TAUX_PAT", Libelle = "Taux CNSS patronal (%)", Categorie = "CNSS", Valeur = 8.98m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // AMO
            new ConstanteGlobale { CabinetId = cabinetId, Code = "AMO_TAUX_SAL", Libelle = "Taux AMO salarial (%)", Categorie = "AMO", Valeur = 2.26m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "AMO_TAUX_PAT", Libelle = "Taux AMO patronal (%)", Categorie = "AMO", Valeur = 4.11m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // CIMR
            new ConstanteGlobale { CabinetId = cabinetId, Code = "CIMR_TAUX_SAL", Libelle = "Taux CIMR salarial (%)", Categorie = "CIMR", Valeur = 3.00m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "CIMR_TAUX_PAT", Libelle = "Taux CIMR patronal (%)", Categorie = "CIMR", Valeur = 3.00m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // Formation professionnelle
            new ConstanteGlobale { CabinetId = cabinetId, Code = "FP_TAUX", Libelle = "Taux formation professionnelle (%)", Categorie = "FP", Valeur = 1.60m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // IR Tranches 2024
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_1_MIN", Libelle = "IR Tranche 1 Min", Categorie = "IR", Valeur = 0m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_1_MAX", Libelle = "IR Tranche 1 Max", Categorie = "IR", Valeur = 30000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_1_TAUX", Libelle = "IR Tranche 1 Taux (%)", Categorie = "IR", Valeur = 0m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_2_MIN", Libelle = "IR Tranche 2 Min", Categorie = "IR", Valeur = 30001m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_2_MAX", Libelle = "IR Tranche 2 Max", Categorie = "IR", Valeur = 50000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_2_TAUX", Libelle = "IR Tranche 2 Taux (%)", Categorie = "IR", Valeur = 10m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_3_MIN", Libelle = "IR Tranche 3 Min", Categorie = "IR", Valeur = 50001m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_3_MAX", Libelle = "IR Tranche 3 Max", Categorie = "IR", Valeur = 60000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_3_TAUX", Libelle = "IR Tranche 3 Taux (%)", Categorie = "IR", Valeur = 20m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_4_MIN", Libelle = "IR Tranche 4 Min", Categorie = "IR", Valeur = 60001m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_4_MAX", Libelle = "IR Tranche 4 Max", Categorie = "IR", Valeur = 80000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_4_TAUX", Libelle = "IR Tranche 4 Taux (%)", Categorie = "IR", Valeur = 30m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_5_MIN", Libelle = "IR Tranche 5 Min", Categorie = "IR", Valeur = 80001m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_5_MAX", Libelle = "IR Tranche 5 Max", Categorie = "IR", Valeur = 180000m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_5_TAUX", Libelle = "IR Tranche 5 Taux (%)", Categorie = "IR", Valeur = 34m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_6_MIN", Libelle = "IR Tranche 6 Min", Categorie = "IR", Valeur = 180001m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_6_MAX", Libelle = "IR Tranche 6 Max", Categorie = "IR", Valeur = 99999999m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_TRANCHE_6_TAUX", Libelle = "IR Tranche 6 Taux (%)", Categorie = "IR", Valeur = 38m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // Déductions IR
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_DEDUCTION_FAMILLE", Libelle = "Déduction par personne à charge (annuel)", Categorie = "IR", Valeur = 360m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "IR_PLAFOND_ENFANTS", Libelle = "Plafond personnes à charge", Categorie = "IR", Valeur = 6m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },

            // SMIG
            new ConstanteGlobale { CabinetId = cabinetId, Code = "SMIG_HORAIRE", Libelle = "SMIG horaire", Categorie = "GENERAL", Valeur = 15.55m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
            new ConstanteGlobale { CabinetId = cabinetId, Code = "SMIG_MENSUEL", Libelle = "SMIG mensuel (191h)", Categorie = "GENERAL", Valeur = 2970.05m, DateEffet = dateEffet, Actif = true, CreatedBy = userId },
        };
    }
}
