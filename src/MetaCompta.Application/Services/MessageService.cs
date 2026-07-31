using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class MessageService : IMessageService
{
    private readonly MetaComptaDbContext _db;

    public MessageService(MetaComptaDbContext db) => _db = db;

    public async Task<ApiResponse<List<MessageDto>>> GetByDeclarationAsync(int declarationId, int cabinetId, int? clientId = null)
    {
        var query = _db.Messages
            .Include(m => m.Client)
            .Where(m => m.CabinetId == cabinetId && m.DeclarationId == declarationId && !m.IsDeleted);

        if (clientId.HasValue)
            query = query.Where(m => m.ClientId == clientId.Value);

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();
        return ApiResponse<List<MessageDto>>.Ok(messages);
    }

    public async Task<ApiResponse<List<MessageDto>>> GetByClientAsync(int clientId, int cabinetId)
    {
        var messages = await _db.Messages
            .Include(m => m.Client)
            .Where(m => m.CabinetId == cabinetId && m.ClientId == clientId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();
        return ApiResponse<List<MessageDto>>.Ok(messages);
    }

    public async Task<ApiResponse<List<MessageDto>>> GetConversationsAsync(int cabinetId, string? userId = null, int? clientId = null)
    {
        var query = _db.Messages
            .Include(m => m.Client)
            .Where(m => m.CabinetId == cabinetId && !m.IsDeleted && m.ParentMessageId == null);

        // Security: filter by clientId for Extranet client users
        if (clientId.HasValue)
            query = query.Where(m => m.ClientId == clientId.Value);
        else if (!string.IsNullOrEmpty(userId))
            query = query.Where(m => m.SenderId == userId || m.Client != null);

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(50)
            .Select(m => MapToDto(m))
            .ToListAsync();
        return ApiResponse<List<MessageDto>>.Ok(messages);
    }

    public async Task<ApiResponse<MessageDto>> SendAsync(SendMessageDto dto, int cabinetId, string userId, string userName, string senderRole)
    {
        var message = new Domain.Entities.Message
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            DeclarationId = dto.DeclarationId,
            SenderId = userId,
            SenderName = userName,
            SenderRole = senderRole,
            Sujet = dto.Sujet,
            Contenu = dto.Contenu,
            ParentMessageId = dto.ParentMessageId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return ApiResponse<MessageDto>.Ok(new MessageDto
        {
            Id = message.Id,
            ClientId = message.ClientId,
            DeclarationId = message.DeclarationId,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Sujet = message.Sujet,
            Contenu = message.Contenu,
            CreatedAt = message.CreatedAt,
            ParentMessageId = message.ParentMessageId
        });
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(int id, string userId)
    {
        var msg = await _db.Messages.FindAsync(id);
        if (msg == null) return ApiResponse<bool>.Fail("Message introuvable.");
        if (msg.SenderId == userId) return ApiResponse<bool>.Ok(true); // can't mark own as read

        msg.IsReadByRecipient = true;
        msg.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(int cabinetId, string? userId = null, int? clientId = null)
    {
        var query = _db.Messages.Where(m => m.CabinetId == cabinetId && !m.IsDeleted && !m.IsReadByRecipient);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(m => m.SenderId != userId); // messages not sent by me
        if (clientId.HasValue)
            query = query.Where(m => m.ClientId == clientId);

        var count = await query.CountAsync();
        return ApiResponse<int>.Ok(count);
    }

    private static MessageDto MapToDto(Domain.Entities.Message m) => new()
    {
        Id = m.Id,
        ClientId = m.ClientId,
        ClientName = m.Client?.RaisonSociale,
        DeclarationId = m.DeclarationId,
        SenderId = m.SenderId,
        SenderName = m.SenderName,
        SenderRole = m.SenderRole,
        Sujet = m.Sujet,
        Contenu = m.Contenu,
        IsReadByRecipient = m.IsReadByRecipient,
        CreatedAt = m.CreatedAt,
        ParentMessageId = m.ParentMessageId
    };
}
