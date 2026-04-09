using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MetaCompta.Application.Services;

public class ClientService : IClientService
{
    private readonly MetaComptaDbContext _db;
    private readonly IMapper _mapper;

    public ClientService(MetaComptaDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<ClientDto>>> GetAllAsync(int cabinetId, int page = 1, int pageSize = 20, string? search = null, string? statut = null)
    {
        var query = _db.Clients
            .Where(c => c.CabinetId == cabinetId)
            .Include(c => c.ManagerPrincipal)
            .Include(c => c.Employees)
            .Include(c => c.Declarations)
            .Include(c => c.ServiceContrats)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.RaisonSociale.ToLower().Contains(s) ||
                (c.ICE != null && c.ICE.Contains(s)) ||
                (c.IF != null && c.IF.Contains(s)) ||
                (c.RC != null && c.RC.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(c => c.Statut == statut);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.RaisonSociale)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<ClientDto>>.Ok(new PagedResult<ClientDto>
        {
            Items = _mapper.Map<List<ClientDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<ClientDto>> GetByIdAsync(int id, int cabinetId)
    {
        var client = await _db.Clients
            .Include(c => c.ManagerPrincipal)
            .Include(c => c.Employees)
            .Include(c => c.Declarations)
            .Include(c => c.ServiceContrats)
            .FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);

        if (client == null)
            return ApiResponse<ClientDto>.Fail("Client non trouvé.");

        return ApiResponse<ClientDto>.Ok(_mapper.Map<ClientDto>(client));
    }

    public async Task<ApiResponse<ClientDto>> CreateAsync(ClientCreateDto dto, int cabinetId, string userId)
    {
        // Validate uniqueness
        if (!string.IsNullOrWhiteSpace(dto.ICE) && await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.ICE == dto.ICE))
            return ApiResponse<ClientDto>.Fail("Un client avec cet ICE existe déjà.");
        if (!string.IsNullOrWhiteSpace(dto.IF) && await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.IF == dto.IF))
            return ApiResponse<ClientDto>.Fail("Un client avec cet IF existe déjà.");

        var client = _mapper.Map<Client>(dto);
        client.CabinetId = cabinetId;
        client.CreatedBy = userId;
        client.Statut = "Active";

        // ⚠️ Services are NOT manually created here.
        // They are activated exclusively through the Contract lifecycle (ContratCommercial validation).
        // Any ServicesFiduciaires submitted in the DTO are IGNORED by design.

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        // Reload with navigations for correct DTO mapping
        var created = await _db.Clients
            .Include(c => c.ServiceContrats)
            .Include(c => c.Employees)
            .Include(c => c.Declarations)
            .FirstAsync(c => c.Id == client.Id);

        return ApiResponse<ClientDto>.Ok(_mapper.Map<ClientDto>(created), "Client créé avec succès.");
    }

    public async Task<ApiResponse<ClientDto>> UpdateAsync(int id, ClientCreateDto dto, int cabinetId, string userId)
    {
        var client = await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);
        if (client == null)
            return ApiResponse<ClientDto>.Fail("Client non trouvé.");

        _mapper.Map(dto, client);
        client.UpdatedBy = userId;
        client.UpdatedAt = DateTime.UtcNow;

        // ⚠️ Services are NOT manually updated here.
        // They are managed exclusively through the Contract lifecycle (ContratCommercial validation).
        // Any ServicesFiduciaires submitted in the DTO are IGNORED by design.

        await _db.SaveChangesAsync();

        // Audit log
        _db.AuditLogs.Add(new Domain.Common.AuditLog
        {
            UserId = userId,
            UserName = userId,
            Action = "Update",
            EntityType = "Client",
            EntityId = id.ToString(),
            CabinetId = cabinetId,
            NewValues = JsonSerializer.Serialize(dto)
        });
        await _db.SaveChangesAsync();

        return ApiResponse<ClientDto>.Ok(_mapper.Map<ClientDto>(client), "Client modifié avec succès.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId)
    {
        var client = await _db.Clients
            .Include(c => c.Employees)
            .Include(c => c.Declarations)
            .FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);

        if (client == null)
            return ApiResponse<bool>.Fail("Client non trouvé.");

        if (client.Employees.Any() || client.Declarations.Any())
            return ApiResponse<bool>.Fail("Impossible de supprimer un client avec des employés ou déclarations associés.");

        client.IsDeleted = true;
        client.DeletedAt = DateTime.UtcNow;
        client.DeletedBy = userId;
        client.Statut = "Dormante";

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Client supprimé avec succès.");
    }

    /// <summary>
    /// Returns the active service type codes for a client (temporal-aware).
    /// Only services active TODAY are returned for sidebar filtering.
    /// </summary>
    public async Task<List<string>> GetActiveServicesAsync(int clientId, int cabinetId)
    {
        var today = DateTime.UtcNow;
        return await _db.ServiceContrats
            .Where(s => s.ClientId == clientId && s.Client!.CabinetId == cabinetId
                && s.Actif && s.DateDebut <= today && (s.DateFin == null || s.DateFin >= today))
            .Select(s => s.TypeService)
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// Returns true if the client has at least one contract with status Valide or Actif.
    /// Used to gate declaration creation/editing.
    /// </summary>
    public async Task<bool> HasActiveContractAsync(int clientId, int cabinetId)
    {
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;
        return await _db.Set<ContratCommercial>()
            .AnyAsync(c => c.ClientId == clientId && c.CabinetId == cabinetId
                && activeStatuses.Contains(c.Statut) && !c.IsDeleted);
    }

    /// <summary>Returns all service contracts for a client, optionally filtered by exercise year</summary>
    public async Task<ApiResponse<List<ServiceContratDto>>> GetServiceContratsAsync(int clientId, int cabinetId, int? exerciceFiscal = null)
    {
        var query = _db.ServiceContrats
            .Where(s => s.ClientId == clientId && s.Client!.CabinetId == cabinetId);

        if (exerciceFiscal.HasValue)
            query = query.Where(s => s.ExerciceFiscal == exerciceFiscal.Value || s.ExerciceFiscal == null);

        var services = await query.OrderBy(s => s.Categorie).ThenBy(s => s.TypeService).ToListAsync();
        return ApiResponse<List<ServiceContratDto>>.Ok(_mapper.Map<List<ServiceContratDto>>(services));
    }

    /// <summary>Get a single service contract by ID</summary>
    public async Task<ApiResponse<ServiceContratDto>> GetServiceContratByIdAsync(int serviceContratId, int cabinetId)
    {
        var sc = await _db.ServiceContrats
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Id == serviceContratId && s.Client!.CabinetId == cabinetId);

        if (sc == null) return ApiResponse<ServiceContratDto>.Fail("Contrat de service non trouvé.");
        return ApiResponse<ServiceContratDto>.Ok(_mapper.Map<ServiceContratDto>(sc));
    }

    /// <summary>
    /// Initialize services for a new exercise year by cloning active service contracts.
    /// Respects the client's MoisClotureExercice for exercise date calculation.
    /// </summary>
    public async Task<ApiResponse<List<ServiceContratDto>>> InitializeServicesForExerciceAsync(int clientId, int exerciceFiscal, int cabinetId, string userId)
    {
        var client = await _db.Clients
            .Include(c => c.ServiceContrats)
            .FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<List<ServiceContratDto>>.Fail("Client non trouvé.");

        // Find active ongoing services (no exercise or previous exercise)
        var templateServices = client.ServiceContrats
            .Where(s => s.Actif && (s.ExerciceFiscal == null || s.ExerciceFiscal == exerciceFiscal - 1))
            .ToList();

        if (!templateServices.Any())
            return ApiResponse<List<ServiceContratDto>>.Fail("Aucun service actif trouvé pour servir de base.");

        // Check if this exercise already has services
        var existingForExercice = client.ServiceContrats.Where(s => s.ExerciceFiscal == exerciceFiscal).ToList();
        if (existingForExercice.Any())
            return ApiResponse<List<ServiceContratDto>>.Fail($"Des services existent déjà pour l'exercice {exerciceFiscal}.");

        var newServices = new List<ServiceContrat>();
        foreach (var template in templateServices)
        {
            var newSc = new ServiceContrat
            {
                ClientId = clientId,
                TypeService = template.TypeService,
                LibelleService = template.LibelleService,
                Categorie = template.Categorie,
                Frequence = template.Frequence,
                ExerciceFiscal = exerciceFiscal,
                JourLimiteDeclaration = template.JourLimiteDeclaration,
                DelaiDeclarationMois = template.DelaiDeclarationMois,
                ConfigurationJson = template.ConfigurationJson,
                HonorairesMensuels = template.HonorairesMensuels,
                HonorairesAnnuels = template.HonorairesAnnuels,
                Actif = true,
                AutoGenererDeclarations = template.AutoGenererDeclarations,
                ResponsableId = template.ResponsableId,
                Notes = template.Notes,
                CreatedBy = userId
            };
            ComputeExerciceDates(newSc, client.MoisClotureExercice);
            newSc.DateDebut = newSc.ExerciceDebut ?? new DateTime(exerciceFiscal, 1, 1);
            newSc.DateFin = newSc.ExerciceFin ?? new DateTime(exerciceFiscal, 12, 31);
            newServices.Add(newSc);
        }

        _db.ServiceContrats.AddRange(newServices);
        await _db.SaveChangesAsync();

        return ApiResponse<List<ServiceContratDto>>.Ok(
            _mapper.Map<List<ServiceContratDto>>(newServices),
            $"{newServices.Count} services initialisés pour l'exercice {exerciceFiscal}.");
    }

    /// <summary>
    /// Auto-generate declarations from active service contracts for a given exercise.
    /// Respects frequency (monthly, quarterly, annual), exercise dates, and deadlines.
    /// Only generates declarations that don't already exist.
    /// </summary>
    public async Task<ApiResponse<GenerateDeclarationsResultDto>> GenerateDeclarationsFromServicesAsync(
        GenerateDeclarationsRequestDto request, int cabinetId, string userId)
    {
        var client = await _db.Clients
            .Include(c => c.ServiceContrats)
            .Include(c => c.Declarations)
            .FirstOrDefaultAsync(c => c.Id == request.ClientId && c.CabinetId == cabinetId);

        if (client == null)
            return ApiResponse<GenerateDeclarationsResultDto>.Fail("Client non trouvé.");

        var result = new GenerateDeclarationsResultDto();
        var exercice = request.ExerciceFiscal;

        // Filter services eligible for declaration generation
        var declarationTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TVA", "IS", "IR", "CNSS" };
        var services = client.ServiceContrats
            .Where(s => s.Actif && declarationTypes.Contains(s.TypeService)
                && (s.ExerciceFiscal == null || s.ExerciceFiscal == exercice)
                && s.AutoGenererDeclarations)
            .ToList();

        if (request.TypeServices?.Any() == true)
            services = services.Where(s => request.TypeServices.Contains(s.TypeService, StringComparer.OrdinalIgnoreCase)).ToList();

        if (!services.Any())
        {
            result.Warnings.Add("Aucun service actif éligible pour la génération de déclarations.");
            return ApiResponse<GenerateDeclarationsResultDto>.Ok(result);
        }

        // Compute exercise boundaries
        var moisCloture = client.MoisClotureExercice;
        var (exDebut, exFin) = ComputeExerciceBounds(exercice, moisCloture);

        var existingDeclarations = client.Declarations
            .Where(d => d.DateDebut >= exDebut && d.DateFin <= exFin)
            .Select(d => $"{d.Type}|{d.Periode}")
            .ToHashSet();

        var newDeclarations = new List<Declaration>();

        foreach (var svc in services)
        {
            var periods = GeneratePeriodsForService(svc, exDebut, exFin);

            foreach (var period in periods)
            {
                var key = $"{svc.TypeService}|{period.Periode}";
                if (existingDeclarations.Contains(key))
                {
                    result.Warnings.Add($"{svc.TypeService} {period.Periode} existe déjà, ignorée.");
                    continue;
                }

                var declaration = new Declaration
                {
                    ClientId = request.ClientId,
                    CabinetId = cabinetId,
                    Type = svc.TypeService,
                    Periode = period.Periode,
                    DateDebut = period.DateDebut,
                    DateFin = period.DateFin,
                    DateLimite = period.DateLimite,
                    Statut = RefCodes.StatutDeclaration.Brouillon,
                    CreatedBy = userId,
                    Notes = $"Auto-générée depuis contrat de service #{svc.Id}"
                };
                newDeclarations.Add(declaration);
            }
        }

        if (newDeclarations.Any())
        {
            _db.Declarations.AddRange(newDeclarations);
            await _db.SaveChangesAsync();
        }

        result.TotalGenerated = newDeclarations.Count;
        result.Declarations = _mapper.Map<List<DeclarationDto>>(newDeclarations);

        return ApiResponse<GenerateDeclarationsResultDto>.Ok(result,
            $"{result.TotalGenerated} déclaration(s) générée(s) pour l'exercice {exercice}.");
    }

    // ════════════════════════════════════════════════════
    //  PRIVATE HELPERS — Smart Defaults & Period Logic
    // ════════════════════════════════════════════════════

    /// <summary>Build a ServiceContrat entity from DTO with smart defaults</summary>
    private ServiceContrat BuildServiceContrat(ServiceContratCreateDto svc, Client client)
    {
        var sc = _mapper.Map<ServiceContrat>(svc);
        sc.LibelleService = GetServiceLabel(svc.TypeService);
        sc.Categorie = GetServiceCategory(svc.TypeService);
        sc.DateDebut = svc.DateDebut ?? client.DateDebutContrat ?? DateTime.UtcNow;

        // Smart frequency defaults based on service type + client config
        if (string.IsNullOrWhiteSpace(svc.Frequence) || svc.Frequence == RefCodes.Frequence.Mensuelle)
        {
            sc.Frequence = GetSmartFrequency(svc.TypeService, client.TypeTVA);
        }

        // Smart deadline defaults (Moroccan fiscal calendar)
        ApplySmartDeadlines(sc);

        // Compute exercise dates if exercise year is set
        if (svc.ExerciceFiscal.HasValue)
        {
            sc.ExerciceFiscal = svc.ExerciceFiscal;
            ComputeExerciceDates(sc, client.MoisClotureExercice);
        }

        return sc;
    }

    /// <summary>Smart frequency based on service type and client TVA regime</summary>
    private static string GetSmartFrequency(string typeService, string typeTVA)
    {
        return typeService?.ToUpperInvariant() switch
        {
            RefCodes.TypeService.TVA => typeTVA == RefCodes.Frequence.Trimestrielle ? RefCodes.Frequence.Trimestrielle : RefCodes.Frequence.Mensuelle,
            RefCodes.TypeService.IS => RefCodes.Frequence.Annuelle,       // Annual with quarterly installments
            RefCodes.TypeService.IR => RefCodes.Frequence.Annuelle,       // Annual declaration
            RefCodes.TypeService.CNSS => RefCodes.Frequence.Mensuelle,    // Monthly since 2019 reform
            RefCodes.TypeService.Paie => RefCodes.Frequence.Mensuelle,    // Monthly payroll
            RefCodes.TypeService.Comptabilite => RefCodes.Frequence.Mensuelle,
            RefCodes.TypeService.Bilan => RefCodes.Frequence.Annuelle,
            RefCodes.TypeService.EtatsFiscaux => RefCodes.Frequence.Annuelle,
            RefCodes.TypeService.Juridique => RefCodes.Frequence.Ponctuelle,
            RefCodes.TypeService.AuditComptable => RefCodes.Frequence.Annuelle,
            RefCodes.TypeService.ConseilFiscal => RefCodes.Frequence.Ponctuelle,
            _ => RefCodes.Frequence.Mensuelle
        };
    }

    /// <summary>Apply Moroccan fiscal deadline defaults</summary>
    private static void ApplySmartDeadlines(ServiceContrat sc)
    {
        switch (sc.TypeService?.ToUpperInvariant())
        {
            case "TVA":
                sc.JourLimiteDeclaration = 20;  // 20th of M+1
                sc.DelaiDeclarationMois = 1;
                break;
            case "IS":
                sc.JourLimiteDeclaration = 31;  // End of M+3 after exercise close
                sc.DelaiDeclarationMois = 3;
                break;
            case "IR":
                sc.JourLimiteDeclaration = 28;  // Feb 28 for annual IR
                sc.DelaiDeclarationMois = 2;
                break;
            case "CNSS":
                sc.JourLimiteDeclaration = 10;  // 10th of M+1
                sc.DelaiDeclarationMois = 1;
                break;
        }
    }

    /// <summary>Compute exercise start/end dates from fiscal year and closing month</summary>
    private static void ComputeExerciceDates(ServiceContrat sc, int moisCloture)
    {
        if (!sc.ExerciceFiscal.HasValue) return;
        var (debut, fin) = ComputeExerciceBounds(sc.ExerciceFiscal.Value, moisCloture);
        sc.ExerciceDebut = debut;
        sc.ExerciceFin = fin;
    }

    /// <summary>Compute exercise boundaries: e.g., closing month=12 → Jan 1 to Dec 31; closing month=6 → Jul 1 to Jun 30</summary>
    private static (DateTime debut, DateTime fin) ComputeExerciceBounds(int exerciceYear, int moisCloture)
    {
        if (moisCloture == 12)
        {
            return (new DateTime(exerciceYear, 1, 1), new DateTime(exerciceYear, 12, 31));
        }
        else
        {
            // Broken exercise: e.g., June closing → July Y-1 to June Y
            var startYear = exerciceYear - 1;
            var startMonth = moisCloture + 1;
            return (
                new DateTime(startYear, startMonth, 1),
                new DateTime(exerciceYear, moisCloture, DateTime.DaysInMonth(exerciceYear, moisCloture))
            );
        }
    }

    /// <summary>Generate declaration periods for a service within exercise bounds</summary>
    private static List<(string Periode, DateTime DateDebut, DateTime DateFin, DateTime DateLimite)> GeneratePeriodsForService(
        ServiceContrat svc, DateTime exDebut, DateTime exFin)
    {
        var periods = new List<(string Periode, DateTime DateDebut, DateTime DateFin, DateTime DateLimite)>();
        var jourLimite = svc.JourLimiteDeclaration ?? 20;
        var delaiMois = svc.DelaiDeclarationMois ?? 1;

        switch (svc.Frequence?.ToUpperInvariant())
        {
            case "MENSUELLE":
                var current = exDebut;
                while (current <= exFin)
                {
                    var periodEnd = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
                    var limiteDate = periodEnd.AddMonths(delaiMois);
                    limiteDate = new DateTime(limiteDate.Year, limiteDate.Month, Math.Min(jourLimite, DateTime.DaysInMonth(limiteDate.Year, limiteDate.Month)));
                    periods.Add(($"{current:yyyy-MM}", current, periodEnd, limiteDate));
                    current = current.AddMonths(1);
                }
                break;

            case "TRIMESTRIELLE":
                var qStart = exDebut;
                int q = 1;
                while (qStart <= exFin)
                {
                    var qEnd = qStart.AddMonths(3).AddDays(-1);
                    if (qEnd > exFin) qEnd = exFin;
                    var limiteDate = qEnd.AddMonths(delaiMois);
                    limiteDate = new DateTime(limiteDate.Year, limiteDate.Month, Math.Min(jourLimite, DateTime.DaysInMonth(limiteDate.Year, limiteDate.Month)));
                    periods.Add(($"{qStart.Year}-Q{q}", qStart, qEnd, limiteDate));
                    qStart = qStart.AddMonths(3);
                    q++;
                }
                break;

            case "ANNUELLE":
                var annualLimite = exFin.AddMonths(delaiMois);
                annualLimite = new DateTime(annualLimite.Year, annualLimite.Month, Math.Min(jourLimite, DateTime.DaysInMonth(annualLimite.Year, annualLimite.Month)));
                periods.Add(($"{exDebut.Year}", exDebut, exFin, annualLimite));
                break;
        }

        return periods;
    }

    private static string GetServiceLabel(string typeService)
    {
        return typeService?.ToUpperInvariant() switch
        {
            RefCodes.TypeService.TVA => "Taxe sur la Valeur Ajoutée",
            RefCodes.TypeService.IS => "Impôt sur les Sociétés",
            RefCodes.TypeService.IR => "Impôt sur le Revenu",
            RefCodes.TypeService.CNSS => "Caisse Nationale de Sécurité Sociale",
            RefCodes.TypeService.Paie => "Gestion de la Paie",
            RefCodes.TypeService.Juridique => "Dossiers Juridiques",
            RefCodes.TypeService.Comptabilite => "Tenue Comptable",
            RefCodes.TypeService.Bilan => "Bilan & États de Synthèse",
            RefCodes.TypeService.EtatsFiscaux => "États Fiscaux",
            RefCodes.TypeService.AuditComptable => "Audit Comptable",
            RefCodes.TypeService.ConseilFiscal => "Conseil Fiscal",
            _ => typeService ?? ""
        };
    }

    private static string GetServiceCategory(string typeService)
    {
        return typeService?.ToUpperInvariant() switch
        {
            RefCodes.TypeService.TVA or RefCodes.TypeService.IS or RefCodes.TypeService.IR or RefCodes.TypeService.EtatsFiscaux => RefCodes.CategorieService.Fiscal,
            RefCodes.TypeService.CNSS or RefCodes.TypeService.Paie => RefCodes.CategorieService.Social,
            RefCodes.TypeService.Comptabilite or RefCodes.TypeService.Bilan => RefCodes.CategorieService.Comptable,
            RefCodes.TypeService.Juridique => RefCodes.CategorieService.Juridique,
            RefCodes.TypeService.AuditComptable or RefCodes.TypeService.ConseilFiscal => RefCodes.CategorieService.Conseil,
            _ => RefCodes.CategorieService.Autre
        };
    }
}
