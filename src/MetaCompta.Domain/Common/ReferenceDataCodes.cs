namespace MetaCompta.Domain.Common;

/// <summary>
/// Centralised ENUM-like constants for every ReferenceData category & code.
/// Each nested class corresponds to a Category in the ReferenceData table.
/// Use these constants instead of magic strings throughout the codebase.
/// </summary>
public static class RefCodes
{
    // ════════════════════════════════════════════════════════════
    //  CATEGORY NAMES
    // ════════════════════════════════════════════════════════════

    public static class Categories
    {
        public const string TvaRate = "TVA_RATE";
        public const string Cnss = "CNSS";
        public const string Amo = "AMO";
        public const string FP = "FP";
        public const string IrBracket = "IR_BRACKET";
        public const string IsRate = "IS_RATE";
        public const string IrDeduction = "IR_DEDUCTION";
        public const string DocCategory = "DOC_CATEGORY";
        public const string TypeContrat = "TYPE_CONTRAT";
        public const string FormeJuridique = "FORME_JURIDIQUE";
        public const string StatutClient = "STATUT_CLIENT";
        public const string StatutEmploye = "STATUT_EMPLOYE";
        public const string TypeDeclaration = "TYPE_DECLARATION";
        public const string StatutDeclaration = "STATUT_DECLARATION";
        public const string TypeTva = "TYPE_TVA";
        public const string Ville = "VILLE";
        public const string SituationFamiliale = "SITUATION_FAMILIALE";
        public const string Sexe = "SEXE";
        public const string ModePaiement = "MODE_PAIEMENT";
        public const string TypeAbsence = "TYPE_ABSENCE";
        public const string StatutDocument = "STATUT_DOCUMENT";
        public const string PrioriteTache = "PRIORITE_TACHE";
        public const string StatutTache = "STATUT_TACHE";
        public const string StatutPaie = "STATUT_PAIE";
        public const string SecteurActivite = "SECTEUR_ACTIVITE";
        public const string CategorieDocument = "CATEGORIE_DOCUMENT";
        public const string StatutDevis = "STATUT_DEVIS";
        public const string StatutBonCommande = "STATUT_BON_COMMANDE";
        public const string StatutContrat = "STATUT_CONTRAT";
        public const string StatutFacture = "STATUT_FACTURE";
        public const string StatutPaiement = "STATUT_PAIEMENT";
        public const string ModeleFacturation = "MODELE_FACTURATION";
        public const string TypeLigne = "TYPE_LIGNE";
        public const string ModePaiementCommercial = "MODE_PAIEMENT_COMMERCIAL";
        public const string TypeService = "TYPE_SERVICE";
        public const string CategorieService = "CATEGORIE_SERVICE";
        public const string Frequence = "FREQUENCE";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT DÉCLARATION
    // ════════════════════════════════════════════════════════════

    public static class StatutDeclaration
    {
        // Legacy codes (kept for existing declarations + aliases)
        public const string Brouillon = "Brouillon";
        public const string ACompleter = "ACompleter";
        public const string EnCours = "EnCours";
        public const string EnPreparation = "EnPreparation";
        public const string Completee = "Completee";
        public const string EnRevision = "EnRevision";
        public const string Validee = "Validee";
        public const string Soumise = "Soumise";
        public const string Payee = "Payee";
        public const string Refusee = "Refusee";
        public const string Rejetee = "Rejetee";

        // TVA end-to-end lifecycle (plan alignment)
        public const string ACreer = "ACreer";
        public const string CreeeParSysteme = "CreeeParSysteme";
        public const string DocumentsDemandes = "DocumentsDemandes";
        public const string DocumentsRecus = "DocumentsRecus";
        public const string ControleEnCours = "ControleEnCours";
        public const string EnAttenteCorrection = "EnAttenteCorrection";
        public const string CalculTVA = "CalculTVA";
        public const string EnRevueSuperviseur = "EnRevueSuperviseur";
        public const string ACorriger = "ACorriger";
        public const string EnvoyeeSimplTva = "EnvoyeeSimplTva";
        public const string Deposee = "Deposee";
        public const string APayer = "APayer";
        public const string Rapprochee = "Rapprochee";
        public const string Archivee = "Archivee";
        public const string Cloturee = "Cloturee";

