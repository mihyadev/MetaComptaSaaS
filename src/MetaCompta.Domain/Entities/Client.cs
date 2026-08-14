using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>
/// Client company managed by the fiduciary cabinet
/// </summary>
public class Client : TenantEntity
{
    // Identification
    public string RaisonSociale { get; set; } = string.Empty;
    public string? ICE { get; set; } // 15 digits, unique
    public string? IF { get; set; }  // Identifiant Fiscal, unique
    public string? RC { get; set; }  // Registre Commerce, unique
    public string? NumeroCNSS { get; set; } // unique
    public string? NumeroPatente { get; set; }
    public DateTime? DateDebutActivite { get; set; }
    public string? CodeActivite { get; set; }
    public string? FormeJuridique { get; set; } // SARL, SA, SNC, etc.

    // Legal info
    public string? SecteurActivite { get; set; }
    public DateTime? DateCreation { get; set; }
    public decimal? CapitalSocial { get; set; }
    public string? AdresseSiegeSocial { get; set; }
    public string? VilleSiegeSocial { get; set; }
    public string? TelephonePrincipal { get; set; }
    public string? EmailPrincipal { get; set; }

    // Contact principal
    public string? ContactNom { get; set; }
    public string? ContactFonction { get; set; }
    public string? ContactTelephone { get; set; }
    public string? ContactEmail { get; set; }

    // Service info
    public string? ManagerPrincipalId { get; set; }
    public virtual ApplicationUser? ManagerPrincipal { get; set; }
    public string TypeTVA { get; set; } = "Mensuelle"; // Mensuelle, Trimestrielle
    public int MoisClotureExercice { get; set; } = 12; // Default December
    public string Statut { get; set; } = "Active"; // Active, Dissoute, Dormante
    public DateTime? DateDebutContrat { get; set; }
    public string? NotesInternes { get; set; }
    public string? CustomFields { get; set; } // JSON for extensibility

    // Navigation
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Declaration> Declarations { get; set; } = new List<Declaration>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<PayrollPeriod> PayrollPeriods { get; set; } = new List<PayrollPeriod>();
    public virtual ICollection<ApplicationUser> ExtranetUsers { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<ServiceContrat> ServiceContrats { get; set; } = new List<ServiceContrat>();

    // Commercial
    public virtual ICollection<Devis> Devis { get; set; } = new List<Devis>();
    public virtual ICollection<BonCommande> BonsCommande { get; set; } = new List<BonCommande>();
    public virtual ICollection<ContratCommercial> ContratsCommerciaux { get; set; } = new List<ContratCommercial>();
    public virtual ICollection<Facture> Factures { get; set; } = new List<Facture>();
}

/// <summary>
/// Enterprise-grade fiduciary service contract for a client.
/// Each service line represents a specific obligation (fiscal, social, accounting, legal)
/// with temporal boundaries, exercise year awareness, smart frequency defaults,
/// and per-service configuration (regime, rates, thresholds).
/// Declaration auto-generation is driven from active service contracts.
/// </summary>
public class ServiceContrat : BaseEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // ──────── Service Identity ────────
    /// <summary>
    /// Service type code: TVA, IS, IR, CNSS, Paie, Juridique, Comptabilite, Bilan, AuditComptable, ConseilFiscal, EtatsFiscaux
    /// </summary>
    public string TypeService { get; set; } = string.Empty;

    /// <summary>Display name for this service (auto-generated from TypeService)</summary>
    public string LibelleService { get; set; } = string.Empty;

    /// <summary>Category: Fiscal, Social, Comptable, Juridique, Conseil</summary>
    public string Categorie { get; set; } = string.Empty;

    // ──────── Temporal Boundaries ────────
    /// <summary>Contract start date for this service</summary>
    public DateTime DateDebut { get; set; }

    /// <summary>Contract end date (null = open-ended / ongoing)</summary>
    public DateTime? DateFin { get; set; }

    /// <summary>Exercise fiscal year this service applies to (e.g. 2026). Null for ongoing services.</summary>
    public int? ExerciceFiscal { get; set; }

    /// <summary>Canonical link to ledger exercice (preferred over ExerciceFiscal alone).</summary>
    public int? ExerciceComptableId { get; set; }
    public virtual ExerciceComptable? ExerciceComptable { get; set; }

    /// <summary>Exercise start date (computed from Client.MoisClotureExercice, e.g. 01/01/2026 for Dec closing)</summary>
    public DateTime? ExerciceDebut { get; set; }

    /// <summary>Exercise end date (e.g. 31/12/2026 for Dec closing)</summary>
    public DateTime? ExerciceFin { get; set; }

    // ──────── Frequency & Scheduling ────────
    /// <summary>Mensuelle, Trimestrielle, Annuelle, Ponctuelle</summary>
    public string Frequence { get; set; } = "Mensuelle";

    /// <summary>Day of the month for the declaration deadline (e.g., 20 for TVA = 20th of month+1)</summary>
    public int? JourLimiteDeclaration { get; set; }

    /// <summary>Number of months after period end for the deadline (e.g., 1 for TVA = M+1)</summary>
    public int? DelaiDeclarationMois { get; set; }

    // ──────── Service Configuration (JSON) ────────
    /// <summary>
    /// JSON configuration specific to each service type:
    /// TVA: {"regime":"Debit|Encaissement","prorata":100,"exoneration":false}
    /// IS: {"tauxApplicable":20,"cotisationMinimale":0.75,"exerciceDebut":"2026-01-01"}
    /// IR: {"categorieRevenus":"Professionnel|Foncier|Salarial","regime":"RNR|RNS|Forfait"}
    /// CNSS: {"nombreEmployes":5,"plafondMensuel":6000}
    /// Paie: {"convention":"General|BTP|Commerce","heuresMensuelles":191}
    /// </summary>
    public string? ConfigurationJson { get; set; }

    // ──────── Commercial ────────
    /// <summary>Monthly fees (MAD)</summary>
    public decimal? HonorairesMensuels { get; set; }

    /// <summary>Annual fees (MAD) - for annual services</summary>
    public decimal? HonorairesAnnuels { get; set; }

    /// <summary>Whether this service is currently active for the client</summary>
    public bool Actif { get; set; } = true;

    /// <summary>Auto-generate declarations for this service when new exercise starts</summary>
    public bool AutoGenererDeclarations { get; set; } = true;

    /// <summary>Responsible collaborator ID for this specific service</summary>
    public string? ResponsableId { get; set; }
    public virtual ApplicationUser? Responsable { get; set; }

    public string? Notes { get; set; }

    // ──────── Helpers ────────
    /// <summary>Is the service active at a given date?</summary>
    public bool EstActifAuDate(DateTime date) =>
        Actif && DateDebut <= date && (DateFin == null || DateFin >= date);

    /// <summary>Is the date within this service's exercise period?</summary>
    public bool EstDansExercice(DateTime date) =>
        ExerciceDebut.HasValue && ExerciceFin.HasValue &&
        date >= ExerciceDebut.Value && date <= ExerciceFin.Value;
}
