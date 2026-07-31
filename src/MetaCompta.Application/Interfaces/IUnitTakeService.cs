using MetaCompta.Application.DTOs;

namespace MetaCompta.Application.Interfaces;

public interface IUnitTakeService
{
    /// <summary>Open an exclusive take. Fails if another open take exists (unless forceRelease).</summary>
    Task<ApiResponse<UnitTakeDto>> OpenTakeAsync(OpenUnitTakeDto dto, int cabinetId, string userId);

    /// <summary>Close the caller's open take (or force-close if manager).</summary>
    Task<ApiResponse<UnitTakeDto>> CloseTakeAsync(CloseUnitTakeDto dto, int cabinetId, string userId);

    Task<ApiResponse<UnitTakeDto?>> GetOpenTakeAsync(string unitTypeCode, int unitEntityId, int cabinetId);

    Task<ApiResponse<List<UnitTypeDto>>> GetUnitTypesAsync();
}