        /// <summary>French UI labels for badges (canonical + legacy).</summary>
        public static string Label(string? statut) => statut switch
        {
            ACreer => "À créer",
            CreeeParSysteme => "Créée par le système",
            Brouillon => "Brouillon",
            DocumentsDemandes => "Documents demandés",
            ACompleter => "À compléter",
            DocumentsRecus => "Documents reçus",
            ControleEnCours => "Contrôle en cours",
            EnCours => "En cours",
            EnPreparation => "En préparation",
            Completee => "Complétée",
            EnAttenteCorrection => "En attente correction",
            CalculTVA => "Calcul TVA",
            EnRevueSuperviseur => "En revue superviseur",
            EnRevision => "En révision",
            ACorriger => "À corriger",
            Refusee => "Refusée",
            Validee => "Validée",
            EnvoyeeSimplTva => "Envoyée à SIMPL-TVA",
            Soumise => "Soumise",
            Rejetee => "Rejetée",
            Deposee => "Déposée",
            APayer => "À payer",
            Payee => "Payée",
            Rapprochee => "Rapprochée",
            Archivee => "Archivée",
            Cloturee => "Clôturée",
            _ => statut ?? ""
        };

        /// <summary>CSS badge class for Index/Details.</summary>
        public static string BadgeClass(string? statut) => statut switch
        {
            ACreer or CreeeParSysteme or Brouillon => "badge-brouillon",
            DocumentsDemandes or ACompleter or EnAttenteCorrection or ACorriger or Refusee => "badge-acompleter",
            DocumentsRecus or ControleEnCours or EnCours or EnPreparation or Completee or CalculTVA => "badge-completee",
            EnRevueSuperviseur or EnRevision => "badge-enrevision",
            Validee or Deposee or Rapprochee or Archivee => "badge-validee",
            EnvoyeeSimplTva or Soumise or Payee or APayer or Cloturee => "badge-soumise",
            Rejetee => "badge-refusee",
            _ => "badge-brouillon"
        };
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT CONTRAT COMMERCIAL
    // ════════════════════════════════════════════════════════════

    public static class StatutContrat
    {
        public const string Brouillon = "Brouillon";
        public const string EnCours = "EnCours";
        public const string Signe = "Signe";
        public const string Confirme = "Confirme";
        public const string Valide = "Valide";
        public const string Actif = "Actif";
        public const string Suspendu = "Suspendu";
        public const string Expire = "Expire";
        public const string Resilie = "Resilie";
        public const string Termine = "Termine";
        public const string Annule = "Annule";

        /// <summary>Statuses that allow declarations and service operations.</summary>
        public static readonly string[] ActiveStatuses = [Valide, Actif];
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT DEVIS
    // ════════════════════════════════════════════════════════════

    public static class StatutDevis
    {
        public const string Brouillon = "Brouillon";
        public const string Soumis = "Soumis";
        public const string Approuve = "Approuve";
        public const string Rejete = "Rejete";
        public const string Expire = "Expire";
        public const string Annule = "Annule";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT BON DE COMMANDE
    // ════════════════════════════════════════════════════════════

    public static class StatutBonCommande
    {
        public const string Brouillon = "Brouillon";
        public const string Confirme = "Confirme";
        public const string Approuve = "Approuve";
        public const string Annule = "Annule";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT FACTURE
    // ════════════════════════════════════════════════════════════

    public static class StatutFacture
    {
        public const string Brouillon = "Brouillon";
        public const string Emise = "Emise";
        public const string Generee = "Generee";
        public const string Envoyee = "Envoyee";
        public const string Vue = "Vue";
        public const string PartiellementPayee = "PartiellementPayee";
        public const string Payee = "Payee";
        public const string EnRetard = "EnRetard";
        public const string Annulee = "Annulee";
        public const string Remboursee = "Remboursee";
        public const string Avoir = "Avoir";
        public const string Contentieux = "Contentieux";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT PAIEMENT
    // ════════════════════════════════════════════════════════════

    public static class StatutPaiement
    {
        public const string Valide = "Valide";
        public const string EnAttente = "EnAttente";
        public const string Rejete = "Rejete";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT PAIE
    // ════════════════════════════════════════════════════════════

    public static class StatutPaie
    {
        public const string Brouillon = "Brouillon";
        public const string Calcule = "Calcule";
        public const string EnCours = "EnCours";
        public const string EnAttenteValidation = "EnAttenteValidation";
        public const string Validee = "Validee";
        public const string Valide = "Valide";
        public const string Cloturee = "Cloturee";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT CLIENT
    // ════════════════════════════════════════════════════════════

    public static class StatutClient
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Dormante = "Dormante";
        public const string Dissoute = "Dissoute";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT EMPLOYE
    // ════════════════════════════════════════════════════════════

