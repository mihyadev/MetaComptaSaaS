using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>
/// Fiscal/Social declaration (TVA, IS, IR, CNSS)
/// </summary>
public class Declaration : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string Type { get; set; } = string.Empty; // TVA, IS, IR, CNSS, IR_SALAIRES
    public string Periode { get; set; } = string.Empty; // "2026-02" for monthly, "2026-Q1" for quarterly, "2026" for annual
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public DateTime DateLimite { get; set; }

    // Workflow status
    public string Statut { get; set; } = RefCodes.StatutDeclaration.Brouillon; // Brouillon, ACompleter, Completee, EnRevision, Validee, Soumise, Refusee
    public DateTime? DateSoumission { get; set; }
    public string? MotifRefus { get; set; }

    // Assignees
    public string? AssistantId { get; set; }
    public virtual ApplicationUser? Assistant { get; set; }
    public string? ValidateurId { get; set; }
    public virtual ApplicationUser? Validateur { get; set; }

    // Data (JSON for flexibility per declaration type)
    public string? DonneesJson { get; set; }

    // Computed amounts
    public decimal? MontantDu { get; set; }
    public decimal? MontantCredit { get; set; }

    // Generated documents
    public string? PdfUrl { get; set; }
    public string? ExportUrl { get; set; }

    public string? Notes { get; set; }
    public int Version { get; set; } = 1;

    // Navigation
    public virtual ICollection<DeclarationHistory> History { get; set; } = new List<DeclarationHistory>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}

/// <summary>
/// TVA declaration specific data
/// </summary>
public class DeclarationTVA : TenantEntity
{
    public int DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    // CA HT by rate
    public decimal CAHT_20 { get; set; }
    public decimal CAHT_14 { get; set; }
    public decimal CAHT_10 { get; set; }
    public decimal CAHT_7 { get; set; }
    public decimal CAExonere { get; set; }

    // Computed TVA Collected
    public decimal TVA_20 { get; set; }
    public decimal TVA_14 { get; set; }
    public decimal TVA_10 { get; set; }
    public decimal TVA_7 { get; set; }
    public decimal TVACollecteeTotale { get; set; }

    // Recoverable
    public decimal TVARecuperableCharges { get; set; }
    public decimal TVARecuperableImmobilisations { get; set; }
    public decimal TVARecuperableTotale { get; set; }

    // Credit
    public decimal CreditPrecedent { get; set; }

    // Result
    public decimal TVANette { get; set; }
    public decimal TVAaPayer { get; set; }
    public decimal CreditTVA { get; set; }

    // Navigation - Invoice lines
    public virtual ICollection<TVALigneFacture> LignesFactures { get; set; } = new List<TVALigneFacture>();
}

/// <summary>
/// IS (Corporate Tax) declaration specific data
/// </summary>
public class DeclarationIS : TenantEntity
{
    public int DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    public int AnneeFiscale { get; set; }
    public decimal ResultatComptable { get; set; }
    public decimal Reintegrations { get; set; }
    public decimal Deductions { get; set; }
    public decimal ResultatFiscal { get; set; }
    public decimal DeficitsAnterieurs { get; set; }
    public decimal BaseImposable { get; set; }
    public decimal TauxIS { get; set; }
    public decimal ISDu { get; set; }
    public decimal AcomptesVerses { get; set; }
    public decimal ISNet { get; set; }

    // Quarterly installments
    public decimal Acompte1 { get; set; } // 31 March
    public decimal Acompte2 { get; set; } // 30 June
    public decimal Acompte3 { get; set; } // 30 September
    public decimal Acompte4 { get; set; } // 31 December
}

/// <summary>
/// CNSS declaration specific data
/// </summary>
public class DeclarationCNSS : TenantEntity
{
    public int DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    public decimal TotalSalairesBruts { get; set; }
    public decimal TotalCotisationSalariale { get; set; }
    public decimal TotalCotisationPatronale { get; set; }
    public decimal TotalAMOSalariale { get; set; }
    public decimal TotalAMOPatronale { get; set; }
    public decimal TotalFormationProfessionnelle { get; set; }
    public decimal MontantTotal { get; set; }
    public int NombreEmployes { get; set; }

    // Per-employee detail (JSON)
    public string? DetailEmployesJson { get; set; }
}


/// <summary>
/// TVA Invoice line (ligne de facture) for Morocco TVA declaration
/// </summary>
public class TVALigneFacture : TenantEntity
{
    public int DeclarationTVAId { get; set; }
    public virtual DeclarationTVA? DeclarationTVA { get; set; }

    public string TypePiece { get; set; } = "Facture"; // Facture, Avoir, AutoFacture
    public string? NumeroFacture { get; set; }
    public DateTime DateFacture { get; set; } = DateTime.Today;
    public string? FournisseurClient { get; set; }
    public string? ICE { get; set; }
    public string? IdentifiantFiscal { get; set; }
    public string? Description { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; } = 20;
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string TypeOperation { get; set; } = "Vente"; // Vente (Collectee) or Achat (Deductible)
    public bool IsProrata { get; set; }
    public decimal? ProrataPercent { get; set; }
    public string? Regime { get; set; } // Encaissement, Debit
}

/// <summary>
/// Declaration status change history
/// </summary>
public class DeclarationHistory : TenantEntity
{
    public int DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }
    public string AncienStatut { get; set; } = string.Empty;
    public string NouveauStatut { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Commentaire { get; set; }
    public DateTime DateChangement { get; set; } = DateTime.UtcNow;
}
