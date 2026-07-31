using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║          PAYROLL ENGINE ENTITIES — Sage-like Moroccan Payroll System        ║
// ╚══════════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Rubrique de paie — a payroll element (gain, retenue, calculated, informational).
/// Each rubrique defines WHAT appears on a bulletin de paie.
/// Execution order matters — rubriques are evaluated top-to-bottom by OrdreExecution.
/// </summary>
public class Rubrique : TenantEntity
{
    /// <summary>Unique code (e.g. SAL_BASE, CNSS_SAL, IR, NET_A_PAYER)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display label on bulletin</summary>
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Type: Gain, Retenue, Cotisation, Impot, Calcule, Info</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Section on bulletin: Salaire, Prime, Avantage, Cotisation, Impot, Net</summary>
    public string Section { get; set; } = string.Empty;

    /// <summary>Execution order (lower = first). Used for dependency resolution.</summary>
    public int OrdreExecution { get; set; }

    /// <summary>Display order on the bulletin (can differ from execution order)</summary>
    public int OrdreAffichage { get; set; }

    /// <summary>Is this rubrique active (included in payroll calculations)?</summary>
    public bool Actif { get; set; } = true;

    /// <summary>Is this a system rubrique (cannot be deleted, only deactivated)?</summary>
    public bool Systeme { get; set; } = false;

    /// <summary>Does this rubrique appear on the bulletin?</summary>
    public bool AfficherSurBulletin { get; set; } = true;

    /// <summary>Is this rubrique mandatory for all employees?</summary>
    public bool Obligatoire { get; set; } = false;

    /// <summary>Column: Base — formula to compute the base amount (e.g. "BRUT_GLOBAL")</summary>
    public string? FormuleBase { get; set; }

    /// <summary>Column: Taux — formula to compute the rate (e.g. "CONST.CNSS_TAUX_SAL")</summary>
    public string? FormuleTaux { get; set; }

    /// <summary>Column: Montant — formula (e.g. "BASE * TAUX / 100" or "SUM_SECTION(Gain)")</summary>
    public string? FormuleMontant { get; set; }

    /// <summary>Column: Gain — formula (e.g. "MONTANT" for gain-type rubriques)</summary>
    public string? FormuleGain { get; set; }

    /// <summary>Column: Retenue — formula (e.g. "MONTANT" for retenue-type rubriques)</summary>
    public string? FormuleRetenue { get; set; }

    /// <summary>Comma-separated list of rubrique codes or constants this depends on</summary>
    public string? DependsDe { get; set; }

    /// <summary>Description / help text</summary>
    public string? Description { get; set; }

    /// <summary>Optional: linked to a specific client (null = applies to all)</summary>
    public int? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // Navigation
    public virtual ICollection<RegleCalcul> Regles { get; set; } = new List<RegleCalcul>();
    public virtual ICollection<BulletinLigne> BulletinLignes { get; set; } = new List<BulletinLigne>();
}

/// <summary>
/// Constante globale — a named parameter used in payroll formulas.
/// Versioned by date for historical tracking (like Sage).
/// Examples: CNSS_PLAFOND, CNSS_TAUX_SAL, AMO_TAUX_SAL, IR_BRACKET_1_MIN...
/// </summary>
public class ConstanteGlobale : TenantEntity
{
    /// <summary>Unique code (e.g. CNSS_PLAFOND, AMO_TAUX_SAL)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display label</summary>
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Category for grouping: CNSS, AMO, CIMR, IR, General</summary>
    public string Categorie { get; set; } = string.Empty;

    /// <summary>Numeric value</summary>
    public decimal Valeur { get; set; }

    /// <summary>Effective date (for versioning)</summary>
    public DateTime DateEffet { get; set; } = DateTime.UtcNow;

    /// <summary>End date (null = still active)</summary>
    public DateTime? DateFin { get; set; }

    /// <summary>Is this constant currently active?</summary>
    public bool Actif { get; set; } = true;

    /// <summary>Description / legal reference</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Variable spécifique à un employé — overrides or complements rubrique values per employee.
/// Example: Prime de transport = 500 MAD for employee X, 300 for employee Y.
/// </summary>
public class VariableEmploye : TenantEntity
{
    public int EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    /// <summary>The rubrique code this variable applies to (e.g. PRIME_TRANSPORT)</summary>
    public string RubriqueCode { get; set; } = string.Empty;

