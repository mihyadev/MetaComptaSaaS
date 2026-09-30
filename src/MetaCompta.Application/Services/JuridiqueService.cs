using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class JuridiqueService : IJuridiqueService
{
    private readonly MetaComptaDbContext _db;

    public JuridiqueService(MetaComptaDbContext db) => _db = db;

    public async Task<ApiResponse<PagedResult<DossierJuridiqueDto>>> GetAllAsync(
        int cabinetId, int? clientId, string? type, string? statut, int page, string? search)
    {
        page = Math.Max(1, page);
        const int pageSize = 20;
        var q = _db.DossiersJuridiques.AsNoTracking()
            .Include(d => d.Client)
            .Where(d => d.CabinetId == cabinetId && !d.IsDeleted);

        if (clientId is > 0) q = q.Where(d => d.ClientId == clientId);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(d => d.TypeDossier == type);
        if (!string.IsNullOrWhiteSpace(statut)) q = q.Where(d => d.Statut == statut);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(d => d.Objet.Contains(s) || d.Reference.Contains(s)
                || (d.Notes != null && d.Notes.Contains(s)));
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(d => d.DateEcheance ?? d.DateOuverture)
            .ThenByDescending(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => Map(d))
            .ToListAsync();

        return ApiResponse<PagedResult<DossierJuridiqueDto>>.Ok(new PagedResult<DossierJuridiqueDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<DossierJuridiqueDto>> GetByIdAsync(int id, int cabinetId)
    {
        var d = await _db.DossiersJuridiques.AsNoTracking()
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == id && x.CabinetId == cabinetId && !x.IsDeleted);
        if (d == null) return ApiResponse<DossierJuridiqueDto>.Fail("Dossier introuvable.");
        return ApiResponse<DossierJuridiqueDto>.Ok(Map(d));
    }

    public async Task<ApiResponse<DossierJuridiqueDto>> CreateAsync(DossierJuridiqueCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0) return ApiResponse<DossierJuridiqueDto>.Fail("Client obligatoire.");
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (client == null) return ApiResponse<DossierJuridiqueDto>.Fail("Client introuvable.");

        var type = (dto.TypeDossier ?? dto.Type ?? "").Trim();
        if (string.IsNullOrEmpty(type)) type = "PVAG";
        var year = DateTime.UtcNow.Year;
        var seq = await _db.DossiersJuridiques.CountAsync(d => d.CabinetId == cabinetId && d.ClientId == dto.ClientId) + 1;
        var entity = new DossierJuridique
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            TypeDossier = type,
            Reference = $"{type}-{year}-{seq:D4}",
            Objet = string.IsNullOrWhiteSpace(dto.Objet) ? dto.Titre ?? type : dto.Objet.Trim(),
            Description = dto.Description,
            Responsable = dto.Responsable,
            Montant = dto.Montant,
            DateEcheance = dto.DateEcheance,
            Notes = dto.Notes,
            Statut = "EnCours",
            DateOuverture = DateTime.UtcNow,
            ChecklistJson = DefaultChecklist(type),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.DossiersJuridiques.Add(entity);
        await _db.SaveChangesAsync();
        entity.Client = client;
        return ApiResponse<DossierJuridiqueDto>.Ok(Map(entity), "Dossier juridique créé.");
    }

    public async Task<ApiResponse<DossierJuridiqueDto>> UpdateAsync(int id, DossierJuridiqueCreateDto dto, int cabinetId, string userId)
    {
        var entity = await _db.DossiersJuridiques
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId && !d.IsDeleted);
        if (entity == null) return ApiResponse<DossierJuridiqueDto>.Fail("Dossier introuvable.");

        if (!string.IsNullOrWhiteSpace(dto.TypeDossier ?? dto.Type))
            entity.TypeDossier = (dto.TypeDossier ?? dto.Type)!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Objet) || !string.IsNullOrWhiteSpace(dto.Titre))
            entity.Objet = (dto.Objet ?? dto.Titre)!.Trim();
        entity.Description = dto.Description;
        entity.Responsable = dto.Responsable;
        entity.Montant = dto.Montant;
        entity.DateEcheance = dto.DateEcheance;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<DossierJuridiqueDto>.Ok(Map(entity), "Dossier mis à jour.");
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, string statut, int cabinetId, string userId)
    {
        var entity = await _db.DossiersJuridiques
            .FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId && !d.IsDeleted);
        if (entity == null) return ApiResponse<bool>.Fail("Dossier introuvable.");
        var s = (statut ?? "").Trim();
        if (string.IsNullOrEmpty(s)) return ApiResponse<bool>.Fail("Statut requis.");
        entity.Statut = s;
        if (s is "Cloture" or "Archive")
            entity.DateCloture ??= DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Statut mis à jour.");
    }

    private static string DefaultChecklist(string type) => type.ToUpperInvariant() switch
    {
        "PVAG" or "PV_AGO" => "[{\"label\":\"Convocation AG\",\"done\":false},{\"label\":\"PV signé\",\"done\":false},{\"label\":\"Rapport gérance\",\"done\":false}]",
        "DEPOTLEGAL" or "DEPOT_RC" => "[{\"label\":\"Comptes annuels\",\"done\":false},{\"label\":\"Dépôt greffe\",\"done\":false},{\"label\":\"Accusé réception\",\"done\":false}]",
        "RC" => "[{\"label\":\"Modèle 1\",\"done\":false},{\"label\":\"Publication\",\"done\":false}]",
        _ => "[]"
    };

    private static DossierJuridiqueDto Map(DossierJuridique d) => new()
    {
        Id = d.Id,
        ClientId = d.ClientId,
        ClientNom = d.Client?.RaisonSociale,
        TypeDossier = d.TypeDossier,
        Reference = d.Reference,
        Objet = d.Objet,
        Description = d.Description,
        Statut = d.Statut,
        DateOuverture = d.DateOuverture,
        DateCloture = d.DateCloture,
        DateDepot = d.DateDepot,
        DateEcheance = d.DateEcheance,
        Responsable = d.Responsable,
        Montant = d.Montant,
        Notes = d.Notes
    };
}
