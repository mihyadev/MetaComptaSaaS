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

    public PayrollService(MetaComptaDbContext db, IMapper mapper, IReferenceDataService refData)
    {
        _db = db;
        _mapper = mapper;
        _refData = refData;
    }

    public async Task<ApiResponse<List<PayrollPeriodDto>>> GetAllPeriodsAsync(int cabinetId, int? clientId = null)
    {
        var query = _db.PayrollPeriods
            .Include(p => p.Client)
            .Where(p => p.Client!.CabinetId == cabinetId);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

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

        var employees = await _db.Employees.Where(e => e.ClientId == clientId && e.Statut == RefCodes.StatutEmploye.Actif).ToListAsync();

        var period = new PayrollPeriod
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            Annee = annee,
            Mois = mois,
            Statut = RefCodes.StatutPaie.Brouillon,
            CreatedBy = userId
        };
        _db.PayrollPeriods.Add(period);
        await _db.SaveChangesAsync();

        foreach (var emp in employees)
        {
            var primesFixesTotales = (emp.PrimeAnciennete ?? 0) + (emp.PrimeResponsabilite ?? 0) + (emp.PrimePresence ?? 0) + (emp.IndemniteTransport ?? 0) + (emp.IndemniteRepresentation ?? 0);
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
        var slips = await _db.PaySlips.Include(s => s.Employee).Where(s => s.PayrollPeriodId == periodId && s.CabinetId == cabinetId).OrderBy(s => s.Employee!.Nom).ToListAsync();
        return ApiResponse<List<PaySlipDto>>.Ok(_mapper.Map<List<PaySlipDto>>(slips));
    }

    public async Task<ApiResponse<PaySlipDto>> UpdateVariablesAsync(int periodId, PaySlipVariablesDto dto, int cabinetId, string userId)
    {
        var slip = await _db.PaySlips.Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.PayrollPeriodId == periodId && s.EmployeeId == dto.EmployeeId && s.CabinetId == cabinetId);
        if (slip == null) return ApiResponse<PaySlipDto>.Fail("Fiche de paie non trouvée.");

        slip.HeuresSupp25 = dto.HeuresSupp25;
        slip.HeuresSupp50 = dto.HeuresSupp50;
        slip.HeuresSupp100 = dto.HeuresSupp100;
        slip.PrimesExceptionnelles = dto.PrimesExceptionnelles;
        slip.IndemnitesExceptionnelles = dto.IndemnitesExceptionnelles;
        slip.AvancesSalaire = dto.AvancesSalaire;
        slip.AbsencesNonRemunerees = dto.AbsencesNonRemunerees;
        slip.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        return ApiResponse<PaySlipDto>.Ok(_mapper.Map<PaySlipDto>(slip));
    }

    public async Task<ApiResponse<PayrollPeriodDto>> CalculateAllAsync(int periodId, int cabinetId, string userId)
    {
        var period = await _db.PayrollPeriods.Include(p => p.PaySlips).ThenInclude(s => s.Employee).FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<PayrollPeriodDto>.Fail("Période non trouvée.");

        // Get rates from reference data
        var plafondCNSS = await _refData.GetNumericValueAsync("CNSS", "PLAFOND");
        var tauxCNSSSalarial = await _refData.GetNumericValueAsync("CNSS", "TAUX_SALARIAL") / 100m;
        var tauxCNSSPatronal = await _refData.GetNumericValueAsync("CNSS", "TAUX_PATRONAL") / 100m;
        var tauxAMOSalarial = await _refData.GetNumericValueAsync("AMO", "TAUX_SALARIAL") / 100m;
        var tauxAMOPatronal = await _refData.GetNumericValueAsync("AMO", "TAUX_PATRONAL") / 100m;
        var tauxFP = await _refData.GetNumericValueAsync("FP", "TAUX") / 100m;
        var deductionParEnfant = await _refData.GetNumericValueAsync("IR_DEDUCTION", "CHARGE_FAMILLE");
        var plafondEnfants = (int)await _refData.GetNumericValueAsync("IR_DEDUCTION", "PLAFOND_ENFANTS");

        // Get IR brackets
        var irBrackets = await _db.ReferenceData
            .Where(r => r.Category == "IR_BRACKET" && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();

        foreach (var slip in period.PaySlips)
        {
            var emp = slip.Employee!;

            // Overtime calculation (based on daily rate)
            var tauxJournalier = slip.SalaireBase / 26m; // 26 working days
            var tauxHoraire = tauxJournalier / 8m;
            slip.MontantHeuresSupp = Math.Round(
                (slip.HeuresSupp25 * tauxHoraire * 1.25m) +
                (slip.HeuresSupp50 * tauxHoraire * 1.50m) +
                (slip.HeuresSupp100 * tauxHoraire * 2.00m), 2);

            // Absences deduction
            slip.MontantAbsences = Math.Round(slip.AbsencesNonRemunerees * tauxJournalier, 2);

            // Gross salary
            slip.SalaireBrutGlobal = slip.SalaireBase + slip.PrimesFixesTotales + slip.MontantHeuresSupp +
                slip.PrimesExceptionnelles + slip.IndemnitesExceptionnelles + (emp.AvantagesNature ?? 0) - slip.MontantAbsences;

            // CNSS (capped)
            var baseCNSS = Math.Min(slip.SalaireBrutGlobal, plafondCNSS);
            slip.CotisationCNSS = Math.Round(baseCNSS * tauxCNSSSalarial, 2);
            slip.CotisationAMO = Math.Round(baseCNSS * tauxAMOSalarial, 2);

            // Net imposable
            slip.SalaireNetImposable = slip.SalaireBrutGlobal - slip.CotisationCNSS - slip.CotisationAMO - slip.CotisationCIMR;

            // IR calculation (annual then monthly)
            var netImposableAnnuel = slip.SalaireNetImposable * 12m;
            var nbEnfants = Math.Min(emp.NombreEnfantsACharge, plafondEnfants);
            var deductionFamille = nbEnfants * deductionParEnfant;
            var baseIRAnnuel = netImposableAnnuel - deductionFamille;

            decimal irAnnuel = 0;
            foreach (var bracket in irBrackets)
            {
                if (bracket.Value != null)
                {
                    var json = System.Text.Json.JsonDocument.Parse(bracket.Value);
                    var min = json.RootElement.GetProperty("min").GetDecimal();
                    var max = json.RootElement.GetProperty("max").GetDecimal();
                    var rate = json.RootElement.GetProperty("rate").GetDecimal() / 100m;

                    if (baseIRAnnuel > min)
                    {
                        var taxable = Math.Min(baseIRAnnuel, max) - min;
                        if (min > 0) taxable = Math.Min(baseIRAnnuel - min, max - min);
                        irAnnuel += Math.Max(0, taxable) * rate;
                    }
                }
            }
            slip.IRRetenu = Math.Round(irAnnuel / 12m, 2);

            // Net to pay
            slip.SalaireNetAPayer = slip.SalaireBrutGlobal - slip.CotisationCNSS - slip.CotisationAMO -
                slip.CotisationCIMR - slip.IRRetenu - slip.AvancesSalaire - slip.AutresRetenues;

            // Employer charges
            slip.CotisationPatronaleCNSS = Math.Round(baseCNSS * tauxCNSSPatronal, 2);
            slip.CotisationPatronaleAMO = Math.Round(slip.SalaireBrutGlobal * tauxAMOPatronal, 2);
            slip.FormationProfessionnelle = Math.Round(slip.SalaireBrutGlobal * tauxFP, 2);
            slip.CoutTotalEmployeur = slip.SalaireBrutGlobal + slip.CotisationPatronaleCNSS + slip.CotisationPatronaleAMO + slip.FormationProfessionnelle + slip.AssurancesComplementaires;

            // Annual cumulative (simplified - sum from previous months)
            var previousSlips = await _db.PaySlips
                .Where(s => s.EmployeeId == slip.EmployeeId && s.PayrollPeriod!.Annee == period.Annee && s.PayrollPeriod.Mois < period.Mois && s.PayrollPeriod.Statut == RefCodes.StatutPaie.Validee)
                .ToListAsync();
            slip.CumulBrutAnnuel = previousSlips.Sum(s => s.SalaireBrutGlobal) + slip.SalaireBrutGlobal;
            slip.CumulNetImposableAnnuel = previousSlips.Sum(s => s.SalaireNetImposable) + slip.SalaireNetImposable;
            slip.CumulIRAnnuel = previousSlips.Sum(s => s.IRRetenu) + slip.IRRetenu;
            slip.CumulCNSSAnnuel = previousSlips.Sum(s => s.CotisationCNSS) + slip.CotisationCNSS;
        }

        // Update totals
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

    public async Task<ApiResponse<bool>> ValidateAsync(int periodId, int cabinetId, string userId)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == periodId && p.CabinetId == cabinetId);
        if (period == null) return ApiResponse<bool>.Fail("Période non trouvée.");
        if (period.Statut == RefCodes.StatutPaie.Validee) return ApiResponse<bool>.Fail("Paie déjà validée.");

        period.Statut = RefCodes.StatutPaie.Validee;
        period.ValidateurId = userId;
        period.DateValidation = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Paie validée avec succès.");
    }
}