    /// <summary>The value for this employee (amount, hours, days, etc.)</summary>
    public decimal Valeur { get; set; }

    /// <summary>Optional label override for this employee</summary>
    public string? LibellePersonnalise { get; set; }

    /// <summary>Is this variable active for this employee?</summary>
    public bool Actif { get; set; } = true;

    /// <summary>Effective from date</summary>
    public DateTime DateEffet { get; set; } = DateTime.UtcNow;

    /// <summary>End date (null = still active)</summary>
    public DateTime? DateFin { get; set; }
}

/// <summary>
/// Règle de calcul — a calculation rule attached to a rubrique.
/// Supports conditions and multiple formulas per rubrique (like Sage planchers/plafonds).
/// </summary>
public class RegleCalcul : TenantEntity
{
    public int RubriqueId { get; set; }
    public virtual Rubrique? Rubrique { get; set; }

    /// <summary>Execution priority within the rubrique (lower = first)</summary>
    public int Priorite { get; set; }

    /// <summary>Condition expression (e.g. "BRUT_GLOBAL > CONST.CNSS_PLAFOND")</summary>
    public string? Condition { get; set; }

    /// <summary>Formula to execute if condition is met (or always if no condition)</summary>
    public string Formule { get; set; } = string.Empty;

    /// <summary>Description of this rule</summary>
    public string? Description { get; set; }

    public bool Actif { get; set; } = true;
}

/// <summary>
/// Ligne de bulletin de paie — one row on the generated pay slip.
/// Each line corresponds to one rubrique that was evaluated during payroll calculation.
/// </summary>
public class BulletinLigne : BaseEntity
{
    public int PaySlipId { get; set; }
    public virtual PaySlip? PaySlip { get; set; }

    public int RubriqueId { get; set; }
    public virtual Rubrique? Rubrique { get; set; }

    /// <summary>Rubrique code (denormalized for performance)</summary>
    public string RubriqueCode { get; set; } = string.Empty;

    /// <summary>Display label (may be personalized per employee)</summary>
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Section for display grouping</summary>
    public string Section { get; set; } = string.Empty;

    /// <summary>Type: Gain, Retenue, etc.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Display order</summary>
    public int OrdreAffichage { get; set; }

    /// <summary>Base amount (e.g. brut salary for CNSS calculation)</summary>
    public decimal Base { get; set; }

    /// <summary>Rate applied (e.g. 4.48% for CNSS)</summary>
    public decimal Taux { get; set; }

    /// <summary>Gain amount (positive contribution to salary)</summary>
    public decimal Gain { get; set; }

    /// <summary>Retenue amount (deduction from salary)</summary>
    public decimal Retenue { get; set; }

    /// <summary>Whether this line should be visible on the printed bulletin</summary>
    public bool AfficherSurBulletin { get; set; } = true;
}

/// <summary>
/// Cumul — aggregated payroll values per employee per rubrique per month.
/// Used for annual cumulative tracking (CNSS annuelle, IR cumulé, etc.)
/// </summary>
public class CumulEmploye : TenantEntity
{
    public int EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    /// <summary>Rubrique code</summary>
    public string RubriqueCode { get; set; } = string.Empty;

    public int Mois { get; set; }
    public int Annee { get; set; }

    /// <summary>The cumulated value for this rubrique/month/year</summary>
    public decimal Valeur { get; set; }
}

/// <summary>
/// Affectation d'une rubrique a un employe specifique.
/// Determines which optional rubriques appear on an employee's bulletin.
/// </summary>
public class AffectationRubrique : TenantEntity
{
    public int EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    public int RubriqueId { get; set; }
    public virtual Rubrique? Rubrique { get; set; }

    /// <summary>Rubrique code cached for fast lookup</summary>
    public string RubriqueCode { get; set; } = string.Empty;

    /// <summary>Default value used if no monthly VariableEmploye exists</summary>
    public decimal ValeurDefaut { get; set; }

    public bool Actif { get; set; } = true;
    public DateTime? DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
}
