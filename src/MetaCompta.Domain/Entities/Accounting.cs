using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>Fiscal year for a client's ledger (client-scoped).</summary>
public class ExerciceComptable : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public int Annee { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    /// <summary>Ouvert | Cloture</summary>
    public string Statut { get; set; } = "Ouvert";

    public virtual ICollection<PeriodeComptable> Periodes { get; set; } = new List<PeriodeComptable>();
}

/// <summary>Monthly / period lock within an exercice.</summary>
public class PeriodeComptable : TenantEntity
{
    public int ExerciceComptableId { get; set; }
    public virtual ExerciceComptable? Exercice { get; set; }

    public int ClientId { get; set; }
    public int Annee { get; set; }
    public int Mois { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public bool EstVerrouillee { get; set; }
}

/// <summary>PCGM chart of accounts line (per client).</summary>
public class CompteComptable : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string Numero { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    /// <summary>Class 1–7 (PCGM).</summary>
    public string Classe { get; set; } = string.Empty;
    public string? CompteParentNumero { get; set; }
    /// <summary>General | Auxiliaire | Titre</summary>
    public string Nature { get; set; } = "General";
    public bool EstActif { get; set; } = true;
    public bool EstCollectif { get; set; }
}

/// <summary>Accounting journal (AC, VE, BQ…).</summary>
public class JournalComptable : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    /// <summary>Achats | Ventes | Banque | Caisse | OD | AN | Paie</summary>
    public string Type { get; set; } = "OD";
    public int ProchainNumero { get; set; } = 1;
    public bool EstActif { get; set; } = true;
}

/// <summary>Entry header.</summary>
public class EcritureComptable : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public int? ExerciceComptableId { get; set; }
    public virtual ExerciceComptable? Exercice { get; set; }

    public int JournalComptableId { get; set; }
    public virtual JournalComptable? Journal { get; set; }

    public string JournalCode { get; set; } = string.Empty;
    public string NumeroPiece { get; set; } = string.Empty;
    public DateTime DateEcriture { get; set; }
    public DateTime? DatePiece { get; set; }
    public string? Libelle { get; set; }
    public string? Reference { get; set; }
    /// <summary>Brouillon | Validee</summary>
    public string Statut { get; set; } = "Brouillon";
    public DateTime? DateValidation { get; set; }
    public string? ValideeParUserId { get; set; }

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public virtual ICollection<LigneEcriture> Lignes { get; set; } = new List<LigneEcriture>();
}

/// <summary>Entry line.</summary>
public class LigneEcriture : TenantEntity
{
    public int EcritureComptableId { get; set; }
    public virtual EcritureComptable? Ecriture { get; set; }

    public int ClientId { get; set; }
    public int? CompteComptableId { get; set; }
    public virtual CompteComptable? Compte { get; set; }

    public string NumCompte { get; set; } = string.Empty;
    public string LibelleCompte { get; set; } = string.Empty;
    public string? CompteAuxiliaire { get; set; }
    public string? LibelleLigne { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? CodeTva { get; set; }
    public decimal? MontantTva { get; set; }
    /// <summary>Simple lettrage code (same code on lignes of one compte).</summary>
    public string? LettrageCode { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Client bank account for reconciliation.</summary>
public class CompteBancaire : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    /// <summary>Linked PCGM account (e.g. 5141).</summary>
    public string CompteComptableNumero { get; set; } = "5141";
    public string? Iban { get; set; }
    public string? BanqueNom { get; set; }
    public bool EstActif { get; set; } = true;
}

/// <summary>Bank reconciliation header.</summary>
public class RapprochementBancaire : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public int? CompteBancaireId { get; set; }
    public virtual CompteBancaire? CompteBancaireNav { get; set; }

    public int? ExerciceComptableId { get; set; }
    public virtual ExerciceComptable? Exercice { get; set; }

    public string CompteBancaire { get; set; } = string.Empty;
    public string Periode { get; set; } = string.Empty;
    public decimal SoldeCompta { get; set; }
    public decimal SoldeBanque { get; set; }
    public decimal Ecart { get; set; }
    public bool EstRapproche { get; set; }
    /// <summary>EnCours | Rapproche</summary>
    public string Statut { get; set; } = "EnCours";
    public int NombreLignesNonRapprochees { get; set; }
    public DateTime DateRapprochement { get; set; }
    public string? Notes { get; set; }

    public virtual ICollection<RapprochementLigne> Lignes { get; set; } = new List<RapprochementLigne>();
}

public class RapprochementLigne : TenantEntity
{
    public int RapprochementBancaireId { get; set; }
    public virtual RapprochementBancaire? Rapprochement { get; set; }

    public int ClientId { get; set; }
    public int? LigneEcritureId { get; set; }
    public virtual LigneEcriture? LigneEcriture { get; set; }

    public DateTime? DateOperation { get; set; }
    public string? Libelle { get; set; }
    public decimal Montant { get; set; }
    public bool EstRapprochee { get; set; }
}
