using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Common;

namespace MetaCompta.Application.Interfaces;

/// <summary>
/// Per-declaration-type lifecycle plug-in (create + edit routing).
/// Calcul-specific save stays on IDeclarationService for typed types.
/// </summary>
public interface IDeclarationTypeHandler
{
    string Code { get; }
    string EditAction { get; }
    bool NeedsAnneeFiscale { get; }
    Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId);
}

public interface IDeclarationTypeHandlerRegistry
{
    IDeclarationTypeHandler? Get(string? type);
    IReadOnlyList<IDeclarationTypeHandler> All { get; }
    string GetEditAction(string? type);
}

public sealed class DeclarationTypeHandlerRegistry : IDeclarationTypeHandlerRegistry
{
    private readonly Dictionary<string, IDeclarationTypeHandler> _map;

    public DeclarationTypeHandlerRegistry(IEnumerable<IDeclarationTypeHandler> handlers)
    {
        _map = handlers.ToDictionary(h => h.Code, h => h, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IDeclarationTypeHandler> All => _map.Values.ToList();

    public IDeclarationTypeHandler? Get(string? type)
    {
        var code = DeclarationServiceCatalog.NormalizeType(type);
        return _map.TryGetValue(code, out var h) ? h : null;
    }

    public string GetEditAction(string? type)
        => Get(type)?.EditAction ?? DeclarationServiceCatalog.GetEditAction(type);
}
