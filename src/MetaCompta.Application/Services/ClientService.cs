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
    private readonly IContractAccessService _contractAccess;

    public ClientService(MetaComptaDbContext db, IMapper mapper, IContractAccessService contractAccess)
    {
        _db = db;
        _mapper = mapper;
        _contractAccess = contractAccess;
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
        NormalizeClientDto(dto);
        var validation = await ValidateClientDtoAsync(dto, cabinetId, excludeClientId: null);
        if (validation != null) return validation;

        try
        {
            var client = _mapper.Map<Client>(dto);
            client.CabinetId = cabinetId;
            client.CreatedBy = userId;
            client.Statut = "Active";
            client.ManagerPrincipalId = dto.ManagerPrincipalId;

            // Services are activated exclusively through ContratCommercial validation.
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            var created = await _db.Clients
                .Include(c => c.ServiceContrats)
                .Include(c => c.Employees)
                .Include(c => c.Declarations)
                .FirstAsync(c => c.Id == client.Id);

            return ApiResponse<ClientDto>.Ok(_mapper.Map<ClientDto>(created), "Client créé avec succès.");
        }
        catch (DbUpdateException ex)
        {
            return ApiResponse<ClientDto>.Fail(MapDbUpdateError(ex));
        }
        catch (Exception)
        {
            return ApiResponse<ClientDto>.Fail("Impossible de créer le client. Vérifiez les informations saisies (ICE, IF, manager) puis réessayez.");
        }
    }

    public async Task<ApiResponse<ClientDto>> UpdateAsync(int id, ClientCreateDto dto, int cabinetId, string userId)
    {
        var client = await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);
        if (client == null)
            return ApiResponse<ClientDto>.Fail("Client non trouvé.");

        NormalizeClientDto(dto);
        var validation = await ValidateClientDtoAsync(dto, cabinetId, excludeClientId: id);
        if (validation != null) return validation;

        try
        {
            // Preserve manager if the form omitted it (edit tabs may not bind the field)
            var previousManager = client.ManagerPrincipalId;
            _mapper.Map(dto, client);
            if (string.IsNullOrWhiteSpace(dto.ManagerPrincipalId))
                client.ManagerPrincipalId = previousManager;
            else
                client.ManagerPrincipalId = dto.ManagerPrincipalId;

            client.UpdatedBy = userId;
            client.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

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
        catch (DbUpdateException ex)
        {
            return ApiResponse<ClientDto>.Fail(MapDbUpdateError(ex));
        }
        catch (Exception)
        {
            return ApiResponse<ClientDto>.Fail("Impossible de modifier le client. Vérifiez les informations saisies (ICE, IF, manager) puis réessayez.");
        }
    }

    private static void NormalizeClientDto(ClientCreateDto dto)
    {
        static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        dto.RaisonSociale = (dto.RaisonSociale ?? string.Empty).Trim();
        dto.ICE = Clean(dto.ICE);
        dto.IF = Clean(dto.IF);
        dto.RC = Clean(dto.RC);
        dto.NumeroCNSS = Clean(dto.NumeroCNSS);
        dto.NumeroPatente = Clean(dto.NumeroPatente);
        dto.CodeActivite = Clean(dto.CodeActivite);
        dto.FormeJuridique = Clean(dto.FormeJuridique);
        dto.SecteurActivite = Clean(dto.SecteurActivite);
        dto.AdresseSiegeSocial = Clean(dto.AdresseSiegeSocial);
        dto.VilleSiegeSocial = Clean(dto.VilleSiegeSocial);
        dto.TelephonePrincipal = Clean(dto.TelephonePrincipal);
        dto.EmailPrincipal = Clean(dto.EmailPrincipal);
        dto.ContactNom = Clean(dto.ContactNom);
        dto.ContactFonction = Clean(dto.ContactFonction);
        dto.ContactTelephone = Clean(dto.ContactTelephone);
        dto.ContactEmail = Clean(dto.ContactEmail);
        dto.ManagerPrincipalId = Clean(dto.ManagerPrincipalId);
        dto.TypeTVA = string.IsNullOrWhiteSpace(dto.TypeTVA) ? "Mensuelle" : dto.TypeTVA.Trim();
        dto.NotesInternes = Clean(dto.NotesInternes);
        if (dto.MoisClotureExercice < 1 || dto.MoisClotureExercice > 12)
            dto.MoisClotureExercice = 12;
    }

    private async Task<ApiResponse<ClientDto>?> ValidateClientDtoAsync(ClientCreateDto dto, int cabinetId, int? excludeClientId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.RaisonSociale))
            errors.Add("La raison sociale est obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.ICE))
            errors.Add("L'ICE est obligatoire.");
        else if (dto.ICE.Length != 15 || !dto.ICE.All(char.IsDigit))
            errors.Add("L'ICE doit contenir exactement 15 chiffres.");
        if (string.IsNullOrWhiteSpace(dto.IF))
            errors.Add("L'identifiant fiscal (IF) est obligatoire.");
        if (dto.MoisClotureExercice < 1 || dto.MoisClotureExercice > 12)
            errors.Add("Le mois de clôture doit être entre 1 et 12.");
        if (!string.IsNullOrWhiteSpace(dto.EmailPrincipal) && !dto.EmailPrincipal.Contains('@'))
            errors.Add("L'email principal n'est pas valide.");
        if (!string.IsNullOrWhiteSpace(dto.ContactEmail) && !dto.ContactEmail.Contains('@'))
            errors.Add("L'email du contact n'est pas valide.");

        if (errors.Count > 0)
            return ApiResponse<ClientDto>.Fail(errors[0], errors);

        if (!string.IsNullOrWhiteSpace(dto.ICE) &&
            await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.ICE == dto.ICE && (!excludeClientId.HasValue || c.Id != excludeClientId.Value)))
            return ApiResponse<ClientDto>.Fail("Un client avec cet ICE existe déjà dans votre cabinet.");

        if (!string.IsNullOrWhiteSpace(dto.IF) &&
            await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.IF == dto.IF && (!excludeClientId.HasValue || c.Id != excludeClientId.Value)))
            return ApiResponse<ClientDto>.Fail("Un client avec cet IF existe déjà dans votre cabinet.");

        if (!string.IsNullOrWhiteSpace(dto.RC) &&
            await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.RC == dto.RC && (!excludeClientId.HasValue || c.Id != excludeClientId.Value)))
            return ApiResponse<ClientDto>.Fail("Un client avec ce RC existe déjà dans votre cabinet.");

        if (!string.IsNullOrWhiteSpace(dto.NumeroCNSS) &&
            await _db.Clients.AnyAsync(c => c.CabinetId == cabinetId && c.NumeroCNSS == dto.NumeroCNSS && (!excludeClientId.HasValue || c.Id != excludeClientId.Value)))
            return ApiResponse<ClientDto>.Fail("Un client avec ce N° CNSS existe déjà dans votre cabinet.");

        if (!string.IsNullOrWhiteSpace(dto.ManagerPrincipalId))
        {
            var managerExists = await _db.Users.AnyAsync(u =>
                u.Id == dto.ManagerPrincipalId && u.CabinetId == cabinetId && u.IsActive);
            if (!managerExists)
                return ApiResponse<ClientDto>.Fail("Le manager principal est introuvable. Laissez le champ vide ou choisissez un utilisateur valide du cabinet.");
        }

        return null;
    }

    private static string MapDbUpdateError(DbUpdateException ex)
    {
        var raw = (ex.InnerException?.Message ?? ex.Message) ?? string.Empty;
        var lower = raw.ToLowerInvariant();
        if (lower.Contains("managerprincipal") || lower.Contains("fk_clients_aspnetusers"))
            return "Le manager principal est invalide. Laissez le champ vide ou sélectionnez un utilisateur existant.";
        if (lower.Contains("ice") && (lower.Contains("unique") || lower.Contains("duplicate") || lower.Contains("ix_")))
            return "Un client avec cet ICE existe déjà dans votre cabinet.";
        if ((lower.Contains("\"if\"") || lower.Contains("[if]") || lower.Contains("_if")) && (lower.Contains("unique") || lower.Contains("duplicate") || lower.Contains("ix_")))
            return "Un client avec cet IF existe déjà dans votre cabinet.";
        if (lower.Contains("numero_cnss") || lower.Contains("numerocnss"))
            return "Un client avec ce N° CNSS existe déjà dans votre cabinet.";
        if (lower.Contains("rc") && (lower.Contains("unique") || lower.Contains("duplicate") || lower.Contains("ix_")))
            return "Un client avec ce RC existe déjà dans votre cabinet.";
        if (lower.Contains("unique") || lower.Contains("duplicate") || lower.Contains("2627") || lower.Contains("2601"))
            return "Une donnée unique est déjà utilisée (ICE, IF, RC ou CNSS). Vérifiez les identifiants saisis.";
        return "Impossible d'enregistrer le client. Vérifiez les informations saisies et réessayez.";
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
        var ctx = await _contractAccess.GetContractAccessContextAsync(clientId, cabinetId);
        return ctx.ActiveServices ?? new List<string>();
    }

    /// <summary>
    /// Returns true if the client has Manage-mode access (Valide/Actif + date window).
    /// </summary>
    public Task<bool> HasActiveContractAsync(int clientId, int cabinetId)
        => _contractAccess.HasManageAccessAsync(clientId, cabinetId);

    /// <inheritdoc />
    public async Task<ApiResponse<ClientContratPickerResultDto>> GetForContratPickerAsync(int cabinetId)
    {
        var today = ContractAccessService.BusinessToday();
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;

        var clients = await _db.Clients.AsNoTracking()
            .Where(c => c.CabinetId == cabinetId)
            .Select(c => new
            {
                c.Id,
                c.RaisonSociale,
                c.ICE,
                c.IF,
                c.RC,
                c.NumeroCNSS,
                c.VilleSiegeSocial
            })
            .OrderBy(c => c.RaisonSociale)
            .ToListAsync();

        var activeByClient = await _db.ContratsCommerciaux.AsNoTracking()
            .Where(c => c.CabinetId == cabinetId
                && !c.IsDeleted
                && activeStatuses.Contains(c.Statut)
                && (c.DateDebutContrat == null || c.DateDebutContrat <= today)
                && (c.DateFinContrat == null || c.DateFinContrat >= today))
            .Select(c => new { c.ClientId, c.Reference, c.Statut, c.DateDebutContrat })
            .ToListAsync();

        var activeLookup = activeByClient
            .GroupBy(c => c.ClientId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.DateDebutContrat).First());

        var sans = new List<ClientContratPickerItemDto>();
        var avec = new List<ClientContratPickerItemDto>();

        foreach (var c in clients)
        {
            var item = new ClientContratPickerItemDto
            {
                Id = c.Id,
                RaisonSociale = c.RaisonSociale,
                ICE = c.ICE,
                IF = c.IF,
                RC = c.RC,
                NumeroCNSS = c.NumeroCNSS,
                VilleSiegeSocial = c.VilleSiegeSocial
            };

            if (activeLookup.TryGetValue(c.Id, out var act))
            {
                item.HasActiveContract = true;
                item.ActiveContractReference = act.Reference;
                item.ActiveContractStatut = act.Statut;
                avec.Add(item);
            }
            else
            {
                item.HasActiveContract = false;
                sans.Add(item);
            }
        }

        return ApiResponse<ClientContratPickerResultDto>.Ok(new ClientContratPickerResultDto
        {
            SansContratActif = sans,
            AvecContratActif = avec
        });
    }

    public Task<ContractAccessContextDto> GetContractAccessContextAsync(int clientId, int cabinetId, int? exerciceComptableId = null)
        => _contractAccess.GetContractAccessContextAsync(clientId, cabinetId, exerciceComptableId);

    /// <summary>Returns all service contracts for a client, optionally filtered by exercise year / id</summary>
    public async Task<ApiResponse<List<ServiceContratDto>>> GetServiceContratsAsync(
        int clientId, int cabinetId, int? exerciceFiscal = null, int? exerciceComptableId = null)
    {
        var query = _db.ServiceContrats
            .Where(s => s.ClientId == clientId && s.Client!.CabinetId == cabinetId);

        if (exerciceComptableId is > 0)
        {
            var exoId = exerciceComptableId.Value;
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exoId && e.ClientId == clientId && e.CabinetId == cabinetId);
            var annee = exo?.Annee;
            query = query.Where(s =>
                s.ExerciceComptableId == exoId
                || (s.ExerciceComptableId == null && (annee == null || s.ExerciceFiscal == null || s.ExerciceFiscal == annee)));
        }
        else if (exerciceFiscal.HasValue)
        {
            query = query.Where(s => s.ExerciceFiscal == exerciceFiscal.Value || s.ExerciceFiscal == null);
        }

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

        var openExo = await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == request.ClientId && e.CabinetId == cabinetId && e.Statut == "Ouvert")
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();
        if (request.ExerciceComptableId is > 0)
            openExo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.ExerciceComptableId && e.ClientId == request.ClientId && e.CabinetId == cabinetId && e.Statut == "Ouvert");
        if (openExo == null)
            return ApiResponse<GenerateDeclarationsResultDto>.Fail(ExerciceContextService.NoOpenExerciceMessage);
        if (exercice <= 0) exercice = openExo.Annee;

        // Filter services eligible for declaration generation
        var declarationTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TVA", "IS", "IR", "CNSS", "BILAN", "LIASSEFISCALE", "ETATSSYNTHESE", "CPC", "AMORTISSEMENTS" };
        var services = client.ServiceContrats
            .Where(s => s.Actif && declarationTypes.Contains(s.TypeService)
                && (s.ExerciceComptableId == openExo.Id || s.ExerciceFiscal == null || s.ExerciceFiscal == exercice)
                && s.AutoGenererDeclarations)
            .ToList();

        if (request.TypeServices?.Any() == true)
            services = services.Where(s => request.TypeServices.Contains(s.TypeService, StringComparer.OrdinalIgnoreCase)).ToList();

        if (!services.Any())
        {
            result.Warnings.Add("Aucun service actif éligible pour la génération de déclarations.");
            return ApiResponse<GenerateDeclarationsResultDto>.Ok(result);
        }

        // Periods must fall inside the open exercice window
        var exDebut = openExo.DateDebut.Date;
        var exFin = openExo.DateFin.Date;

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
                    ExerciceComptableId = openExo.Id,
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
                sc.JourLimiteDeclaration = 26;  // working deadline: 26th of M+1
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

    /// <summary>Generate declaration periods for a service within exercise bounds (shared template).</summary>
    private static List<(string Periode, DateTime DateDebut, DateTime DateFin, DateTime DateLimite)> GeneratePeriodsForService(
        ServiceContrat svc, DateTime exDebut, DateTime exFin)
    {
        var periods = new List<(string Periode, DateTime DateDebut, DateTime DateFin, DateTime DateLimite)>();
        var jourLimite = svc.JourLimiteDeclaration
            ?? (string.Equals(svc.TypeService, "TVA", StringComparison.OrdinalIgnoreCase) ? 26 : 20);
        var delaiMois = svc.DelaiDeclarationMois ?? 1;
        var freq = DeclarationPeriodTemplate.NormalizeFrequency(svc.Frequence);

        var isTva = string.Equals(svc.TypeService, "TVA", StringComparison.OrdinalIgnoreCase);

        // Walk each calendar year covered by the exercise
        for (var year = exDebut.Year; year <= exFin.Year; year++)
        {
            if (isTva && (freq == "Mensuelle" || freq == "Trimestrielle"))
            {
                var morocco = freq == "Trimestrielle"
                    ? MoroccoTvaCalendar.GenerateQuarterlyDeclarationYear(year)
                    : MoroccoTvaCalendar.GenerateMonthlyDeclarationYear(year);
                foreach (var row in morocco.Where(r => !r.DisplayOnly))
                {
                    if (row.ActivityEnd < exDebut || row.ActivityStart > exFin) continue;
                    var start = row.ActivityStart < exDebut ? exDebut : row.ActivityStart;
                    var end = row.ActivityEnd > exFin ? exFin : row.ActivityEnd;
                    periods.Add((
                        $"{row.ActivityStart:yyyy-MM}",
                        start,
                        end,
                        row.Deadline));
                }
                continue;
            }

            foreach (var slice in DeclarationPeriodTemplate.GenerateForYear(year, freq))
            {
                if (slice.End < exDebut || slice.Start > exFin) continue;
                var start = slice.Start < exDebut ? exDebut : slice.Start;
                var end = slice.End > exFin ? exFin : slice.End;
                var limiteDate = end.AddMonths(delaiMois);
                limiteDate = new DateTime(
                    limiteDate.Year,
                    limiteDate.Month,
                    Math.Min(jourLimite, DateTime.DaysInMonth(limiteDate.Year, limiteDate.Month)));
                periods.Add((slice.Code, start, end, limiteDate));
            }
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
