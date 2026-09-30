namespace MetaCompta.BackOffice.Helpers;

/// <summary>
/// BackOffice workspace modes: Cabinet + métier spaces after client selection.
/// Legacy "Client" maps to Fiscal.
/// </summary>
public static class WorkspaceModes
{
    public const string Cabinet = "Cabinet";
    public const string Fiscal = "Fiscal";
    public const string Erp = "Erp";
    public const string Paie = "Paie";
    public const string Juridique = "Juridique";

    /// <summary>Legacy dossier mode — treat as Fiscal.</summary>
    public const string LegacyClient = "Client";

    public static string Normalize(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return Cabinet;
        if (string.Equals(mode, LegacyClient, StringComparison.OrdinalIgnoreCase)) return Fiscal;
        return mode.Trim() switch
        {
            var m when m.Equals(Fiscal, StringComparison.OrdinalIgnoreCase) => Fiscal,
            var m when m.Equals(Erp, StringComparison.OrdinalIgnoreCase) => Erp,
            var m when m.Equals(Paie, StringComparison.OrdinalIgnoreCase) => Paie,
            var m when m.Equals(Juridique, StringComparison.OrdinalIgnoreCase) => Juridique,
            var m when m.Equals(Cabinet, StringComparison.OrdinalIgnoreCase) => Cabinet,
            _ => Cabinet
        };
    }

    public static bool IsMetier(string? mode)
    {
        var m = Normalize(mode);
        return m is Fiscal or Erp or Paie or Juridique;
    }

    public static string DisplayLabel(string? mode) => Normalize(mode) switch
    {
        Fiscal => "Fiscal & déclarations",
        Erp => "ERP — Tenue comptable",
        Paie => "Paie",
        Juridique => "Juridique",
        _ => "Espace cabinet"
    };

    public static string? RequiredModeForController(string controller) => controller switch
    {
        "Declarations" or "Documents" or "Tasks" or "Planning" or "Workflow" => Fiscal,
        "Comptabilite" or "EtatsFiscaux" => Erp,
        "Payroll" or "Employees" => Paie,
        "Juridique" => Juridique,
        _ => null
    };

    public static (string Controller, string Action) Landing(string mode) => Normalize(mode) switch
    {
        Erp => ("Comptabilite", "Index"),
        Paie => ("Payroll", "Index"),
        Juridique => ("Juridique", "Index"),
        Fiscal => ("Declarations", "Index"),
        _ => ("Home", "Index")
    };
}
