using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class WorkflowSchedulerService : IWorkflowSchedulerService
{
    private readonly MetaComptaDbContext _db;
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowSchedulerService> _logger;

    public WorkflowSchedulerService(
        MetaComptaDbContext db,
        IWorkflowService workflowService,
        ILogger<WorkflowSchedulerService> logger)
    {
        _db = db;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task SyncScheduledJobsAsync()
    {
        var scheduledSteps = await _db.WorkflowStepTemplates
            .Where(s => s.IsScheduled
                && !string.IsNullOrEmpty(s.CronExpression)
                && s.AssignmentType == AssignmentType.AutoSystem
                && !s.IsDeleted)
            .Include(s => s.WorkflowTemplate)
            .Where(s => s.WorkflowTemplate != null && s.WorkflowTemplate.IsActive && !s.WorkflowTemplate.IsDeleted)
            .ToListAsync();

        _logger.LogInformation(
            "WorkflowScheduler: Found {Count} scheduled step templates to sync.",
            scheduledSteps.Count);
    }

    public async Task ExecuteScheduledStepAsync(int stepTemplateId, int cabinetId)
    {
        _logger.LogInformation(
            "WorkflowScheduler: Executing scheduled step template {Id} for cabinet {CabinetId}.",
            stepTemplateId, cabinetId);

        try
        {
            var stepTemplate = await _db.WorkflowStepTemplates
                .Include(s => s.WorkflowTemplate)
                .FirstOrDefaultAsync(s => s.Id == stepTemplateId && s.CabinetId == cabinetId && !s.IsDeleted);

            if (stepTemplate == null)
            {
                _logger.LogWarning("WorkflowScheduler: Step template {Id} not found or deleted.", stepTemplateId);
                return;
            }

            var pendingStepInstances = await _db.WorkflowStepInstances
                .Where(si => si.WorkflowStepTemplateId == stepTemplateId
                    && si.CabinetId == cabinetId
                    && (si.Status == WorkflowStepStatus.InProgress
                        || si.Status == WorkflowStepStatus.Pending
                        || si.Status == WorkflowStepStatus.WaitingForValidation))
                .Include(si => si.WorkflowInstance)
                .Where(si => si.WorkflowInstance != null && si.WorkflowInstance.Status == WorkflowInstanceStatus.Active)
                .ToListAsync();

            _logger.LogInformation(
                "WorkflowScheduler: Found {Count} pending instances for step template {Id}.",
                pendingStepInstances.Count, stepTemplateId);

            foreach (var stepInstance in pendingStepInstances)
            {
                try
                {
                    var advanceDto = new DTOs.AdvanceStepDto
                    {
                        Notes = $"Auto-exécuté par le planificateur CRON ({stepTemplate.CronDescription ?? stepTemplate.CronExpression})",
                        Comment = "Exécution automatique planifiée"
                    };

                    var result = await _workflowService.AdvanceStepAsync(
                        stepInstance.WorkflowInstanceId, advanceDto, cabinetId, "SYSTEM");

                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "WorkflowScheduler: Successfully advanced instance {InstanceId} step {StepOrder}.",
                            stepInstance.WorkflowInstanceId, stepInstance.StepOrder);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "WorkflowScheduler: Failed to advance instance {InstanceId}: {Message}",
                            stepInstance.WorkflowInstanceId, result.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "WorkflowScheduler: Error processing instance {InstanceId} for step template {Id}.",
                        stepInstance.WorkflowInstanceId, stepTemplateId);
                }
            }

            if (stepTemplate.WorkflowTemplate != null
                && stepTemplate.StepOrder == 0
                && stepTemplate.ActionOnEntry == WorkflowActionType.CreateDeclaration)
            {
                await AutoCreateScheduledWorkflowsAsync(stepTemplate, cabinetId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "WorkflowScheduler: Unhandled error executing step template {Id}.",
                stepTemplateId);
        }
    }

    private async Task AutoCreateScheduledWorkflowsAsync(WorkflowStepTemplate stepTemplate, int cabinetId)
    {
        var template = stepTemplate.WorkflowTemplate;
        if (template == null) return;

        _logger.LogInformation(
            "WorkflowScheduler: Auto-creating workflows for template '{Name}' (ID: {Id}).",
            template.Name, template.Id);

        var declType = template.DeclarationType;
        var clientsQuery = _db.Clients.Where(c => c.CabinetId == cabinetId && !c.IsDeleted && c.Statut == "Actif");

        if (template.ClientId.HasValue)
        {
            clientsQuery = clientsQuery.Where(c => c.Id == template.ClientId.Value);
        }

        var clients = await clientsQuery.Select(c => c.Id).ToListAsync();

        foreach (var clientId in clients)
        {
            var hasActive = await _db.WorkflowInstances
                .AnyAsync(wi => wi.WorkflowTemplateId == template.Id
                    && wi.ClientId == clientId
                    && wi.CabinetId == cabinetId
                    && wi.Status == WorkflowInstanceStatus.Active);

            if (hasActive) continue;

            try
            {
                var startDto = new DTOs.StartWorkflowDto
                {
                    ClientId = clientId,
                    DeclarationType = declType.ToString(),
                    WorkflowTemplateId = template.Id
                };

                var result = await _workflowService.StartWorkflowAsync(startDto, cabinetId, "SYSTEM");

                if (result.Success)
                {
                    _logger.LogInformation(
                        "WorkflowScheduler: Auto-started workflow for client {ClientId}, template '{Name}'.",
                        clientId, template.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "WorkflowScheduler: Failed to auto-start workflow for client {ClientId}.", clientId);
            }
        }
    }

    public async Task ProcessDueRelancesAsync()
    {
        try
        {
            var count = await _workflowService.ProcessDueRelancesAsync();
            if (count > 0)
                _logger.LogInformation("WorkflowScheduler: Processed {Count} due relance(s).", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkflowScheduler: ProcessDueRelances failed.");
        }
    }

    public async Task ProcessDueLifecycleEndsAsync()
    {
        try
        {
            var count = await _workflowService.ProcessDueLifecycleEndsAsync();
            if (count > 0)
                _logger.LogInformation("WorkflowScheduler: Processed {Count} lifecycle end(s).", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkflowScheduler: ProcessDueLifecycleEnds failed.");
        }
    }
}
