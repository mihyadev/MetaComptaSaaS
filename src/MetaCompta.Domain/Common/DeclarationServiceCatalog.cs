namespace MetaCompta.Domain.Common;

/// <summary>
/// Single source of truth: contract service code ↔ declaration type ↔ claim domain ↔ edit/list/create routes.
/// Used by Index HasService, Create, Workflow auto-create, and menus.
/// </summary>
public static class DeclarationServiceCatalog
{
    public sealed record Entry(
        string DeclarationType,
        string DisplayName,
        string ClaimDomain,
        string EditAction,
        string Frequency,
        string[] ContractServiceCodes,
        string ListAction,
        string CreateAction,
        bool NeedsAnneeFiscale = false,
        bool IsShellOnly = false);

    /// <summary>All fiscal/social declaration types supported by the platform shell.</summary>
    public static readonly IReadOnlyList<Entry> All = new List<Entry>
    {
        new("TVA", "TVA", "tva", "EditTVA", "Mensuelle", new[] { "TVA" }, "TVA", "CreateTVA"),
        new("IS", "IS — Liasse", "is", "EditIS", "Annuelle", new[] { "IS", "DRF", "LIASSEFISCALE" }, "IS", "CreateIS", NeedsAnneeFiscale: true),
        new("ACOMPTES", "Acomptes IS", "is", "EditGeneric", "Trimestrielle", new[] { "ACOMPTES", "IS" }, "Acomptes", "CreateAcomptes", IsShellOnly: true),
        new("REG_IS", "Régularisation IS", "is", "EditGeneric", "Annuelle", new[] { "REG_IS", "IS" }, "RegIs", "CreateRegIs", NeedsAnneeFiscale: true, IsShellOnly: true),
        new("CM", "Cotisation Minimale", "is", "EditGeneric", "Annuelle", new[] { "CM", "IS" }, "CM", "CreateCM", NeedsAnneeFiscale: true, IsShellOnly: true),
        new("DRF", "D.R.F — Résultat Fiscal", "is", "EditGeneric", "Annuelle", new[] { "DRF", "LIASSEFISCALE", "IS" }, "DRF", "CreateDRF", NeedsAnneeFiscale: true, IsShellOnly: true),
        new("IR", "IR — Revenu", "ir", "EditIR", "Annuelle", new[] { "IR", "IR_SAL", "RAS_FONC", "RAS_DIV" }, "IR", "CreateIR", NeedsAnneeFiscale: true),
        new("IR_SAL", "IR / Salaires", "ir", "EditGeneric", "Mensuelle", new[] { "IR_SAL", "IR" }, "IrSalaires", "CreateIrSalaires", IsShellOnly: true),
        new("RAS_FONC", "RAS Foncier", "ir", "EditGeneric", "Ponctuelle", new[] { "RAS_FONC", "IR" }, "RasFoncier", "CreateRasFoncier", IsShellOnly: true),
        new("RAS_DIV", "RAS Dividendes", "ir", "EditGeneric", "Ponctuelle", new[] { "RAS_DIV", "IR" }, "RasDividendes", "CreateRasDividendes", IsShellOnly: true),
        new("CNSS", "CNSS / DMS", "cnss", "EditCNSS", "Mensuelle", new[] { "CNSS" }, "CNSS", "CreateCNSS"),
        // DTS is a separate contrat service — do not unlock it when only CNSS is active
        new("DTS", "D.T.S / État 9421", "cnss", "EditGeneric", "Annuelle", new[] { "DTS" }, "Dts", "CreateDts", NeedsAnneeFiscale: true, IsShellOnly: true),
    };

    private static readonly Dictionary<string, Entry> ByType =
        All.ToDictionary(e => e.DeclarationType, e => e, StringComparer.OrdinalIgnoreCase);

    public static Entry? Get(string? declarationType)
        => string.IsNullOrWhiteSpace(declarationType) ? null
            : ByType.TryGetValue(declarationType.Trim(), out var e) ? e : null;

    public static bool IsSupported(string? declarationType) => Get(declarationType) != null;

    public static string GetEditAction(string? declarationType)
        => Get(declarationType)?.EditAction ?? "Details";

    public static string GetListAction(string? declarationType)
        => Get(declarationType)?.ListAction ?? "Index";

    public static string GetCreateAction(string? declarationType)
        => Get(declarationType)?.CreateAction ?? "Create";

    public static string GetClaimDomain(string? declarationType)
        => Get(declarationType)?.ClaimDomain ?? "declaration";

    /// <summary>True when ActiveServices ∩ ContractServiceCodes is non-empty.</summary>
    public static bool HasContractService(string declarationType, IEnumerable<string> activeServices)
    {
        var entry = Get(declarationType);
        if (entry == null) return false;
        var set = new HashSet<string>(activeServices, StringComparer.OrdinalIgnoreCase);
        return entry.ContractServiceCodes.Any(set.Contains);
    }

    /// <summary>Normalize contract / WF type aliases to canonical declaration type.</summary>
    public static string NormalizeType(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "TVA";
        var t = raw.Trim().ToUpperInvariant();
        return t switch
        {
            "IR_SALAIRE" or "IR_SALAIRES" or "IRSAL" => "IR_SAL",
            "REGUL_IS" or "REGULARISATION" or "REGULIS" => "REG_IS",
            "ACOMPTE" or "ACOMPTE_IS" => "ACOMPTES",
            "9421" or "ETAT9421" => "DTS",
            "LIASSE" or "LIASSEFISCALE" => "DRF",
            _ => t
        };
    }

    public static IEnumerable<Entry> ForMenu(IEnumerable<string> activeServices)
        => All.Where(e => HasContractService(e.DeclarationType, activeServices));
}
