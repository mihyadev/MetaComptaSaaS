using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class NotificationService : INotificationService
{
    private readonly MetaComptaDbContext _db;

    public NotificationService(MetaComptaDbContext db) => _db = db;

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int count = 20)
    {
        var query = _db.Notifications.Where(n => n.UserId == userId && !n.IsDeleted);
        if (unreadOnly) query = query.Where(n => !n.IsRead);

        var items = await query.OrderByDescending(n => n.CreatedAt).Take(count)
            .Select(n => new NotificationDto { Id = n.Id, Titre = n.Titre, Message = n.Message, Type = n.Type, Module = n.Module, Url = n.Url, IsRead = n.IsRead, CreatedAt = n.CreatedAt })
            .ToListAsync();
        return ApiResponse<List<NotificationDto>>.Ok(items);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
    {
        var count = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(int id, string userId)
    {
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif == null) return ApiResponse<bool>.Fail("Notification non trouvée.");
        notif.IsRead = true;
        notif.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId)
    {
        var notifs = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        notifs.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task CreateAsync(string userId, string titre, string message, string type = "Info", string? module = null, string? url = null, int? cabinetId = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Titre = titre,
            Message = message,
            Type = type,
            Module = module,
            Url = url,
            CabinetId = cabinetId ?? 0
        });
        await _db.SaveChangesAsync();
    }
}

public class ReferenceDataService : IReferenceDataService
{
    private readonly MetaComptaDbContext _db;
    public ReferenceDataService(MetaComptaDbContext db) => _db = db;

    public async Task<ApiResponse<List<Domain.Common.ReferenceData>>> GetByCategoryAsync(string category, int? cabinetId = null)
    {
        var items = await _db.ReferenceData
            .Where(r => r.Category == category && r.IsActive && (r.CabinetId == null || r.CabinetId == cabinetId))
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Code)
            .ToListAsync();
        return ApiResponse<List<Domain.Common.ReferenceData>>.Ok(items);
    }

    public async Task<ApiResponse<Domain.Common.ReferenceData>> CreateOrUpdateAsync(Domain.Common.ReferenceData data)
    {
        var existing = await _db.ReferenceData.FirstOrDefaultAsync(r => r.Category == data.Category && r.Code == data.Code);
        if (existing != null)
        {
            existing.Label = data.Label;
            existing.Value = data.Value;
            existing.NumericValue = data.NumericValue;
            existing.IsActive = data.IsActive;
            existing.EffectiveFrom = data.EffectiveFrom;
            existing.EffectiveTo = data.EffectiveTo;
            existing.Metadata = data.Metadata;
        }
        else
        {
            _db.ReferenceData.Add(data);
        }
        await _db.SaveChangesAsync();
        return ApiResponse<Domain.Common.ReferenceData>.Ok(existing ?? data);
    }

    public async Task<decimal> GetNumericValueAsync(string category, string code)
    {
        var item = await _db.ReferenceData.FirstOrDefaultAsync(r => r.Category == category && r.Code == code && r.IsActive);
        return item?.NumericValue ?? 0;
    }
}
