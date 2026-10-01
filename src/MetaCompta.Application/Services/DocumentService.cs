using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly MetaComptaDbContext _db;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(MetaComptaDbContext db, ILogger<DocumentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<DocumentDto>>> GetAllAsync(
        int cabinetId, int? clientId = null, string? categorie = null,
        int page = 1, int pageSize = 20, string? search = null,
        bool extranetVisibleOnly = false, string? extranetUserId = null)
    {
        var query = _db.Documents
            .Include(d => d.Client)
            .Include(d => d.UploadedByUser)
            .Where(d => d.CabinetId == cabinetId)
            .AsQueryable();

        if (clientId.HasValue) query = query.Where(d => d.ClientId == clientId);
        if (!string.IsNullOrEmpty(categorie)) query = query.Where(d => d.Categorie == categorie);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(d => d.Titre.Contains(search) || d.NomFichier.Contains(search));

        if (extranetVisibleOnly)
        {
            query = query.Where(d =>
                d.PartageExtranet
                || (!string.IsNullOrEmpty(extranetUserId) && d.UploadedByUserId == extranetUserId));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<DocumentDto>>.Ok(new PagedResult<DocumentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByDeclarationIdAsync(int declarationId, int cabinetId)
    {
        var docs = await _db.Documents
            .Include(d => d.Client)
            .Include(d => d.UploadedByUser)
            .Where(d => d.CabinetId == cabinetId && d.DeclarationId == declarationId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<DocumentDto>>.Ok(docs.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<DocumentDto>> GetByIdAsync(int id, int cabinetId)
    {
        var doc = await _db.Documents
            .Include(d => d.Client)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);

        return doc == null
            ? ApiResponse<DocumentDto>.Fail("Document introuvable.")
            : ApiResponse<DocumentDto>.Ok(MapToDto(doc));
    }

    public async Task<ApiResponse<DocumentDto>> UploadAsync(
        Stream fileStream, string fileName, string contentType, long size,
        string titre, string categorie, int? clientId, int cabinetId, string userId,
        int? declarationId = null, int? checklistItemId = null)
    {
        // Validate file size (max 20MB)
        if (size > 20 * 1024 * 1024)
            return ApiResponse<DocumentDto>.Fail("Le fichier ne doit pas dépasser 20 Mo.");

        // Sanitize filename
        var safeName = Path.GetFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var uploadDir = Path.Combine("uploads", cabinetId.ToString());
        Directory.CreateDirectory(uploadDir);
        var filePath = Path.Combine(uploadDir, uniqueName);

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        var doc = new Document
        {
            Titre = titre,
            Categorie = categorie,
            NomFichier = safeName,
            CheminStockage = filePath,
            ContentType = contentType,
            TailleFichier = size,
            ClientId = clientId,
            DeclarationId = declarationId,
            CabinetId = cabinetId,
            UploadedByUserId = userId,
            Statut = "EnAttente",
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        if (checklistItemId.HasValue && declarationId.HasValue)
        {
            var item = await _db.DeclarationChecklistItems.FirstOrDefaultAsync(c =>
                c.Id == checklistItemId.Value && c.DeclarationId == declarationId.Value
                && c.CabinetId == cabinetId && !c.IsDeleted);
            if (item != null)
            {
                item.IsReceived = true;
                item.DocumentId = doc.Id;
                item.ReceivedAt = DateTime.UtcNow;
                item.UpdatedBy = userId;
                item.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Document uploaded: {DocId} ({FileName}) for declaration {DeclId}",
            doc.Id, safeName, declarationId);

        return ApiResponse<DocumentDto>.Ok(MapToDto(doc));
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId, string? motif = null)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (doc == null) return ApiResponse<bool>.Fail("Document introuvable.");

        var validStatuses = new[] { "EnAttente", "Valide", "Rejete", "Archive" };
        if (!validStatuses.Contains(newStatut))
            return ApiResponse<bool>.Fail($"Statut invalide: {newStatut}");

        doc.Statut = newStatut;
        if (newStatut == "Rejete") doc.MotifRejet = motif;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> ToggleShareAsync(int id, int cabinetId, string userId)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (doc == null) return ApiResponse<bool>.Fail("Document introuvable.");

        doc.PartageExtranet = !doc.PartageExtranet;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(doc.PartageExtranet);
    }

    public async Task<ApiResponse<Stream>> DownloadAsync(int id, int cabinetId)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (doc == null) return ApiResponse<Stream>.Fail("Document introuvable.");

        if (!File.Exists(doc.CheminStockage))
            return ApiResponse<Stream>.Fail("Le fichier physique est introuvable.");

        var stream = new FileStream(doc.CheminStockage, FileMode.Open, FileAccess.Read);
        return ApiResponse<Stream>.Ok(stream);
    }

    public async Task<int?> GetDeclarationClientIdAsync(int declarationId, int cabinetId)
    {
        var decl = await _db.Set<Domain.Entities.Declaration>()
            .Where(d => d.Id == declarationId && d.CabinetId == cabinetId)
            .Select(d => (int?)d.ClientId)
            .FirstOrDefaultAsync();
        return decl;
    }

    private static DocumentDto MapToDto(Document d) => new()
    {
        Id = d.Id,
        Titre = d.Titre,
        Nom = d.NomFichier,
        NomFichier = d.NomFichier,
        Description = d.Description,
        Categorie = d.Categorie,
        TypeDocument = d.ContentType,
        TailleFichier = FormatSize(d.TailleFichier),
        DateDocument = d.DateDocument,
        DateUpload = d.CreatedAt,
        ClientId = d.ClientId,
        ClientNom = d.Client?.RaisonSociale,
        Statut = d.Statut,
        EstPartage = d.PartageExtranet,
        Version = d.Version,
        UploadePar = d.UploadedByUser != null ? $"{d.UploadedByUser.FirstName} {d.UploadedByUser.LastName}" : null,
        CreatedAt = d.CreatedAt,
        DeclarationId = d.DeclarationId,
        ContentType = d.ContentType
    };

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} o",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} Ko",
        _ => $"{bytes / (1024.0 * 1024.0):F1} Mo"
    };
}
