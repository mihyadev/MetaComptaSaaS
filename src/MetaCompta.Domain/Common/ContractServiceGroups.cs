namespace MetaCompta.Domain.Common;

/// <summary>
/// Métier groups for commercial contract service selection.
/// Selecting a group attaches all <see cref="ContractServiceGroup.AttachedTypeServiceCodes"/>;
/// <see cref="ContractServiceGroup.InfoItems"/> are display-only official catalogs (DGI / CNSS / OMPIC).
/// </summary>
public static class ContractServiceGroups
{
    public sealed record InfoItem(int Number, string Label, string Periodicite);

    public sealed record ContractServiceGroup(
        string GroupId,
        string Title,
        string ShortTitle,
        string ReferenceNote,
        string Color,
        IReadOnlyList<string> AttachedTypeServiceCodes,
        IReadOnlyList<InfoItem> InfoItems);

    public const string Fiscal = "Fiscal";
    public const string Social = "Social";
    public const string Juridique = "Juridique";
    public const string Comptable = "Comptable";
    public const string Domiciliation = "Domiciliation";

    public static readonly IReadOnlyList<ContractServiceGroup> All = new List<ContractServiceGroup>
    {
        new(
            Fiscal,
            "Déclarations fiscales — DGI",
            "Fiscal",
            "Référence : DGI Maroc — CGI 2026.",
            "primary",
            new[]
            {
                "TVA", "ACOMPTES", "REG_IS", "DRF", "CM",
                "IR_SAL", "RAS_FONC", "RAS_DIV", "IR",
                "EG", "DT", "DEI_TP"
            },
            new[]
            {
                new InfoItem(1, "Déclaration de TVA mensuelle", "Mensuelle"),
                new InfoItem(2, "Déclaration de TVA trimestrielle", "Trimestrielle"),
                new InfoItem(3, "Déclaration du résultat fiscal", "Annuelle"),
                new InfoItem(4, "Déclaration de l'impôt sur les sociétés (IS)", "Annuelle"),
                new InfoItem(5, "Déclaration des acomptes provisionnels IS", "Trimestrielle"),
                new InfoItem(6, "Déclaration de l'IR professionnel", "Annuelle, selon le régime"),
                new InfoItem(7, "Déclaration de l'IR retenu à la source sur les salaires", "Selon l'échéance fiscale"),
                new InfoItem(8, "Déclaration annuelle des traitements et salaires", "Annuelle"),
                new InfoItem(9, "Déclarations de retenue à la source sur les dividendes, loyers et prestations", "Selon les opérations"),
                new InfoItem(10, "Déclaration des revenus fonciers et profits fonciers", "Selon la situation"),
                new InfoItem(11, "Déclaration des factures impayées dans les délais légaux", "Selon les conditions applicables"),
                new InfoItem(12, "Déclaration des droits d'enregistrement et de timbre", "Selon les opérations"),
                new InfoItem(13, "Déclarations relatives à la taxe professionnelle", "Selon la situation"),
                new InfoItem(14, "Déclaration rectificative ou complémentaire", "En cas de correction"),
                new InfoItem(15, "Déclaration de cessation d'activité fiscale", "Lors de la cessation"),
            }),

        new(
            Social,
            "Déclarations sociales — CNSS et autres organismes",
            "Social",
            "Référence : CNSS Damancom — CIMR.",
            "success",
            new[] { "CNSS", "DTS", "PAIE", "DCH_ETS" },
            new[]
            {
                new InfoItem(1, "Bordereau de déclaration des salaires (BDS)", "Mensuelle"),
                new InfoItem(2, "Déclaration des cotisations CNSS", "Mensuelle"),
                new InfoItem(3, "Déclaration des cotisations AMO", "Selon le régime"),
                new InfoItem(4, "Immatriculation des salariés à la CNSS", "À l'embauche"),
                new InfoItem(5, "Modification des données d'un salarié", "En cas de changement"),
                new InfoItem(6, "Déclaration de sortie d'un salarié", "Lors du départ"),
                new InfoItem(7, "Déclaration rectificative des salaires", "En cas d'erreur"),
                new InfoItem(8, "Régularisation des cotisations sociales", "Si nécessaire"),
                new InfoItem(9, "Déclarations de cotisations CIMR", "Pour les employeurs affiliés"),
                new InfoItem(10, "Déclarations de cotisations aux mutuelles complémentaires", "Pour les employeurs affiliés"),
            }),

        new(
            Juridique,
            "Déclarations et formalités juridiques — OMPIC / Registre du commerce",
            "Juridique",
            "Liens officiels : OMPIC — création et modification d'entreprise · Registre des bénéficiaires effectifs.",
            "warning",
            new[] { "PV_AGO", "DEPOT_RC" },
            new[]
            {
                new InfoItem(1, "Déclaration de création d'entreprise et demande d'immatriculation au registre du commerce (RC)", "À la création"),
                new InfoItem(2, "Dépôt des statuts de la société", "À la constitution ou lors d'une modification"),
                new InfoItem(3, "Dépôt des procès-verbaux d'assemblée générale (PV d'AG)", "Lors des décisions concernées"),
                new InfoItem(4, "Dépôt annuel des états de synthèse au registre du commerce", "Annuellement, pour les sociétés assujetties"),
                new InfoItem(5, "Déclaration d'inscription modificative au registre du commerce", "Lors d'un changement soumis à inscription"),
                new InfoItem(6, "Déclaration de changement de siège social", "Lors du transfert"),
                new InfoItem(7, "Déclaration de changement de gérant ou de dirigeant", "Lors du changement"),
                new InfoItem(8, "Déclaration de modification de la dénomination sociale", "Lors du changement"),
                new InfoItem(9, "Déclaration d'augmentation ou de réduction du capital social", "Lors de l'opération"),
                new InfoItem(10, "Déclaration de modification de l'objet social", "Lors du changement"),
                new InfoItem(11, "Déclaration de changement de forme juridique", "Lors de la transformation"),
                new InfoItem(12, "Dépôt des actes de cession de parts sociales ou d'actions, lorsque requis", "Lors de la cession"),
                new InfoItem(13, "Déclaration des bénéficiaires effectifs au registre public (RBE)", "À la déclaration initiale et lors des modifications requises"),
                new InfoItem(14, "Déclaration de dissolution de la société", "Lors de la dissolution"),
                new InfoItem(15, "Formalités de liquidation et de clôture de liquidation", "Pendant et à la fin de la liquidation"),
                new InfoItem(16, "Demande de radiation du registre du commerce", "À la cessation définitive, lorsque les conditions sont réunies"),
            }),

        new(
            Domiciliation,
            "Domiciliation — siège social et courrier",
            "Domiciliation",
            "Service opérationnel distinct des formalités OMPIC / RC.",
            "secondary",
            new[] { "DOMICILIATION" },
            new[]
            {
                new InfoItem(1, "Mise à disposition d'une adresse de siège social", "Continue"),
                new InfoItem(2, "Réception et gestion du courrier", "Continue"),
                new InfoItem(3, "Remise / numérisation des documents reçus", "Selon les arrivées"),
                new InfoItem(4, "Suivi des échéances liées au dossier domiciliation", "Selon le contrat"),
            }),

        new(
            Comptable,
            "Tenue comptable",
            "Comptable",
            "Périmètre de tenue et états financiers.",
            "info",
            new[] { "COMPTABILITE", "TRESORERIE", "BILAN", "ETATSFISCAUX", "LIASSEFISCALE" },
            new[]
            {
                new InfoItem(1, "Tenue comptable, saisie et rapprochements", "Continue"),
                new InfoItem(2, "Trésorerie et rapprochements bancaires", "Continue"),
                new InfoItem(3, "Bilan, CPC et états de synthèse", "Annuelle"),
                new InfoItem(4, "États fiscaux (CPC, ESG, TFT)", "Annuelle"),
                new InfoItem(5, "Liasse fiscale", "Annuelle"),
            }),
    };

