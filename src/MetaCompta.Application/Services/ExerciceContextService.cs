using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class ExerciceContextService : IExerciceContextService
{
    public const string NoOpenExerciceMessage =
        "Aucun exercice comptable ouvert pour ce client. Créez un exercice (permission comptabilité) avant toute opération (déclarations, paie, écritures).";

    public const string ClosedExerciceMessage =
        "L'exercice sélectionné est clôturé. Aucune création ou modification n'est autorisée.";

    private readonly MetaComptaDbContext _db;

    public ExerciceContextService(MetaComptaDbContext db) => _db = db;

    public async Task<ExerciceComptable?> GetOpenExerciceAsync(int clientId, int cabinetId)
    {
        return await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId
                        && e.Statut == "Ouvert")
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();
    }

    public async Task<ApiResponse<ExerciceComptableDto>> RequireOpenExerciceAsync(
        int clientId, int cabinetId, int? preferredExerciceId = null)
    {
        ExerciceComptable? exo = null;
        if (preferredExerciceId is > 0)
        {
            exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == preferredExerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);
            if (exo == null)
                return ApiResponse<ExerciceComptableDto>.Fail("Exercice introuvable pour ce client.");
            if (!string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
                return ApiResponse<ExerciceComptableDto>.Fail(ClosedExerciceMessage);
        }
        else
        {
            exo = await GetOpenExerciceAsync(clientId, cabinetId);
            if (exo == null)
                return ApiResponse<ExerciceComptableDto>.Fail(NoOpenExerciceMessage);
        }

        return ApiResponse<ExerciceComptableDto>.Ok(new ExerciceComptableDto
        {
            Id = exo.Id,
            ClientId = exo.ClientId,
            Annee = exo.Annee,
            DateDebut = exo.DateDebut,
            DateFin = exo.DateFin,
            Statut = exo.Statut
        });
    }

    public async Task<bool> HasOpenExerciceAsync(int clientId, int cabinetId)
        => await _db.ExercicesComptables.AnyAsync(e =>
            e.ClientId == clientId && e.CabinetId == cabinetId && e.Statut == "Ouvert");

    public static bool IsOpen(string? statut)
        => string.Equals(statut, "Ouvert", StringComparison.OrdinalIgnoreCase);
}
