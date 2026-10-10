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
    private readonly IContractAccessService _contractAccess;
    private readonly IExerciceContextService _exerciceContext;

    public DeclarationService(
        MetaComptaDbContext db,
        IMapper mapper,
        IReferenceDataService refData,
        INotificationService notifications,
        IContractAccessService contractAccess,
        IExerciceContextService exerciceContext)
    {
        _db = db;
        _mapper = mapper;
        _refData = refData;
        _notifications = notifications;
        _contractAccess = contractAccess;
        _exerciceContext = exerciceContext;
    }

    private async Task<ApiResponse<ExerciceComptableDto>> RequireExerciceAsync(int clientId, int cabinetId, int? preferredId = null)
        => await _exerciceContext.RequireOpenExerciceAsync(clientId, cabinetId, preferredId);

    private async Task<string?> EnsureDeclarationExerciceOpenAsync(Declaration declaration, int cabinetId)
    {
        if (declaration.ExerciceComptableId is not int exoId) return null;
        var exo = await _db.ExercicesComptables.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == exoId && e.CabinetId == cabinetId);
        if (exo != null && !string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            return ExerciceContextService.ClosedExerciceMessage;
        return null;
    }

    // ─── WORKFLOW STEP OWNERSHIP LOCK ──────────────────────────────
    /// <summary>
    /// When an Active workflow has a current employee step, only the step holder
    /// (or Manager/Gérant) may edit declaration data or change status.
    /// Returns an error message, or null if edit is allowed.
    /// </summary>
    private async Task<string?> EnsureStepHolderCanEditAsync(int declarationId, int cabinetId, string userId)
    {
        var activeWf = await _db.WorkflowInstances
            .Include(w => w.StepInstances)
            .FirstOrDefaultAsync(w =>
                w.DeclarationId == declarationId
                && w.CabinetId == cabinetId
                && w.Status == WorkflowInstanceStatus.Active
                && !w.IsDeleted);
        if (activeWf == null) return null;

        var currentStep = activeWf.StepInstances?
            .FirstOrDefault(s => s.StepOrder == activeWf.CurrentStepOrder && !s.IsDeleted);
        if (currentStep == null) return null;

        var template = currentStep.WorkflowStepTemplateId > 0
            ? await _db.WorkflowStepTemplates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == currentStep.WorkflowStepTemplateId)
            : null;

        if (template?.AssignmentType == AssignmentType.AutoSystem)
            return null;
        if (template?.IsClientStep == true || template?.AssignmentType == AssignmentType.Client)
            return null;

        if (await IsManagerOrGerantAsync(userId, cabinetId))
            return null;

        // Exclusive soft lock: prefer open UnitTake on current workflow step, else AssignedToUserId.
        var openTake = await _db.UnitTakes.AsNoTracking()
            .Include(t => t.TakenByUser)
            .Include(t => t.UnitType)
            .Where(t => t.CabinetId == cabinetId
                        && t.UnitEntityId == currentStep.Id
                        && t.ClosedAt == null
                        && t.Status == UnitTakeStatuses.Open
                        && !t.IsDeleted
                        && t.UnitType != null
                        && t.UnitType.Code == UnitTypeCodes.WorkflowStep)
            .OrderByDescending(t => t.TakenAt)
            .FirstOrDefaultAsync();

        var holderId = openTake?.TakenByUserId ?? currentStep.AssignedToUserId;

        if (string.IsNullOrEmpty(holderId))
        {
            return "Cette étape n'est pas encore prise. Utilisez « Prendre / Ouvrir » avant de modifier la déclaration.";
        }

        if (string.Equals(holderId, userId, StringComparison.OrdinalIgnoreCase))
            return null;

        {
            var holder = openTake?.TakenByUser != null
                ? $"{openTake.TakenByUser.FirstName} {openTake.TakenByUser.LastName}".Trim()
                : await _db.Users.AsNoTracking()
                    .Where(u => u.Id == holderId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefaultAsync();
            var who = string.IsNullOrWhiteSpace(holder) ? "un autre collaborateur" : holder.Trim();
            return $"Cette déclaration est verrouillée par {who} (titulaire de l'étape en cours). Seul ce collaborateur (ou un Manager/Gérant) peut la modifier.";
        }
    }

    private async Task<bool> IsManagerOrGerantAsync(string userId, int cabinetId)
    {
        return await _db.Users
            .Where(u => u.Id == userId && u.CabinetId == cabinetId && u.IsActive)
            .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => ur)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .AnyAsync(name => name == "Manager" || name == "Gerant" || name == "Admin" || name == "SuperAdmin");
    }

    // ─── CONTRACT GUARD ────────────────────────────────────────────
    /// <summary>
    /// Manage-mode gate: Valide/Actif + date window (shared with ClientService).
    /// </summary>
    private Task<bool> HasActiveContractAsync(int clientId, int cabinetId)
        => _contractAccess.HasManageAccessAsync(clientId, cabinetId);

    // ─── LIST / GET ────────────────────────────────────────────────
    public async Task<ApiResponse<PagedResult<DeclarationDto>>> GetAllAsync(
        int cabinetId, int? clientId = null, string? type = null, string? statut = null,
        int page = 1, int pageSize = 20, int? exerciceComptableId = null)
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

        if (exerciceComptableId is > 0)
        {
            var exoId = exerciceComptableId.Value;
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exoId && e.CabinetId == cabinetId
                    && (!clientId.HasValue || e.ClientId == clientId.Value));
            if (exo != null)
            {
                var annee = exo.Annee;
                var debut = exo.DateDebut.Date;
                var fin = exo.DateFin.Date;
                query = query.Where(d =>
                    d.ExerciceComptableId == exoId
                    || (d.ExerciceComptableId == null
                        && d.DateDebut >= debut && d.DateFin <= fin)
                    || (d.ExerciceComptableId == null
                        && d.Periode != null
                        && (d.Periode == annee.ToString()
                            || d.Periode.StartsWith(annee.ToString() + "-")
                            || d.Periode.StartsWith(annee.ToString() + "Q")
                            || d.Periode.StartsWith(annee.ToString() + "-Q"))));
            }
            else
            {
                query = query.Where(d => d.ExerciceComptableId == exoId);
            }
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.DateLimite)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<DeclarationDto>>(items);
        await EnrichDeclarationsWithWorkflowAsync(dtos, cabinetId);

        return ApiResponse<PagedResult<DeclarationDto>>.Ok(new PagedResult<DeclarationDto>
        {
            Items = dtos,
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
        var dto = _mapper.Map<DeclarationDto>(dec);
        await EnrichDeclarationsWithWorkflowAsync(new List<DeclarationDto> { dto }, cabinetId);
        return ApiResponse<DeclarationDto>.Ok(dto);
    }

    private async Task EnrichDeclarationsWithWorkflowAsync(List<DeclarationDto> dtos, int cabinetId)
    {
        if (dtos.Count == 0) return;
        var ids = dtos.Select(d => d.Id).ToList();
        var instances = await _db.WorkflowInstances
            .AsNoTracking()
            .Include(w => w.StepInstances)
            .Where(w => w.CabinetId == cabinetId
                        && w.DeclarationId != null
                        && ids.Contains(w.DeclarationId.Value)
                        && !w.IsDeleted
                        && (w.Status == WorkflowInstanceStatus.Active || w.Status == WorkflowInstanceStatus.Paused || w.Status == WorkflowInstanceStatus.Completed))
            .OrderByDescending(w => w.StartedAt)
            .ToListAsync();

        foreach (var dto in dtos)
        {
            var wi = instances.FirstOrDefault(w => w.DeclarationId == dto.Id
                                                   && w.Status is WorkflowInstanceStatus.Active or WorkflowInstanceStatus.Paused)
                     ?? instances.FirstOrDefault(w => w.DeclarationId == dto.Id);
            if (wi == null) continue;
            dto.WorkflowInstanceId = wi.Id;
            dto.WorkflowStatus = wi.Status.ToString();
            dto.TotalSteps = wi.StepInstances?.Count ?? 0;
            dto.CompletedSteps = wi.StepInstances?.Count(s =>
                s.Status == WorkflowStepStatus.Completed || s.Status == WorkflowStepStatus.Skipped) ?? 0;
            dto.CurrentStepName = wi.StepInstances?
                .FirstOrDefault(s => s.StepOrder == wi.CurrentStepOrder)?.StepName;
        }
    }

    // ─── GENERIC CREATE ────────────────────────────────────────────
    public async Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
    {
        if (!await HasActiveContractAsync(dto.ClientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration : le client n'a aucun contrat actif ou validé.");

        var exoGate = await RequireExerciceAsync(dto.ClientId, cabinetId, dto.ExerciceComptableId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);
        dto.ExerciceComptableId = exoGate.Data.Id;

        var type = DeclarationServiceCatalog.NormalizeType(dto.Type);
        dto.Type = type;

        return type switch
        {
            "TVA" => await CreateTVAInternalAsync(dto.ClientId, cabinetId, userId, dto.ExerciceComptableId, dto),
            "IS" => await CreateISInternalAsync(dto.ClientId, dto.AnneeFiscale ?? exoGate.Data.Annee, cabinetId, userId, dto.ExerciceComptableId, dto),
            "IR" => await CreateIRInternalAsync(dto.ClientId, dto.AnneeFiscale ?? exoGate.Data.Annee, cabinetId, userId, dto.ExerciceComptableId, dto),
            "CNSS" => await CreateCNSSInternalAsync(dto.ClientId, cabinetId, userId, dto.ExerciceComptableId, dto),
            _ when DeclarationServiceCatalog.IsSupported(type) => await CreateShellAsync(dto, cabinetId, userId),
            _ => ApiResponse<DeclarationDto>.Fail($"Type de déclaration '{dto.Type}' non supporté.")
        };
    }

    /// <summary>
    /// DateLimite: per-row due (DueMonth/DueDay or DateLimite) → legacy window* → calendar.
    /// PeriodEnd is the taxable slice end — never the filing deadline.
    /// </summary>
    private static DateTime ResolveInitDateLimite(
        CreateDeclarationDto? periodCfg,
        DateTime periodEnd,
        string? frequency,
        string type,
        DateTime calendarFilingDeadline)
    {
        if (periodCfg == null) return calendarFilingDeadline;

        if (periodCfg.DateLimite is DateTime explicitDue)
            return explicitDue.Date;

        if (periodCfg.DueMonth is >= 1 and <= 12 && periodCfg.DueDay is >= 1 and <= 31)
            return DeclarationPeriodTemplate.MaterializeDue(periodCfg.DueMonth.Value, periodCfg.DueDay.Value, periodEnd);

        var hasWindow = periodCfg.WindowEndDay is >= 1 and <= 31
            || periodCfg.DeadlineMonth is >= 1 and <= 12
            || periodCfg.WindowMonthOffset.HasValue
            || periodCfg.DeadlineYearOffset.HasValue;

        if (hasWindow)
        {
            var day = periodCfg.WindowEndDay
                ?? MoroccoDeclarationCalendar.GetDefaultFilingWindow(type).EndDay;
            return MoroccoDeclarationCalendar.ApplyInitDeadline(
                periodCfg.Frequency ?? frequency,
                periodEnd,
                day,
                periodCfg.WindowMonthOffset,
                periodCfg.DeadlineMonth,
                periodCfg.DeadlineYearOffset,
                type);
        }

        // TVA default without Init due: end of next month
        if (string.Equals(type, "TVA", StringComparison.OrdinalIgnoreCase))
            return DeclarationPeriodTemplate.EndOfMonthAfter(periodEnd);

        return calendarFilingDeadline;
    }

    /// <summary>Init period template: periods[] = taxable slice + per-row due; window* = legacy fallback.</summary>
    private static (DateTime Start, DateTime End, DateTime Limite, string Periode) ApplyPeriodTemplate(
        CreateDeclarationDto? dto, MoroccoDeclarationCalendar.Window window, string type)
    {
        if (dto?.PeriodStart is DateTime ps && dto.PeriodEnd is DateTime pe)
        {
            var label = !string.IsNullOrWhiteSpace(dto.Periode)
                ? dto.Periode!
                : (ps.Month == 1 && pe.Month == 12 && ps.Day == 1 && pe.Day == 31 && ps.Year == pe.Year
                    ? ps.Year.ToString()
                    : ps.ToString("yyyy-MM"));
            var limite = ResolveInitDateLimite(dto, pe.Date, dto.Frequency ?? window.Frequency, type, window.FilingDeadline);
            return (ps.Date, pe.Date, limite, label);
        }

        return (window.PeriodStart, window.PeriodEnd,
            ResolveInitDateLimite(dto, window.PeriodEnd, window.Frequency, type, window.FilingDeadline),
            window.PeriodeLabel);
    }

    /// <summary>
    /// Platform shell create for catalogue services (same lifecycle as TVA: contract, period, échéance).
    /// </summary>
    public async Task<ApiResponse<DeclarationDto>> CreateShellAsync(CreateDeclarationDto dto, int cabinetId, string userId)
    {
        var type = DeclarationServiceCatalog.NormalizeType(dto.Type);
        var entry = DeclarationServiceCatalog.Get(type);
        if (entry == null)
            return ApiResponse<DeclarationDto>.Fail($"Type de déclaration '{type}' non supporté.");

        if (!await HasActiveContractAsync(dto.ClientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail($"Impossible de créer une déclaration {entry.DisplayName} : aucun contrat actif.");

        var exoGate = await RequireExerciceAsync(dto.ClientId, cabinetId, dto.ExerciceComptableId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            type,
            DateTime.UtcNow,
            dto.AnneeFiscale ?? exoGate.Data.Annee,
            dto.Periode,
            client.TypeTVA,
            dto.Frequency);

        var (periodStart, periodEnd, dateLimite, periodeLabel) = ApplyPeriodTemplate(dto, window, type);

        var declaration = new Declaration
        {
            ClientId = dto.ClientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Type = type,
            Periode = periodeLabel,
            DateDebut = periodStart,
            DateFin = periodEnd,
            DateLimite = dateLimite,
            Statut = RefCodes.StatutDeclaration.CreeeParSysteme,
            AssistantId = userId,
            CreatedBy = userId,
            DonneesJson = "{}"
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), $"Déclaration {entry.DisplayName} créée.");
    }

    public async Task<ApiResponse<DeclarationDto>> SaveShellAsync(int declarationId, decimal? montantDu, decimal? montantCredit, string? notes, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Modification impossible : aucun contrat actif.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationDto>.Fail("Impossible de modifier une déclaration validée ou soumise.");
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationDto>.Fail(lockErr);

        declaration.MontantDu = montantDu;
        declaration.MontantCredit = montantCredit;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var payload = new { Notes = notes, UpdatedAt = DateTime.UtcNow };
            declaration.DonneesJson = System.Text.Json.JsonSerializer.Serialize(payload);
        }
        declaration.UpdatedAt = DateTime.UtcNow;
        declaration.UpdatedBy = userId;
        if (declaration.Statut == RefCodes.StatutDeclaration.CreeeParSysteme || declaration.Statut == RefCodes.StatutDeclaration.Brouillon)
            declaration.Statut = RefCodes.StatutDeclaration.ACompleter;

        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration enregistrée.");
    }

    public async Task<ApiResponse<DeclarationDto>> SuggestAcompteAsync(int declarationId, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(declaration.Type, "ACOMPTES", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<DeclarationDto>.Fail("Suggestion disponible uniquement pour ACOMPTES.");

        var year = declaration.DateDebut.Year;
        var prevIs = await _db.DeclarationsIS
            .Where(i => i.CabinetId == cabinetId && i.Declaration!.ClientId == declaration.ClientId && i.AnneeFiscale == year - 1)
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        var baseIs = prevIs?.ISDu ?? 0m;
        var acompte = Math.Round(baseIs * 0.25m, 2);
        declaration.MontantDu = acompte;
        declaration.MontantCredit = 0;
        declaration.DonneesJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            Source = "SuggestAcompte",
            PreviousYearIsDu = baseIs,
            Rate = 0.25m,
            Suggested = acompte
        });
        declaration.UpdatedAt = DateTime.UtcNow;
        declaration.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration),
            baseIs > 0
                ? $"Acompte suggéré = 25% × IS {year - 1} ({baseIs:N2}) = {acompte:N2} MAD."
                : "Aucun IS N-1 trouvé — acompte mis à 0. Saisissez le montant manuellement.");
    }

    public async Task<ApiResponse<DeclarationDto>> SuggestRegIsAsync(int declarationId, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(declaration.Type, "REG_IS", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(declaration.Type, "CM", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(declaration.Type, "DRF", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<DeclarationDto>.Fail("Suggestion disponible pour REG_IS / CM / DRF.");

        var year = int.TryParse(declaration.Periode, out var y) ? y : declaration.DateDebut.Year;
        var isData = await _db.DeclarationsIS
            .Where(i => i.CabinetId == cabinetId && i.Declaration!.ClientId == declaration.ClientId && i.AnneeFiscale == year)
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        if (isData == null)
            return ApiResponse<DeclarationDto>.Fail($"Aucune liasse IS {year} trouvée pour ce client. Créez d'abord la déclaration IS.");

        var due = isData.ISDu;
        var cm = Math.Max(isData.ResultatComptable * 0.005m, 3000m);
        due = Math.Max(due, cm);
        var acomptes = isData.AcomptesVerses;
        if (acomptes <= 0)
            acomptes = isData.Acompte1 + isData.Acompte2 + isData.Acompte3 + isData.Acompte4;
        var net = Math.Round(due - acomptes, 2);
        declaration.MontantDu = Math.Max(0, net);
        declaration.MontantCredit = net < 0 ? Math.Abs(net) : 0;
        declaration.DonneesJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            Source = "SuggestRegIs",
            ISDu = isData.ISDu,
            CotisationMinimale = cm,
            Acomptes = acomptes,
            Net = net
        });
        declaration.UpdatedAt = DateTime.UtcNow;
        declaration.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration),
            $"Régularisation suggérée = max(IS, CM) − acomptes = {declaration.MontantDu:N2} MAD.");
    }

    public async Task<ApiResponse<byte[]>> ExportCnssDnsCsvAsync(int declarationId, int cabinetId)
    {
        var dec = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<byte[]>.Fail("Déclaration non trouvée.");
        if (!string.Equals(dec.Type, "CNSS", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<byte[]>.Fail("Export DNS réservé aux déclarations CNSS.");

        var roster = await BuildCnssRosterAsync(dec);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Matricule;CIN;CNSS;Nom;Prenom;DateEmbauche;DateSortie;MotifSortie;Mouvement;JoursTravailles;SalaireBrut;Periode");
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var e in roster.Employees)
        {
            sb.AppendLine(string.Join(';',
                e.MatriculeEmployeur,
                e.CIN,
                e.NumeroCNSS,
                e.Nom,
                e.Prenom,
                e.DateEmbauche.ToString("yyyy-MM-dd"),
                e.DateSortie?.ToString("yyyy-MM-dd") ?? "",
                e.MotifSortie,
                e.Movement,
                e.JoursTravailles,
                e.SalaireBrut.ToString(inv),
                dec.Periode));
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return ApiResponse<byte[]>.Ok(bytes, $"{roster.Employees.Count} salarié(s) exporté(s).");
    }

    public async Task<ApiResponse<CnssEmployeeRosterDto>> GetCnssEmployeesAsync(int declarationId, int cabinetId)
    {
        var dec = await _db.Declarations.Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<CnssEmployeeRosterDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(dec.Type, "CNSS", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Réservé aux déclarations CNSS.");

        var roster = await BuildCnssRosterAsync(dec);
        return ApiResponse<CnssEmployeeRosterDto>.Ok(roster);
    }

    public async Task<ApiResponse<CnssEmployeeRosterDto>> AddCnssEmployeeAsync(int declarationId, CnssEmployeeAddDto dto, int cabinetId, string userId)
    {
        var dec = await _db.Declarations.Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<CnssEmployeeRosterDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(dec.Type, "CNSS", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Réservé aux déclarations CNSS.");
        if (!await HasActiveContractAsync(dec.ClientId, cabinetId))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Le client n'a aucun contrat actif ou validé.");
        if (string.IsNullOrWhiteSpace(dto.CIN) || string.IsNullOrWhiteSpace(dto.Nom) || string.IsNullOrWhiteSpace(dto.Prenom))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("CIN, Nom et Prénom sont obligatoires.");

        // Client employees only — ClientId forced from declaration
        if (await _db.Employees.AnyAsync(e => e.CabinetId == cabinetId && e.CIN == dto.CIN && !e.IsDeleted))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Un employé avec ce CIN existe déjà.");
        if (!string.IsNullOrWhiteSpace(dto.NumeroCNSS)
            && await _db.Employees.AnyAsync(e => e.ClientId == dec.ClientId && e.NumeroCNSS == dto.NumeroCNSS && !e.IsDeleted))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Un employé avec ce numéro CNSS existe déjà pour ce client.");

        var employee = new Employee
        {
            ClientId = dec.ClientId,
            CabinetId = cabinetId,
            CreatedBy = userId,
            CIN = dto.CIN.Trim(),
            Nom = dto.Nom.Trim(),
            Prenom = dto.Prenom.Trim(),
            NumeroCNSS = string.IsNullOrWhiteSpace(dto.NumeroCNSS) ? null : dto.NumeroCNSS.Trim(),
            MatriculeEmployeur = string.IsNullOrWhiteSpace(dto.MatriculeEmployeur) ? null : dto.MatriculeEmployeur.Trim(),
            DateEmbauche = dto.DateEmbauche == default ? dec.DateDebut : dto.DateEmbauche,
            DateSortie = dto.DateSortie,
            MotifSortie = dto.MotifSortie,
            SalaireBase = dto.SalaireBase,
            Sexe = dto.Sexe,
            Statut = dto.DateSortie.HasValue ? RefCodes.StatutEmploye.Inactif : RefCodes.StatutEmploye.Actif
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var roster = await BuildCnssRosterAsync(dec);
        return ApiResponse<CnssEmployeeRosterDto>.Ok(roster, "Salarié du client ajouté.");
    }

    public async Task<ApiResponse<CnssEmployeeRosterDto>> UpdateCnssEmployeeAsync(
        int declarationId, int employeeId, CnssEmployeeAddDto dto, int cabinetId, string userId)
    {
        var dec = await _db.Declarations.Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<CnssEmployeeRosterDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(dec.Type, "CNSS", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Réservé aux déclarations CNSS.");
        if (!await HasActiveContractAsync(dec.ClientId, cabinetId))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Le client n'a aucun contrat actif ou validé.");
        if (string.IsNullOrWhiteSpace(dto.CIN) || string.IsNullOrWhiteSpace(dto.Nom) || string.IsNullOrWhiteSpace(dto.Prenom))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("CIN, Nom et Prénom sont obligatoires.");

        var employee = await _db.Employees.FirstOrDefaultAsync(e =>
            e.Id == employeeId && e.ClientId == dec.ClientId && e.CabinetId == cabinetId && !e.IsDeleted);
        if (employee == null)
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Salarié du client introuvable.");

        if (await _db.Employees.AnyAsync(e =>
                e.CabinetId == cabinetId && e.CIN == dto.CIN && e.Id != employeeId && !e.IsDeleted))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Un employé avec ce CIN existe déjà.");
        if (!string.IsNullOrWhiteSpace(dto.NumeroCNSS)
            && await _db.Employees.AnyAsync(e =>
                e.ClientId == dec.ClientId && e.NumeroCNSS == dto.NumeroCNSS && e.Id != employeeId && !e.IsDeleted))
            return ApiResponse<CnssEmployeeRosterDto>.Fail("Un employé avec ce numéro CNSS existe déjà pour ce client.");

        employee.CIN = dto.CIN.Trim();
        employee.Nom = dto.Nom.Trim();
        employee.Prenom = dto.Prenom.Trim();
        employee.NumeroCNSS = string.IsNullOrWhiteSpace(dto.NumeroCNSS) ? null : dto.NumeroCNSS.Trim();
        employee.MatriculeEmployeur = string.IsNullOrWhiteSpace(dto.MatriculeEmployeur) ? null : dto.MatriculeEmployeur.Trim();
        employee.DateEmbauche = dto.DateEmbauche == default ? employee.DateEmbauche : dto.DateEmbauche;
        employee.DateSortie = dto.DateSortie;
        employee.MotifSortie = dto.MotifSortie;
        employee.SalaireBase = dto.SalaireBase;
        if (!string.IsNullOrWhiteSpace(dto.Sexe)) employee.Sexe = dto.Sexe;
        employee.Statut = dto.DateSortie.HasValue ? RefCodes.StatutEmploye.Inactif : RefCodes.StatutEmploye.Actif;
        employee.UpdatedBy = userId;
        employee.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        var roster = await BuildCnssRosterAsync(dec);
        return ApiResponse<CnssEmployeeRosterDto>.Ok(roster, "Salarié du client mis à jour.");
    }

    public async Task<ApiResponse<CnssEmployeeImportResultDto>> ImportCnssEmployeesAsync(
        int declarationId, List<CnssEmployeeImportRowDto> rows, int cabinetId, string userId)
    {
        var dec = await _db.Declarations.Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (dec == null) return ApiResponse<CnssEmployeeImportResultDto>.Fail("Déclaration non trouvée.");
        if (!string.Equals(dec.Type, "CNSS", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<CnssEmployeeImportResultDto>.Fail("Réservé aux déclarations CNSS.");
        if (!await HasActiveContractAsync(dec.ClientId, cabinetId))
            return ApiResponse<CnssEmployeeImportResultDto>.Fail("Le client n'a aucun contrat actif ou validé.");
        if (rows == null || rows.Count == 0)
            return ApiResponse<CnssEmployeeImportResultDto>.Fail("Aucune ligne à importer.");

        var result = new CnssEmployeeImportResultDto();
        var existing = await _db.Employees
            .Where(e => e.ClientId == dec.ClientId && !e.IsDeleted)
            .ToListAsync();

        var lineNo = 0;
        foreach (var row in rows)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(row.CIN) || string.IsNullOrWhiteSpace(row.Nom) || string.IsNullOrWhiteSpace(row.Prenom))
            {
                result.Errors.Add($"Ligne {lineNo}: CIN, Nom et Prénom obligatoires.");
                continue;
            }

            var cin = row.CIN.Trim();
            var cnss = string.IsNullOrWhiteSpace(row.CNSS) ? null : row.CNSS.Trim();
            Employee? match = null;
            if (!string.IsNullOrWhiteSpace(cnss))
                match = existing.FirstOrDefault(e => string.Equals(e.NumeroCNSS, cnss, StringComparison.OrdinalIgnoreCase));
            match ??= existing.FirstOrDefault(e => string.Equals(e.CIN, cin, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                // Avoid CNSS collision with another employee when creating
                if (!string.IsNullOrWhiteSpace(cnss)
                    && existing.Any(e => string.Equals(e.NumeroCNSS, cnss, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add($"Ligne {lineNo}: N° CNSS {cnss} déjà utilisé pour un autre salarié.");
                    continue;
                }

                match = new Employee
                {
                    ClientId = dec.ClientId,
                    CabinetId = cabinetId,
                    CreatedBy = userId,
                    CIN = cin,
                    Nom = row.Nom.Trim(),
                    Prenom = row.Prenom.Trim(),
                    NumeroCNSS = cnss,
                    MatriculeEmployeur = string.IsNullOrWhiteSpace(row.Matricule) ? null : row.Matricule.Trim(),
                    DateEmbauche = row.DateEmbauche ?? dec.DateDebut,
                    DateSortie = row.DateSortie,
                    MotifSortie = row.MotifSortie,
                    SalaireBase = row.SalaireBrut ?? 0,
                    Sexe = row.Sexe,
                    Statut = row.DateSortie.HasValue ? RefCodes.StatutEmploye.Inactif : RefCodes.StatutEmploye.Actif
                };
                _db.Employees.Add(match);
                existing.Add(match);
                result.Created++;
            }
            else
            {
                match.Nom = row.Nom.Trim();
                match.Prenom = row.Prenom.Trim();
                match.CIN = cin;
                if (!string.IsNullOrWhiteSpace(cnss)) match.NumeroCNSS = cnss;
                if (!string.IsNullOrWhiteSpace(row.Matricule)) match.MatriculeEmployeur = row.Matricule.Trim();
                if (row.DateEmbauche.HasValue) match.DateEmbauche = row.DateEmbauche.Value;
                match.DateSortie = row.DateSortie;
                match.MotifSortie = row.MotifSortie;
                if (row.SalaireBrut.HasValue) match.SalaireBase = row.SalaireBrut.Value;
                if (!string.IsNullOrWhiteSpace(row.Sexe)) match.Sexe = row.Sexe;
                match.Statut = match.DateSortie.HasValue ? RefCodes.StatutEmploye.Inactif : RefCodes.StatutEmploye.Actif;
                match.UpdatedBy = userId;
                match.UpdatedAt = DateTime.UtcNow;
                result.Updated++;
            }
        }

        await _db.SaveChangesAsync();
        result.Roster = await BuildCnssRosterAsync(dec);
        var msg = $"Import terminé : {result.Created} créé(s), {result.Updated} mis à jour.";
        if (result.Errors.Count > 0) msg += $" {result.Errors.Count} erreur(s).";
        return ApiResponse<CnssEmployeeImportResultDto>.Ok(result, msg);
    }

    private async Task<CnssEmployeeRosterDto> BuildCnssRosterAsync(Declaration dec)
    {
        var employees = await _db.Employees
            .Where(e => e.ClientId == dec.ClientId && !e.IsDeleted)
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .ToListAsync();

        var debut = dec.DateDebut.Date;
        var fin = dec.DateFin.Date;
        var lines = new List<CnssEmployeeLineDto>();
        foreach (var e in employees)
        {
            if (!IsPresentInPeriod(e, debut, fin)) continue;
            var isEntrant = IsEntrantInPeriod(e, debut, fin);
            var isSortant = IsSortantInPeriod(e, debut, fin);
            lines.Add(MapCnssLine(e, isEntrant, isSortant, debut, fin));
        }

        return new CnssEmployeeRosterDto
        {
            DeclarationId = dec.Id,
            ClientId = dec.ClientId,
            ClientName = dec.Client?.RaisonSociale,
            DateDebut = debut,
            DateFin = fin,
            PresentCount = lines.Count,
            EntrantCount = lines.Count(l => l.IsEntrant),
            SortantCount = lines.Count(l => l.IsSortant),
            Employees = lines
        };
    }

    private static CnssEmployeeLineDto MapCnssLine(Employee e, bool isEntrant, bool isSortant, DateTime debut, DateTime fin)
    {
        var salaire = e.SalaireBase + (e.PrimeAnciennete ?? 0) + (e.PrimeResponsabilite ?? 0) + (e.PrimePresence ?? 0);
        return new CnssEmployeeLineDto
        {
            EmployeeId = e.Id,
            CIN = e.CIN,
            Nom = e.Nom,
            Prenom = e.Prenom,
            NumeroCNSS = e.NumeroCNSS,
            MatriculeEmployeur = e.MatriculeEmployeur,
            DateEmbauche = e.DateEmbauche,
            DateSortie = e.DateSortie,
            MotifSortie = e.MotifSortie,
            IsEntrant = isEntrant,
            IsSortant = isSortant,
            JoursTravailles = DefaultJoursTravailles(e, debut, fin),
            SalaireBrut = Math.Round(salaire, 2),
            Sexe = e.Sexe
        };
    }

    private static bool IsPresentInPeriod(Employee e, DateTime debut, DateTime fin)
        => e.DateEmbauche.Date <= fin && (e.DateSortie == null || e.DateSortie.Value.Date >= debut);

    private static bool IsEntrantInPeriod(Employee e, DateTime debut, DateTime fin)
        => e.DateEmbauche.Date >= debut && e.DateEmbauche.Date <= fin;

    private static bool IsSortantInPeriod(Employee e, DateTime debut, DateTime fin)
        => e.DateSortie.HasValue && e.DateSortie.Value.Date >= debut && e.DateSortie.Value.Date <= fin;

    private static int DefaultJoursTravailles(Employee e, DateTime debut, DateTime fin)
    {
        var start = e.DateEmbauche.Date > debut ? e.DateEmbauche.Date : debut;
        var end = e.DateSortie.HasValue && e.DateSortie.Value.Date < fin ? e.DateSortie.Value.Date : fin;
        if (end < start) return 0;
        return Math.Min(26, (end - start).Days + 1);
    }

    /// <summary>Morocco métier calendar defaults by declaration type + frequency.</summary>
    private static (DateTime debut, DateTime fin, DateTime limite, string periode) ResolvePeriod(string type, int? anneeFiscale, string? periodeOverride, string? clientTypeTva = null)
    {
        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            type,
            DateTime.UtcNow,
            anneeFiscale,
            periodeOverride,
            clientTypeTva);
        return (window.PeriodStart, window.PeriodEnd, window.FilingDeadline, window.PeriodeLabel);
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
        => await CreateTVAInternalAsync(clientId, cabinetId, userId, null);

    private async Task<ApiResponse<DeclarationDto>> CreateTVAInternalAsync(int clientId, int cabinetId, string userId, int? exerciceId, CreateDeclarationDto? periodCfg = null)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration TVA : le client n'a aucun contrat actif ou validé.");

        var exoGate = await RequireExerciceAsync(clientId, cabinetId, exerciceId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            "TVA", DateTime.UtcNow, exoGate.Data.Annee, periodCfg?.Periode, client.TypeTVA, periodCfg?.Frequency);
        var (periodStart, periodEnd, dateLimite, periodeLabel) = ApplyPeriodTemplate(periodCfg, window, "TVA");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Type = "TVA",
            Periode = periodeLabel,
            DateDebut = periodStart,
            DateFin = periodEnd,
            DateLimite = dateLimite,
            Statut = RefCodes.StatutDeclaration.CreeeParSysteme,
            AssistantId = userId,
            CreatedBy = userId
        };
        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();

        var previousCredit = await _db.DeclarationsTVA
            .Where(t => t.Declaration!.ClientId == clientId && t.Declaration.Type == "TVA"
                && (t.Declaration.Statut == RefCodes.StatutDeclaration.Soumise
                    || t.Declaration.Statut == RefCodes.StatutDeclaration.Deposee
                    || t.Declaration.Statut == RefCodes.StatutDeclaration.Payee
                    || t.Declaration.Statut == RefCodes.StatutDeclaration.EnvoyeeSimplTva
                    || t.Declaration.Statut == RefCodes.StatutDeclaration.Cloturee))
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
        var exoClosed = await EnsureDeclarationExerciceOpenAsync(declaration, cabinetId);
        if (exoClosed != null) return ApiResponse<DeclarationTVADto>.Fail(exoClosed);
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationTVADto>.Fail(lockErr);

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
    /// Saves manual Moroccan TVA totals:
    /// TVA à payer = TVA facturée − TVA déductible − crédit déclaration précédente.
    /// Invoice lines still override when present (after add/import).
    /// </summary>
    public async Task<ApiResponse<DeclarationTVADto>> SaveTVAAjustementsAsync(int declarationId, decimal tvaFacturee, decimal tvaDeductible, decimal creditPrecedent, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationTVADto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationTVADto>.Fail("Modification impossible : le client n’a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee)
            return ApiResponse<DeclarationTVADto>.Fail("Impossible de modifier une déclaration validée ou soumise.");
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationTVADto>.Fail(lockErr);

        var tva = await _db.DeclarationsTVA.FirstOrDefaultAsync(t => t.DeclarationId == declarationId && t.CabinetId == cabinetId);
        if (tva == null)
        {
            tva = new DeclarationTVA { DeclarationId = declarationId, CabinetId = cabinetId, CreatedBy = userId };
            _db.DeclarationsTVA.Add(tva);
            await _db.SaveChangesAsync();
        }

        var hasVentes = await _db.Set<TVALigneFacture>().AnyAsync(l =>
            l.DeclarationTVAId == tva.Id && l.CabinetId == cabinetId && !l.IsDeleted && l.TypeOperation == "Vente");
        var hasAchats = await _db.Set<TVALigneFacture>().AnyAsync(l =>
            l.DeclarationTVAId == tva.Id && l.CabinetId == cabinetId && !l.IsDeleted && l.TypeOperation == "Achat");

        // Manual facturée only when no sale lines (otherwise lines are source of truth)
        if (!hasVentes)
        {
            tva.TVACollecteeTotale = Math.Round(tvaFacturee, 2);
            // Keep rate breakdown coherent for display/EDI when entered as a single total
            tva.TVA_20 = tva.TVACollecteeTotale;
            tva.TVA_14 = 0; tva.TVA_10 = 0; tva.TVA_7 = 0;
            tva.CAHT_20 = tva.TVACollecteeTotale > 0 ? Math.Round(tva.TVACollecteeTotale / 0.20m, 2) : 0;
            tva.CAHT_14 = 0; tva.CAHT_10 = 0; tva.CAHT_7 = 0; tva.CAExonere = 0;
        }

        // Manual déductible only when no purchase lines
        if (!hasAchats)
        {
            tva.TVARecuperableCharges = Math.Round(tvaDeductible, 2);
            tva.TVARecuperableImmobilisations = 0;
            tva.TVARecuperableTotale = tva.TVARecuperableCharges;
        }

        tva.CreditPrecedent = Math.Round(creditPrecedent, 2);

        // If lines exist for one side, refresh that side from lines first
        if (hasVentes || hasAchats)
            await RecalculateTVATotalsFromLignesAsync(tva.Id, declarationId, cabinetId);
        else
            ApplyTvaNetFromTotals(tva, declaration);

        await _db.SaveChangesAsync();
        await _db.Entry(tva).ReloadAsync();
        var result = _mapper.Map<DeclarationTVADto>(tva);
        result.DeclarationId = declarationId;
        return ApiResponse<DeclarationTVADto>.Ok(result, "Ajustements TVA enregistrés.");
    }

    private static void ApplyTvaNetFromTotals(DeclarationTVA tva, Declaration declaration)
    {
        // Maroc: TVA à payer = TVA facturée − TVA déductible − crédit déclaration précédente
        tva.TVARecuperableTotale = tva.TVARecuperableCharges + tva.TVARecuperableImmobilisations;
        tva.TVANette = tva.TVACollecteeTotale - tva.TVARecuperableTotale - tva.CreditPrecedent;
        tva.TVAaPayer = Math.Max(0, tva.TVANette);
        tva.CreditTVA = Math.Max(0, -tva.TVANette);
        declaration.MontantDu = tva.TVAaPayer;
        declaration.MontantCredit = tva.CreditTVA;
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
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<TVALigneFactureDto>.Fail(lockErr);

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
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<bool>.Fail(lockErr);

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

        // Vente (Facturée) — lines override when present; else keep manual TVACollecteeTotale
        var ventes = lignes.Where(l => l.TypeOperation == "Vente").ToList();
        if (ventes.Count > 0)
        {
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
        }

        // Achat (Déductible) — lines override charges when present; else keep manual
        var achats = lignes.Where(l => l.TypeOperation == "Achat").ToList();
        if (achats.Count > 0)
            tva.TVARecuperableCharges = achats.Sum(l => l.MontantTVA);
        tva.TVARecuperableTotale = tva.TVARecuperableCharges + tva.TVARecuperableImmobilisations;

        // Maroc: Facturée − Déductible − Crédit déclaration précédente
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
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<List<TVALigneFactureDto>>.Fail(lockErr);

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
        => await CreateISInternalAsync(clientId, anneeFiscale, cabinetId, userId, null);

    private async Task<ApiResponse<DeclarationDto>> CreateISInternalAsync(int clientId, int anneeFiscale, int cabinetId, string userId, int? exerciceId, CreateDeclarationDto? periodCfg = null)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration IS : le client n'a aucun contrat actif ou validé.");

        var exoGate = await RequireExerciceAsync(clientId, cabinetId, exerciceId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var freq = periodCfg?.Frequency ?? "Annuelle";
        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            "IS", DateTime.UtcNow, anneeFiscale, periodCfg?.Periode, null, freq);
        var (debut, fin, dateLimite, periode) = ApplyPeriodTemplate(periodCfg, window, "IS");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Type = "IS",
            Periode = periode,
            DateDebut = debut,
            DateFin = fin,
            DateLimite = dateLimite,
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
        await EnsureAnnualChecklistInternalAsync(declaration.Id, cabinetId, userId);
        return ApiResponse<DeclarationDto>.Ok(_mapper.Map<DeclarationDto>(declaration), "Déclaration fiscale (liasse) créée.");
    }

    public async Task<ApiResponse<DeclarationISDto>> GetISDataAsync(int declarationId, int cabinetId)
    {
        var isData = await _db.DeclarationsIS.FirstOrDefaultAsync(i => i.DeclarationId == declarationId && i.CabinetId == cabinetId);
        if (isData == null) return ApiResponse<DeclarationISDto>.Fail("Données IS non trouvées.");
        return ApiResponse<DeclarationISDto>.Ok(_mapper.Map<DeclarationISDto>(isData));
    }

    public Task<ApiResponse<DeclarationISDto>> CalculateISAsync(DeclarationISDto dto)
    {
        // Aggregate retraitement lines into totals when provided
        if (dto.Retraitements is { Count: > 0 })
        {
            dto.Reintegrations = dto.Retraitements.Where(r => string.Equals(r.Type, "Deduction", StringComparison.OrdinalIgnoreCase) == false)
                .Sum(r => r.Montant);
            dto.Deductions = dto.Retraitements.Where(r => string.Equals(r.Type, "Deduction", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Montant);
            dto.RetraitementsJson = System.Text.Json.JsonSerializer.Serialize(dto.Retraitements);
        }

        dto.ResultatFiscal = dto.ResultatComptable + dto.Reintegrations - dto.Deductions;
        dto.BaseImposable = Math.Max(0, dto.ResultatFiscal - dto.DeficitsAnterieurs);

        // Moroccan IS rates (aligned UI + API): 10 / 20 / 31 / 35
        decimal isDu;
        if (dto.BaseImposable <= 300_000m) { dto.TauxIS = 10m; isDu = dto.BaseImposable * 0.10m; }
        else if (dto.BaseImposable <= 1_000_000m) { dto.TauxIS = 20m; isDu = dto.BaseImposable * 0.20m; }
        else if (dto.BaseImposable <= 100_000_000m) { dto.TauxIS = 31m; isDu = dto.BaseImposable * 0.31m; }
        else { dto.TauxIS = 35m; isDu = dto.BaseImposable * 0.35m; }

        dto.ISDu = Math.Round(isDu, 2);
        // Cotisation minimale : 0,5 % du CA/résultat comptable (plancher 3 000 MAD)
        dto.CotisationMinimale = Math.Round(Math.Max(dto.ResultatComptable * 0.005m, 3000m), 2);
        dto.ISDu = Math.Max(dto.ISDu, dto.CotisationMinimale);

        if (dto.AcomptesVerses <= 0)
            dto.AcomptesVerses = dto.Acompte1 + dto.Acompte2 + dto.Acompte3 + dto.Acompte4;

        dto.ISNet = dto.ISDu - dto.AcomptesVerses;

        return Task.FromResult(ApiResponse<DeclarationISDto>.Ok(dto));
    }

    public async Task<ApiResponse<DeclarationISDto>> SaveISDataAsync(int declarationId, DeclarationISDto dto, int cabinetId, string userId)
    {
        var declaration = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (declaration == null) return ApiResponse<DeclarationISDto>.Fail("Déclaration non trouvée.");
        if (!await HasActiveContractAsync(declaration.ClientId, cabinetId))
            return ApiResponse<DeclarationISDto>.Fail("Modification impossible : le client n'a aucun contrat actif ou validé.");
        if (declaration.Statut == RefCodes.StatutDeclaration.Soumise || declaration.Statut == RefCodes.StatutDeclaration.Validee
            || declaration.Statut == RefCodes.StatutDeclaration.EnvoyeeSimplIs || declaration.Statut == RefCodes.StatutDeclaration.Deposee
            || declaration.Statut == RefCodes.StatutDeclaration.Cloturee)
            return ApiResponse<DeclarationISDto>.Fail("Impossible de modifier une déclaration validée, soumise ou clôturée.");
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationISDto>.Fail(lockErr);

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
        isData.CotisationMinimale = calculated.CotisationMinimale;
        isData.Acompte1 = calculated.Acompte1; isData.Acompte2 = calculated.Acompte2;
        isData.Acompte3 = calculated.Acompte3; isData.Acompte4 = calculated.Acompte4;
        isData.RetraitementsJson = calculated.RetraitementsJson ?? dto.RetraitementsJson;
        if (!string.IsNullOrWhiteSpace(dto.LiassePanesJson))
            isData.LiassePanesJson = dto.LiassePanesJson;
        isData.UpdatedBy = userId;
        isData.UpdatedAt = DateTime.UtcNow;

        declaration.MontantDu = Math.Max(0, calculated.ISNet);
        declaration.MontantCredit = calculated.ISNet < 0 ? Math.Abs(calculated.ISNet) : 0;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationISDto>.Ok(calculated, "Données IS / liasse sauvegardées.");
    }

    private static readonly (string Code, string Label, string Categorie)[] AnnualChecklistTemplate =
    {
        ("BALANCE", "Balance générale / grand livre", "Comptable"),
        ("JOURNAUX", "Journaux (achats, ventes, banque, OD)", "Comptable"),
        ("BANQUES", "Relevés bancaires annuels + rapprochements", "Bancaire"),
        ("INVENTAIRE", "Inventaire stocks / travaux en cours", "Comptable"),
        ("IMMOB", "État des immobilisations & amortissements", "Comptable"),
        ("PAIE", "États de paie / CNSS / IR salaires", "Social"),
        ("FACTURES", "Factures d'achat et de vente significatives", "Fiscal"),
        ("PV_AG", "PV Assemblée Générale / affectation résultat", "Legal"),
        ("CONTRATS", "Contrats, baux, emprunts en cours", "Legal"),
        ("PROVISIONS", "Détail provisions et créances douteuses", "Comptable"),
    };

    private async Task EnsureAnnualChecklistInternalAsync(int declarationId, int cabinetId, string userId)
    {
        var existing = await _db.DeclarationChecklistItems
            .CountAsync(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted);
        if (existing > 0) return;

        var order = 0;
        foreach (var (code, label, cat) in AnnualChecklistTemplate)
        {
            _db.DeclarationChecklistItems.Add(new DeclarationChecklistItem
            {
                DeclarationId = declarationId,
                CabinetId = cabinetId,
                Code = code,
                Label = label,
                Categorie = cat,
                SortOrder = order++,
                IsRequired = true,
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<ApiResponse<List<DeclarationChecklistItemDto>>> GetChecklistAsync(int declarationId, int cabinetId)
    {
        var items = await _db.DeclarationChecklistItems
            .Where(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return ApiResponse<List<DeclarationChecklistItemDto>>.Ok(_mapper.Map<List<DeclarationChecklistItemDto>>(items));
    }

    public async Task<ApiResponse<List<DeclarationChecklistItemDto>>> EnsureAnnualChecklistAsync(int declarationId, int cabinetId, string userId)
    {
        var decl = await _db.Declarations.FirstOrDefaultAsync(d => d.Id == declarationId && d.CabinetId == cabinetId);
        if (decl == null) return ApiResponse<List<DeclarationChecklistItemDto>>.Fail("Déclaration non trouvée.");
        await EnsureAnnualChecklistInternalAsync(declarationId, cabinetId, userId);
        return await GetChecklistAsync(declarationId, cabinetId);
    }

    public async Task<ApiResponse<DeclarationChecklistItemDto>> MarkChecklistItemReceivedAsync(
        int declarationId, int itemId, int? documentId, int cabinetId, string userId)
    {
        var item = await _db.DeclarationChecklistItems
            .FirstOrDefaultAsync(c => c.Id == itemId && c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted);
        if (item == null) return ApiResponse<DeclarationChecklistItemDto>.Fail("Pièce checklist introuvable.");

        item.IsReceived = true;
        item.ReceivedAt = DateTime.UtcNow;
        item.DocumentId = documentId ?? item.DocumentId;
        item.UpdatedBy = userId;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<DeclarationChecklistItemDto>.Ok(_mapper.Map<DeclarationChecklistItemDto>(item), "Pièce marquée reçue.");
    }

    public async Task<ApiResponse<bool>> IsChecklistCompleteAsync(int declarationId, int cabinetId)
    {
        var required = await _db.DeclarationChecklistItems
            .Where(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted && c.IsRequired)
            .ToListAsync();
        if (required.Count == 0) return ApiResponse<bool>.Ok(true);
        var complete = required.All(c => c.IsReceived);
        return ApiResponse<bool>.Ok(complete);
    }

    // ═══════════════════════════════════════════════════════════════
    //  IR (Impôt sur le Revenu)
    // ═══════════════════════════════════════════════════════════════
    public async Task<ApiResponse<DeclarationDto>> CreateIRAsync(int clientId, int anneeFiscale, int cabinetId, string userId)
        => await CreateIRInternalAsync(clientId, anneeFiscale, cabinetId, userId, null);

    private async Task<ApiResponse<DeclarationDto>> CreateIRInternalAsync(int clientId, int anneeFiscale, int cabinetId, string userId, int? exerciceId, CreateDeclarationDto? periodCfg = null)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration IR : le client n'a aucun contrat actif ou validé.");

        var exoGate = await RequireExerciceAsync(clientId, cabinetId, exerciceId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var freq = periodCfg?.Frequency ?? "Annuelle";
        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            "IR", DateTime.UtcNow, anneeFiscale, periodCfg?.Periode, null, freq);
        var (debut, fin, dateLimite, periode) = ApplyPeriodTemplate(periodCfg, window, "IR");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Type = "IR",
            Periode = periode,
            DateDebut = debut,
            DateFin = fin,
            DateLimite = dateLimite,
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
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationIRDto>.Fail(lockErr);

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
        => await CreateCNSSInternalAsync(clientId, cabinetId, userId, null);

    private async Task<ApiResponse<DeclarationDto>> CreateCNSSInternalAsync(int clientId, int cabinetId, string userId, int? exerciceId, CreateDeclarationDto? periodCfg = null)
    {
        if (!await HasActiveContractAsync(clientId, cabinetId))
            return ApiResponse<DeclarationDto>.Fail("Impossible de créer une déclaration CNSS : le client n'a aucun contrat actif ou validé.");

        var exoGate = await RequireExerciceAsync(clientId, cabinetId, exerciceId);
        if (!exoGate.Success || exoGate.Data == null)
            return ApiResponse<DeclarationDto>.Fail(exoGate.Message ?? ExerciceContextService.NoOpenExerciceMessage);

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DeclarationDto>.Fail("Client non trouvé.");

        var window = MoroccoDeclarationCalendar.ResolveForCreate(
            "CNSS", DateTime.UtcNow, exoGate.Data.Annee, periodCfg?.Periode, null, periodCfg?.Frequency);
        var (periodStart, periodEnd, dateLimite, periodeLabel) = ApplyPeriodTemplate(periodCfg, window, "CNSS");

        var declaration = new Declaration
        {
            ClientId = clientId,
            CabinetId = cabinetId,
            ExerciceComptableId = exoGate.Data.Id,
            Type = "CNSS",
            Periode = periodeLabel,
            DateDebut = periodStart,
            DateFin = periodEnd,
            DateLimite = dateLimite,
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

        // Client employees only, filtered to declaration period (présents / entrants / sortants)
        var roster = await BuildCnssRosterAsync(dec);
        var employees = await _db.Employees
            .Where(e => e.ClientId == dec.ClientId && !e.IsDeleted)
            .ToListAsync();
        var byId = employees.ToDictionary(e => e.Id);

        decimal totalBruts = 0, totalCotSal = 0, totalCotPat = 0;
        decimal totalAmoSal = 0, totalAmoPat = 0, totalFp = 0;

        foreach (var line in roster.Employees)
        {
            if (!byId.TryGetValue(line.EmployeeId, out var emp)) continue;
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
            NombreEmployes = roster.Employees.Count,
            DetailEmployesJson = System.Text.Json.JsonSerializer.Serialize(roster.Employees)
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
        var lockErr = await EnsureStepHolderCanEditAsync(declarationId, cabinetId, userId);
        if (lockErr != null) return ApiResponse<DeclarationCNSSDto>.Fail(lockErr);

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
        var lockErr = await EnsureStepHolderCanEditAsync(id, cabinetId, userId);
        if (lockErr != null) return ApiResponse<bool>.Fail(lockErr);

        var oldStatut = dec.Statut;
        // Full TVA lifecycle + legacy aliases so old declarations keep working
        var validTransitions = new Dictionary<string, string[]>
        {
            [RefCodes.StatutDeclaration.ACreer] = [RefCodes.StatutDeclaration.CreeeParSysteme, RefCodes.StatutDeclaration.Brouillon],
            [RefCodes.StatutDeclaration.CreeeParSysteme] = [RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.ACompleter],
            [RefCodes.StatutDeclaration.Brouillon] = [RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.ACompleter, RefCodes.StatutDeclaration.CreeeParSysteme],
            [RefCodes.StatutDeclaration.DocumentsDemandes] = [RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.DocumentsRecus, RefCodes.StatutDeclaration.EnAttenteCorrection, RefCodes.StatutDeclaration.ACompleter],
            [RefCodes.StatutDeclaration.ACompleter] = [RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.DocumentsRecus, RefCodes.StatutDeclaration.Completee, RefCodes.StatutDeclaration.ControleEnCours],
            [RefCodes.StatutDeclaration.DocumentsRecus] = [RefCodes.StatutDeclaration.ControleEnCours, RefCodes.StatutDeclaration.EnAttenteCorrection, RefCodes.StatutDeclaration.DocumentsDemandes],
            [RefCodes.StatutDeclaration.ControleEnCours] = [RefCodes.StatutDeclaration.EnAttenteCorrection, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.Completee],
            [RefCodes.StatutDeclaration.EnCours] = [RefCodes.StatutDeclaration.ControleEnCours, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.Completee, RefCodes.StatutDeclaration.EnAttenteCorrection],
            [RefCodes.StatutDeclaration.EnPreparation] = [RefCodes.StatutDeclaration.ControleEnCours, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.Completee],
            [RefCodes.StatutDeclaration.Completee] = [RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.EnRevueSuperviseur, RefCodes.StatutDeclaration.EnRevision, RefCodes.StatutDeclaration.EnAttenteCorrection, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.EnAttenteCorrection] = [RefCodes.StatutDeclaration.ControleEnCours, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.DocumentsDemandes, RefCodes.StatutDeclaration.DocumentsRecus],
            [RefCodes.StatutDeclaration.CalculTVA] = [RefCodes.StatutDeclaration.EnRevueSuperviseur, RefCodes.StatutDeclaration.EnRevision, RefCodes.StatutDeclaration.EnAttenteCorrection, RefCodes.StatutDeclaration.ACorriger],
            [RefCodes.StatutDeclaration.EnRevueSuperviseur] = [RefCodes.StatutDeclaration.Validee, RefCodes.StatutDeclaration.ACorriger, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.EnRevision] = [RefCodes.StatutDeclaration.Validee, RefCodes.StatutDeclaration.ACorriger, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.ACorriger] = [RefCodes.StatutDeclaration.EnRevueSuperviseur, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.EnRevision, RefCodes.StatutDeclaration.ControleEnCours],
            [RefCodes.StatutDeclaration.Refusee] = [RefCodes.StatutDeclaration.ACorriger, RefCodes.StatutDeclaration.Brouillon, RefCodes.StatutDeclaration.ACompleter, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.EnRevueSuperviseur],
            [RefCodes.StatutDeclaration.Validee] = [RefCodes.StatutDeclaration.EnvoyeeSimplTva, RefCodes.StatutDeclaration.Soumise, RefCodes.StatutDeclaration.ACorriger, RefCodes.StatutDeclaration.Refusee],
            [RefCodes.StatutDeclaration.EnvoyeeSimplTva] = [RefCodes.StatutDeclaration.Deposee, RefCodes.StatutDeclaration.Rejetee],
            [RefCodes.StatutDeclaration.Soumise] = [RefCodes.StatutDeclaration.Deposee, RefCodes.StatutDeclaration.Rejetee, RefCodes.StatutDeclaration.Payee, RefCodes.StatutDeclaration.EnvoyeeSimplTva],
            [RefCodes.StatutDeclaration.Rejetee] = [RefCodes.StatutDeclaration.ACorriger, RefCodes.StatutDeclaration.CalculTVA, RefCodes.StatutDeclaration.EnRevueSuperviseur],
            [RefCodes.StatutDeclaration.Deposee] = [RefCodes.StatutDeclaration.APayer, RefCodes.StatutDeclaration.Rapprochee, RefCodes.StatutDeclaration.Payee],
            [RefCodes.StatutDeclaration.APayer] = [RefCodes.StatutDeclaration.Payee],
            [RefCodes.StatutDeclaration.Payee] = [RefCodes.StatutDeclaration.Rapprochee, RefCodes.StatutDeclaration.Archivee],
            [RefCodes.StatutDeclaration.Rapprochee] = [RefCodes.StatutDeclaration.Archivee, RefCodes.StatutDeclaration.Cloturee],
            [RefCodes.StatutDeclaration.Archivee] = [RefCodes.StatutDeclaration.Cloturee],
        };

        if (!validTransitions.ContainsKey(oldStatut) || !validTransitions[oldStatut].Contains(newStatut))
            return ApiResponse<bool>.Fail($"Transition de '{oldStatut}' vers '{newStatut}' non autorisée.");

        dec.Statut = newStatut;
        if (newStatut is RefCodes.StatutDeclaration.Soumise or RefCodes.StatutDeclaration.EnvoyeeSimplTva or RefCodes.StatutDeclaration.Deposee)
            dec.DateSoumission ??= DateTime.UtcNow;
        if (newStatut is RefCodes.StatutDeclaration.Refusee or RefCodes.StatutDeclaration.Rejetee or RefCodes.StatutDeclaration.ACorriger)
            dec.MotifRefus = comment ?? dec.MotifRefus;

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

    public async Task<ApiResponse<List<DeclarationHistoryDto>>> GetStatusHistoryAsync(int declarationId, int cabinetId)
    {
        var rows = await _db.DeclarationHistories
            .AsNoTracking()
            .Where(h => h.DeclarationId == declarationId && h.CabinetId == cabinetId && !h.IsDeleted)
            .OrderByDescending(h => h.DateChangement)
            .ToListAsync();

        var userIds = rows.Where(r => !string.IsNullOrEmpty(r.UserId)).Select(r => r.UserId!).Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => (u.FirstName + " " + u.LastName).Trim());

        var dtos = rows.Select(h => new DeclarationHistoryDto
        {
            Id = h.Id,
            DeclarationId = h.DeclarationId,
            AncienStatut = h.AncienStatut,
            NouveauStatut = h.NouveauStatut,
            UserId = h.UserId,
            UserName = !string.IsNullOrEmpty(h.UserName)
                ? h.UserName
                : (h.UserId != null && names.TryGetValue(h.UserId, out var n) ? n : null),
            Commentaire = h.Commentaire,
            DateChangement = h.DateChangement
        }).ToList();

        return ApiResponse<List<DeclarationHistoryDto>>.Ok(dtos);
    }
}