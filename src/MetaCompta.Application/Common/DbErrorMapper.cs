using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace MetaCompta.Application.Common;

/// <summary>
/// Maps EF / SQL errors to user-actionable French messages (field-oriented when possible).
/// </summary>
public static class DbErrorMapper
{
    public sealed record MappedError(string Message, List<string> Errors);

    public static MappedError Map(DbUpdateException ex)
    {
        var sql = ex.InnerException as SqlException
                  ?? ex.GetBaseException() as SqlException;
        var raw = (sql?.Message ?? ex.InnerException?.Message ?? ex.Message) ?? "";
        var errors = new List<string>();

        // Unique constraint
        if (sql?.Number is 2601 or 2627 || ContainsAny(raw, "UNIQUE", "duplicate key", "Cannot insert duplicate"))
        {
            var field = DetectUniqueField(raw);
            var label = FieldLabel(field);
            var msg = $"{label} est déjà utilisé par un autre enregistrement. Choisissez une autre valeur.";
            errors.Add(field == null ? msg : $"{field}: {msg}");
            return new MappedError($"Conflit : {label} déjà utilisé.", errors);
        }

        // Foreign key
        if (sql?.Number is 547 || ContainsAny(raw, "FOREIGN KEY", "REFERENCE constraint", "conflicted with the FOREIGN KEY"))
        {
            var field = DetectFkField(raw);
            var label = FieldLabel(field);
            var msg = $"{label} est invalide ou introuvable. Vérifiez la valeur saisie.";
            errors.Add(field == null ? msg : $"{field}: {msg}");
            return new MappedError($"Référence invalide : {label}.", errors);
        }

        // NOT NULL
        if (sql?.Number is 515 || ContainsAny(raw, "Cannot insert the value NULL", "does not allow nulls"))
        {
            var field = DetectNullField(raw);
            var label = FieldLabel(field);
            var msg = $"{label} est obligatoire.";
            errors.Add(field == null ? msg : $"{field}: {msg}");
            return new MappedError($"Champ obligatoire manquant : {label}.", errors);
        }

        // String truncate / length
        if (ContainsAny(raw, "truncated", "String or binary data would be truncated"))
        {
            errors.Add("Une valeur est trop longue pour le champ. Raccourcissez le texte saisi.");
            return new MappedError("Valeur trop longue.", errors);
        }

        errors.Add(Truncate(raw, 220));
        return new MappedError("Enregistrement impossible. Vérifiez les données saisies.", errors);
    }

    private static string? DetectUniqueField(string raw)
    {
        foreach (var key in new[] { "ICE", "IF", "RC", "NumeroCNSS", "Email", "UserName", "CIN" })
        {
            if (raw.Contains(key, StringComparison.OrdinalIgnoreCase))
                return key is "IF" ? "IF" : key;
        }
        if (raw.Contains("IX_Clients_CabinetId_ICE", StringComparison.OrdinalIgnoreCase)) return "ICE";
        if (raw.Contains("IX_Clients_CabinetId_IF", StringComparison.OrdinalIgnoreCase)) return "IF";
        if (raw.Contains("IX_Clients_CabinetId_RC", StringComparison.OrdinalIgnoreCase)) return "RC";
        if (raw.Contains("IX_Clients_CabinetId_NumeroCNSS", StringComparison.OrdinalIgnoreCase)) return "NumeroCNSS";
        return null;
    }

    private static string? DetectFkField(string raw)
    {
        if (raw.Contains("ManagerPrincipal", StringComparison.OrdinalIgnoreCase)) return "ManagerPrincipalId";
        if (raw.Contains("ClientId", StringComparison.OrdinalIgnoreCase)) return "ClientId";
        if (raw.Contains("Responsable", StringComparison.OrdinalIgnoreCase)) return "ResponsableId";
        return null;
    }

    private static string? DetectNullField(string raw)
    {
        foreach (var key in new[] { "RaisonSociale", "Nom", "Prenom", "Email", "CIN", "Libelle", "TypeService" })
        {
            if (raw.Contains($"column '{key}'", StringComparison.OrdinalIgnoreCase)
                || raw.Contains($"'{key}'", StringComparison.OrdinalIgnoreCase))
                return key;
        }
        return null;
    }

    private static string FieldLabel(string? field) => field switch
    {
        "ICE" => "L'ICE",
        "IF" => "L'IF",
        "RC" => "Le RC",
        "NumeroCNSS" => "Le n° CNSS",
        "ManagerPrincipalId" => "Le manager principal",
        "RaisonSociale" => "La raison sociale",
        "ClientId" => "Le client",
        "Email" => "L'e-mail",
        "CIN" => "Le CIN",
        null => "Ce champ",
        _ => field
    };

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(n => text.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static string Truncate(string text, int max)
        => text.Length <= max ? text : text[..max] + "…";
}
