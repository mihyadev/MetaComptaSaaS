namespace MetaCompta.Domain.Common;

/// <summary>
/// Maps contract TypeService codes → workflow declaration types and resolves
/// dossier-team assignees by métier RoleLabel (Fiscaliste, Comptable, …).
/// </summary>
public static class ContratWorkflowAutoAssign
{
    public static string? MapDeclarationType(string? typeService)
    {
        if (string.IsNullOrWhiteSpace(typeService)) return null;
        return typeService.Trim().ToUpperInvariant() switch
        {
            "TVA" => "TVA",
            "ACOMPTES" => "ACOMPTES",
            "REG_IS" => "REG_IS",
            "DRF" => "DRF",
            "CM" => "CM",
            "IR_SAL" => "IR_SAL",
            "RAS_FONC" => "RAS_FONC",
            "RAS_DIV" => "RAS_DIV",
            "IR" => "IR",
            "CNSS" => "CNSS",
            "DTS" => "DTS",
            "PAIE" => "Paie",
            "DOMICILIATION" => "Domiciliation",
            "COMPTABILITE" => "Comptabilite",
            "PV_AGO" or "DEPOT_RC" => "Juridique",
            _ => null
        };
    }

    public static IReadOnlyList<string> MetierRoleHints(string typeService)
    {
        var code = typeService.Trim().ToUpperInvariant();
        return code switch
        {
            "TVA" or "ACOMPTES" or "REG_IS" or "DRF" or "CM"
                or "IR_SAL" or "RAS_FONC" or "RAS_DIV" or "IR" or "EG" or "DT" or "DEI_TP"
                => new[] { "Fiscaliste", "Manager", "Chef de mission", "Collaborateur" },

            "CNSS" or "DTS" or "PAIE" or "DCH_ETS"
                => new[] { "Social", "Collaborateur", "Manager", "Chef de mission" },

            "PV_AGO" or "DEPOT_RC" or "DOMICILIATION"
                => new[] { "Juridique", "Collaborateur", "Manager", "Chef de mission" },

            "COMPTABILITE" or "TRESORERIE" or "BILAN" or "ETATSFISCAUX" or "LIASSEFISCALE"
                => new[] { "Comptable", "Manager", "Chef de mission", "Collaborateur" },

            _ => new[] { "Collaborateur", "Manager", "Chef de mission", "Fiscaliste", "Comptable" }
        };
    }

    public static bool RoleLabelMatchesHint(string roleLabel, string hint)
    {
        if (roleLabel.Contains(hint, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(hint, "Manager", StringComparison.OrdinalIgnoreCase)
            && roleLabel.Contains("Chef de mission", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(hint, "Chef de mission", StringComparison.OrdinalIgnoreCase)
            && roleLabel.Contains("Manager", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static string? ResolveAssignee(
        IReadOnlyList<(string UserId, string? RoleLabel)> membres,
        string typeService)
    {
        if (membres.Count == 0) return null;

        foreach (var hint in MetierRoleHints(typeService))
        {
            var hit = membres.FirstOrDefault(m =>
                !string.IsNullOrWhiteSpace(m.RoleLabel)
                && RoleLabelMatchesHint(m.RoleLabel!, hint));
            if (!string.IsNullOrEmpty(hit.UserId))
                return hit.UserId;
        }

        var lead = membres.FirstOrDefault(m =>
            !string.IsNullOrWhiteSpace(m.RoleLabel)
            && (RoleLabelMatchesHint(m.RoleLabel!, "Manager")
                || RoleLabelMatchesHint(m.RoleLabel!, "Chef de mission")));
        if (!string.IsNullOrEmpty(lead.UserId))
            return lead.UserId;

        return membres[0].UserId;
    }

    public static string? RoleOfMember(
        IReadOnlyList<(string UserId, string? RoleLabel)> membres,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return membres.FirstOrDefault(m => m.UserId == userId).RoleLabel;
    }
}
