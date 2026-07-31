using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly MetaComptaDbContext _db;
    private readonly INotificationService _notifications;

    public DashboardService(MetaComptaDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
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

        // Upcoming deadlines
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

        // Recent notifications
        var notifs = await _notifications.GetUserNotificationsAsync(userId, false, 5);
        dashboard.NotificationsRecentes = notifs.Data ?? new();

        // Charts data
        dashboard.Charts = await BuildChartsDataAsync(cabinetId);

        return ApiResponse<DashboardDto>.Ok(dashboard);
    }

    public async Task<ApiResponse<DashboardDto>> GetAssistantDashboardAsync(int cabinetId, string userId)
    {
        var dashboard = await GetManagerDashboardAsync(cabinetId, userId);
        // Assistant sees only their assigned work
        return dashboard;
    }

    public async Task<ApiResponse<ExtranetDashboardDto>> GetExtranetDashboardAsync(int clientId)
    {
        var now = DateTime.UtcNow;
        var client = await _db.Clients.FindAsync(clientId);

        var declarationsEnCours = await _db.Declarations
            .CountAsync(d => d.ClientId == clientId && d.Statut != RefCodes.StatutDeclaration.Soumise && d.Statut != RefCodes.StatutDeclaration.Brouillon);

        var declarationsSoumises = await _db.Declarations
            .CountAsync(d => d.ClientId == clientId && d.Statut == RefCodes.StatutDeclaration.Soumise);

        var upcomingDeadlines = await _db.Declarations
            .Where(d => d.ClientId == clientId && d.Statut != RefCodes.StatutDeclaration.Soumise && d.DateLimite >= now.AddDays(-30))
            .OrderBy(d => d.DateLimite)
            .Take(10)
            .Select(d => new ExtranetDeadlineDto { Id = d.Id, Type = d.Type, Periode = d.Periode, DateLimite = d.DateLimite, Statut = d.Statut })
            .ToListAsync();

        var recentDeclarations = await _db.Declarations
            .Where(d => d.ClientId == clientId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new ExtranetRecentDto { Id = d.Id, Type = d.Type, Periode = d.Periode, DateSoumission = d.DateSoumission, MontantDu = d.MontantDu ?? 0m, Statut = d.Statut })
            .ToListAsync();

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

        var dto = new ExtranetDashboardDto
        {
            ClientName = client?.RaisonSociale,
            ICE = client?.ICE,
            FormeJuridique = client?.FormeJuridique,
            SecteurActivite = client?.SecteurActivite,
            Statut = client?.Statut,
            EmployeeCount = employeeCount,
            DocumentCount = documentCount,
            DeclarationsEnCours = declarationsEnCours,
            DeclarationsSoumises = declarationsSoumises,
            TachesEnAttente = tachesEnAttente,
            MessagesNonLus = messagesNonLus,
            ProchainsDelais = upcomingDeadlines,
            DernieresDeclarations = recentDeclarations
        };

        return ApiResponse<ExtranetDashboardDto>.Ok(dto);
    }

    private async Task<DashboardChartsDto> BuildChartsDataAsync(int cabinetId)
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        // Declarations evolution by month
        var declarationsEvolution = await _db.Declarations
            .Where(d => d.CabinetId == cabinetId && d.CreatedAt >= sixMonthsAgo)
            .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month, d.Type })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, Count = g.Count() })
            .ToListAsync();

        // Client distribution by sector
        var clientDistribution = await _db.Clients
            .Where(c => c.CabinetId == cabinetId && c.Statut == "Active")
            .GroupBy(c => c.SecteurActivite ?? "Non défini")
            .Select(g => new { Sector = g.Key, Count = g.Count() })
            .ToListAsync();

        // Declaration status for current month
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