    private static readonly Dictionary<string, string> CodeToGroup =
        All.SelectMany(g => g.AttachedTypeServiceCodes.Select(c => (Code: c, g.GroupId)))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().GroupId, StringComparer.OrdinalIgnoreCase);

    public static ContractServiceGroup? Get(string? groupId)
        => string.IsNullOrWhiteSpace(groupId)
            ? null
            : All.FirstOrDefault(g => g.GroupId.Equals(groupId.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns group ids that have at least one attached code present in <paramref name="activeCodes"/>.</summary>
    public static IReadOnlyList<string> InferSelectedGroupIds(IEnumerable<string?> activeCodes)
    {
        var set = new HashSet<string>(
            activeCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim()),
            StringComparer.OrdinalIgnoreCase);

        return All
            .Where(g => g.AttachedTypeServiceCodes.Any(set.Contains))
            .Select(g => g.GroupId)
            .ToList();
    }

    public static string? GetGroupIdForCode(string? typeService)
        => string.IsNullOrWhiteSpace(typeService) ? null
            : CodeToGroup.TryGetValue(typeService.Trim(), out var id) ? id : null;

    /// <summary>Fallback labels for attached TypeService codes when reference data is missing.</summary>
    public static readonly IReadOnlyDictionary<string, string> CodeLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TVA"] = "TVA – Taxe sur la Valeur Ajoutée",
            ["ACOMPTES"] = "Acomptes IS (trimestriels)",
            ["REG_IS"] = "Régularisation / Solde IS",
            ["DRF"] = "D.R.F – Déclaration du Résultat Fiscal",
            ["CM"] = "CM – Cotisation Minimale",
            ["IR_SAL"] = "IR / Salaires – Retenue à la source",
            ["RAS_FONC"] = "RAS / Foncier",
            ["RAS_DIV"] = "RAS / Dividendes",
            ["IR"] = "IR – Revenu",
            ["EG"] = "E.G – État Descriptif du Gasoil",
            ["DT"] = "Droits d'enregistrement et de timbre",
            ["DEI_TP"] = "Taxe professionnelle",
            ["CNSS"] = "CNSS – Déclarations Damancom",
            ["DTS"] = "D.T.S / État 9421",
            ["PAIE"] = "Bulletins de paie & traitement de la paie",
            ["DCH_ETS"] = "Déclarations sociales complémentaires",
            ["PV_AGO"] = "PV AGO & Rapport de gestion",
            ["DEPOT_RC"] = "Dépôt des comptes au greffe / RC",
            ["DOMICILIATION"] = "Domiciliation – Siège social et courrier",
            ["COMPTABILITE"] = "Tenue comptable, saisie & rapprochements",
            ["TRESORERIE"] = "Trésorerie & rapprochements bancaires",
            ["BILAN"] = "Bilan, CPC & états de synthèse",
            ["ETATSFISCAUX"] = "États fiscaux (CPC, ESG, TFT)",
            ["LIASSEFISCALE"] = "Liasse fiscale",
        };

    public static string LabelForCode(string code, IEnumerable<(string Code, string Label)>? refLabels = null)
    {
        if (refLabels != null)
        {
            var hit = refLabels.FirstOrDefault(r =>
                string.Equals(r.Code, code, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(hit.Label))
                return hit.Label;
        }
        return CodeLabels.TryGetValue(code, out var lbl) ? lbl : code;
    }
}