    public static class StatutEmploye
    {
        public const string Actif = "Actif";
        public const string Inactif = "Inactif";
        public const string Suspendu = "Suspendu";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT DOCUMENT
    // ════════════════════════════════════════════════════════════

    public static class StatutDocument
    {
        public const string EnAttente = "EnAttente";
        public const string Valide = "Valide";
        public const string Rejete = "Rejete";
        public const string Archive = "Archive";
    }

    // ════════════════════════════════════════════════════════════
    //  STATUT TACHE
    // ════════════════════════════════════════════════════════════

    public static class StatutTache
    {
        public const string AFaire = "AFaire";
        public const string EnCours = "EnCours";
        public const string EnAttenteRevision = "EnAttenteRevision";
        public const string Termine = "Termine";
        public const string Annule = "Annule";
    }

    // ════════════════════════════════════════════════════════════
    //  TYPE DÉCLARATION
    // ════════════════════════════════════════════════════════════

    public static class TypeDeclaration
    {
        public const string TVA = "TVA";
        public const string IS = "IS";
        public const string IR = "IR";
        public const string CNSS = "CNSS";
        public const string IR_Salaires = "IR_SAL";
        public const string Acomptes = "ACOMPTES";
        public const string RegIs = "REG_IS";
        public const string CM = "CM";
        public const string DRF = "DRF";
        public const string RasFonc = "RAS_FONC";
        public const string RasDiv = "RAS_DIV";
        public const string DTS = "DTS";
    }

    // ════════════════════════════════════════════════════════════
    //  TYPE SERVICE (ServiceContrat.TypeService)
    // ════════════════════════════════════════════════════════════

    public static class TypeService
    {
        public const string TVA = "TVA";
        public const string IS = "IS";
        public const string IR = "IR";
        public const string CNSS = "CNSS";
        public const string Paie = "PAIE";
        public const string Juridique = "JURIDIQUE";
        public const string Comptabilite = "COMPTABILITE";
        public const string Bilan = "BILAN";
        public const string EtatsFiscaux = "ETATSFISCAUX";
        public const string AuditComptable = "AUDITCOMPTABLE";
        public const string ConseilFiscal = "CONSEILFISCAL";
        public const string Acomptes = "ACOMPTES";
        public const string RegIs = "REG_IS";
        public const string CM = "CM";
        public const string DRF = "DRF";
        public const string IrSal = "IR_SAL";
        public const string RasFonc = "RAS_FONC";
        public const string RasDiv = "RAS_DIV";
        public const string DTS = "DTS";
        public const string LiasseFiscale = "LIASSEFISCALE";
    }

    // ════════════════════════════════════════════════════════════
    //  CATEGORIE SERVICE
    // ════════════════════════════════════════════════════════════

    public static class CategorieService
    {
        public const string Fiscal = "Fiscal";
        public const string Social = "Social";
        public const string Comptable = "Comptable";
        public const string Juridique = "Juridique";
        public const string Conseil = "Conseil";
        public const string Autre = "Autre";
    }

    // ════════════════════════════════════════════════════════════
    //  FREQUENCE
    // ════════════════════════════════════════════════════════════

    public static class Frequence
    {
        public const string Mensuelle = "Mensuelle";
        public const string Trimestrielle = "Trimestrielle";
        public const string Annuelle = "Annuelle";
        public const string Ponctuelle = "Ponctuelle";
    }

    // ════════════════════════════════════════════════════════════
    //  TYPE LIGNE DOCUMENT
    // ════════════════════════════════════════════════════════════

    public static class TypeLigne
    {
        public const string Service = "Service";
        public const string Regie = "Regie";
        public const string Forfait = "Forfait";
    }

    // ════════════════════════════════════════════════════════════
    //  MODELE FACTURATION
    // ════════════════════════════════════════════════════════════

    public static class ModeleFacturation
    {
        public const string Forfait = "Forfait";
        public const string Regie = "Regie";
    }

    // ════════════════════════════════════════════════════════════
    //  TYPE TVA (Régime)
    // ════════════════════════════════════════════════════════════

    public static class TypeTva
    {
        public const string Mensuelle = "Mensuelle";
        public const string Trimestrielle = "Trimestrielle";
        public const string Exonere = "Exonere";
    }

    // ════════════════════════════════════════════════════════════
    //  PRIORITE TACHE
    // ════════════════════════════════════════════════════════════

    public static class PrioriteTache
    {
        public const string Faible = "Faible";
        public const string Moyenne = "Moyenne";
        public const string Haute = "Haute";
        public const string Urgente = "Urgente";
    }
}
