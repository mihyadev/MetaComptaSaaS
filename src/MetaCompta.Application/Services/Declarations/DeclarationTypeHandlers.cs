using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;

namespace MetaCompta.Application.Services.Declarations;

public sealed class TvaDeclarationHandler : IDeclarationTypeHandler
{
    private readonly IDeclarationService _svc;
    public TvaDeclarationHandler(IDeclarationService svc) => _svc = svc;
    public string Code => "TVA";
    public string EditAction => "EditTVA";
    public bool NeedsAnneeFiscale => false;
    public Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
        => _svc.CreateTVAAsync(dto.ClientId, cabinetId, userId);
}

public sealed class IsDeclarationHandler : IDeclarationTypeHandler
{
    private readonly IDeclarationService _svc;
    public IsDeclarationHandler(IDeclarationService svc) => _svc = svc;
    public string Code => "IS";
    public string EditAction => "EditIS";
    public bool NeedsAnneeFiscale => true;
    public Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
        => _svc.CreateISAsync(dto.ClientId, dto.AnneeFiscale ?? DateTime.UtcNow.Year, cabinetId, userId);
}

public sealed class IrDeclarationHandler : IDeclarationTypeHandler
{
    private readonly IDeclarationService _svc;
    public IrDeclarationHandler(IDeclarationService svc) => _svc = svc;
    public string Code => "IR";
    public string EditAction => "EditIR";
    public bool NeedsAnneeFiscale => true;
    public Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
        => _svc.CreateIRAsync(dto.ClientId, dto.AnneeFiscale ?? DateTime.UtcNow.Year, cabinetId, userId);
}

public sealed class CnssDeclarationHandler : IDeclarationTypeHandler
{
    private readonly IDeclarationService _svc;
    public CnssDeclarationHandler(IDeclarationService svc) => _svc = svc;
    public string Code => "CNSS";
    public string EditAction => "EditCNSS";
    public bool NeedsAnneeFiscale => false;
    public Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
        => _svc.CreateCNSSAsync(dto.ClientId, cabinetId, userId);
}

/// <summary>Shell handler for catalogue types (ACOMPTES, REG_IS, IR_SAL, DRF, …).</summary>
public sealed class ShellDeclarationHandler : IDeclarationTypeHandler
{
    private readonly IDeclarationService _svc;
    private readonly string _code;
    private readonly bool _needsAnnee;

    public ShellDeclarationHandler(IDeclarationService svc, string code, bool needsAnneeFiscale)
    {
        _svc = svc;
        _code = code;
        _needsAnnee = needsAnneeFiscale;
    }

    public string Code => _code;
    public string EditAction => "EditGeneric";
    public bool NeedsAnneeFiscale => _needsAnnee;

    public Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId)
    {
        dto.Type = _code;
        if (_needsAnnee && !dto.AnneeFiscale.HasValue)
            dto.AnneeFiscale = DateTime.UtcNow.Year;
        return _svc.CreateShellAsync(dto, cabinetId, userId);
    }
}
