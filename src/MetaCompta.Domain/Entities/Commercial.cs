using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

// ════════════════════════════════════════════════════════════════════════════════
//  DOCUMENTS COMMERCIAUX — Devis → Bon de Commande → Contrat → Facture
//  Cycle de vie complet avec traçabilité, versioning et audit
// ════════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Devis (Estimate) — Point d'entrée du cycle commercial.
/// Référence: DEV-YYYY-XXXX
/// </summary>
public class Devis : TenantEntity
{
    // ── Identification ──
    public string Reference { get; set; } = string.Empty; // DEV-2026-0001
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // ── Contenu ──
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; } // Si devise ≠ MAD

    // ── Dates ──
    public DateTime DateEmission { get; set; } = DateTime.UtcNow;
    public DateTime? DateValidite { get; set; } // Date d'expiration
    public DateTime? DateApprobation { get; set; }
    public DateTime? DateRejet { get; set; }

    // ── Statut ──
    /// <summary>Brouillon, Soumis, Approuve, Rejete, Expire, Annule</summary>
    public string Statut { get; set; } = RefCodes.StatutDevis.Brouillon;
    public string? MotifRejet { get; set; }

    // ── Montants (calculés depuis les lignes) ──
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }

    // ── Conditions ──
    public string? ConditionsParticulieres { get; set; }
    public string? NotesPourClient { get; set; }

    // ── Versioning ──
    public int Version { get; set; } = 1;
    public int? ParentDevisId { get; set; }
    public virtual Devis? ParentDevis { get; set; }

    // ── Assignation ──
    public string? CommercialId { get; set; }
    public virtual ApplicationUser? Commercial { get; set; }
    public string? ValidateurId { get; set; }
    public virtual ApplicationUser? Validateur { get; set; }

    // ── Documents joints ──
    public string? PdfUrl { get; set; }
    public string? PiecesJointesJson { get; set; } // JSON array of attachment URLs

    public string? Notes { get; set; }

    // ── Navigation ──
    public virtual ICollection<LigneDocumentCommercial> Lignes { get; set; } = new List<LigneDocumentCommercial>();
    public virtual ICollection<DocumentCommercialHistorique> Historique { get; set; } = new List<DocumentCommercialHistorique>();
    public virtual BonCommande? BonCommande { get; set; } // 0..1 relation if converted to BC
    public virtual ContratCommercial? ContratDirect { get; set; } // 0..1 relation if converted directly to Contrat
}

/// <summary>
/// Bon de Commande (Purchase Order) — Généré depuis un Devis approuvé.
/// Référence: PO-YYYY-XXXX
/// </summary>
public class BonCommande : TenantEntity
{
    // ── Identification ──
    public string Reference { get; set; } = string.Empty; // PO-2026-0001
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // ── Liaison obligatoire au Devis ──
    public int DevisId { get; set; }
    public virtual Devis? Devis { get; set; }

    // ── Contenu ──
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; }

    // ── Dates ──
    public DateTime DateEmission { get; set; } = DateTime.UtcNow;
    public DateTime? DateConfirmation { get; set; }
    public DateTime? DateApprobation { get; set; }

    // ── Statut ──
    /// <summary>Brouillon, Confirme, Approuve, Annule</summary>
    public string Statut { get; set; } = RefCodes.StatutBonCommande.Brouillon;

    // ── Montants ──
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }

    // ── Conditions ──
    public string? ConditionsParticulieres { get; set; }

    // ── Versioning ──
    public int Version { get; set; } = 1;

    // ── Assignation ──
    public string? ResponsableId { get; set; }
    public virtual ApplicationUser? Responsable { get; set; }

    public string? PdfUrl { get; set; }
    public string? PiecesJointesJson { get; set; }
    public string? Notes { get; set; }

    // ── Navigation ──
    public virtual ICollection<LigneDocumentCommercial> Lignes { get; set; } = new List<LigneDocumentCommercial>();
    public virtual ICollection<DocumentCommercialHistorique> Historique { get; set; } = new List<DocumentCommercialHistorique>();
    public virtual ContratCommercial? Contrat { get; set; } // 0..1
}

/// <summary>
/// Contrat Commercial — Généré depuis un Bon de Commande approuvé.
/// Référence: CTR-YYYY-XXXX
/// La signature est OBLIGATOIRE avant validation.
/// La validation ACTIVE automatiquement les services du client.
/// </summary>
public class ContratCommercial : TenantEntity
{
    // ── Identification ──
    public string Reference { get; set; } = string.Empty; // CTR-2026-0001
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // ── Liaison optionnelle au Bon de Commande (null si contrat direct ou depuis devis) ──
    public int? BonCommandeId { get; set; }
    public virtual BonCommande? BonCommande { get; set; }

    // ── Liaison optionnelle au Devis (conversion directe Devis → Contrat) ──
    public int? DevisId { get; set; }
    public virtual Devis? Devis { get; set; }

    // ── Contenu ──
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; }

    // ── Dates ──
    public DateTime DateEmission { get; set; } = DateTime.UtcNow;
    public DateTime? DateDebutContrat { get; set; }
    public DateTime? DateFinContrat { get; set; }
    public DateTime? DateSignature { get; set; }
    public DateTime? DateValidation { get; set; }

    // ── Statut ──
    /// <summary>Brouillon, EnCours, Signe, Confirme, Valide, Actif, Suspendu, Termine, Annule</summary>
    public string Statut { get; set; } = RefCodes.StatutContrat.Brouillon;

    // ── Signature ──
    public string? SignataireNom { get; set; }
    public string? SignataireFonction { get; set; }
    public string? SignatureUrl { get; set; } // URL du document signé
    public bool EstSigne { get; set; } = false;

    // ── Facturation ──
    /// <summary>Régie (Time & Material) ou Forfait (Fixed Price)</summary>
    public string ModeleFacturation { get; set; } = "Forfait";
    /// <summary>Mensuelle, Trimestrielle, Annuelle</summary>
    public string? FrequenceFacturation { get; set; }

    // ── Montants ──
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }

    // ── Conditions ──
    public string? ConditionsParticulieres { get; set; }
    public string? ClausesSpecifiques { get; set; }

    // ── Versioning ──
    public int Version { get; set; } = 1;

    // ── Assignation ──
    public string? ResponsableId { get; set; }
    public virtual ApplicationUser? Responsable { get; set; }
    public string? ValidateurId { get; set; }
    public virtual ApplicationUser? ValidateurUser { get; set; }

    public string? PdfUrl { get; set; }
    public string? ContratSigneUrl { get; set; }
    public string? PiecesJointesJson { get; set; }
    public string? Notes { get; set; }

    // ── Services fiduciaires activés (JSON: [{typeService, honoraires, autoGenerer}]) ──
    /// <summary>JSON array of enabled fiduciary services for this contract, each with optional per-service honoraires.</summary>
    public string? ServicesActivesJson { get; set; }

    // ── Navigation ──
    public virtual ICollection<ContratMembre> Membres { get; set; } = new List<ContratMembre>();
    public virtual ICollection<LigneDocumentCommercial> Lignes { get; set; } = new List<LigneDocumentCommercial>();
    public virtual ICollection<DocumentCommercialHistorique> Historique { get; set; } = new List<DocumentCommercialHistorique>();
    public virtual ICollection<Facture> Factures { get; set; } = new List<Facture>();
}

/// <summary>
/// Team member assigned to a commercial contract (organizational visibility).
/// </summary>
public class ContratMembre : TenantEntity
{
    public int ContratId { get; set; }
    public virtual ContratCommercial? Contrat { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    /// <summary>Optional label (e.g. Collaborateur, Superviseur).</summary>
    public string? RoleLabel { get; set; }
}

/// <summary>
/// Facture — Toujours liée à un Contrat validé.
/// Référence: INV-YYYY-XXXX
/// </summary>
public class Facture : TenantEntity
{
    // ── Identification ──
    public string Reference { get; set; } = string.Empty; // INV-2026-0001
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // ── Liaison optionnelle au Contrat (null si facture manuelle) ──
    public int? ContratId { get; set; }
    public virtual ContratCommercial? Contrat { get; set; }

    // ── Contenu ──
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; }

    // ── Période de facturation ──
    public DateTime? PeriodeDebut { get; set; }
    public DateTime? PeriodeFin { get; set; }

    // ── Dates ──
    public DateTime DateEmission { get; set; } = DateTime.UtcNow;
    public DateTime DateEcheance { get; set; }
    public DateTime? DateEnvoi { get; set; }
    public DateTime? DateVue { get; set; }

    // ── Statut ──
    /// <summary>Brouillon, Generee, Envoyee, Vue, PartiellementPayee, Payee, EnRetard, Annulee, Remboursee</summary>
    public string Statut { get; set; } = RefCodes.StatutFacture.Brouillon;

    // ── Montants ──
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal MontantPaye { get; set; }
    public decimal MontantRestant { get; set; }

    // ── Versioning ──
    public int Version { get; set; } = 1;

    // ── Assignation ──
    public string? EmetteurId { get; set; }
    public virtual ApplicationUser? Emetteur { get; set; }

    public string? PdfUrl { get; set; }
    public string? PiecesJointesJson { get; set; }
    public string? Notes { get; set; }

    // ── Navigation ──
    public virtual ICollection<LigneDocumentCommercial> Lignes { get; set; } = new List<LigneDocumentCommercial>();
    public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    public virtual ICollection<DocumentCommercialHistorique> Historique { get; set; } = new List<DocumentCommercialHistorique>();
}

/// <summary>
/// Ligne de document commercial — Utilisée par Devis, BonCommande, Contrat, Facture.
/// Chaque ligne peut être de type Service, Régie ou Forfait.
/// </summary>
public class LigneDocumentCommercial : TenantEntity
{
    // ── Polymorphic FK — exactement un doit être rempli ──
    public int? DevisId { get; set; }
    public virtual Devis? Devis { get; set; }
    public int? BonCommandeId { get; set; }
    public virtual BonCommande? BonCommande { get; set; }
    public int? ContratId { get; set; }
    public virtual ContratCommercial? Contrat { get; set; }
    public int? FactureId { get; set; }
    public virtual Facture? Facture { get; set; }

    // ── Identifiant de ligne source (pour traçabilité lors de la conversion) ──
    public int? LigneSourceId { get; set; }

    // ── Type de ligne ──
    /// <summary>Service, Régie (Time & Material), Forfait (Fixed Price)</summary>
    public string TypeLigne { get; set; } = "Service";

    // ── Référence au service fiduciaire ──
    public string? TypeService { get; set; } // TVA, IS, IR, CNSS, Paie, etc.
    public string? CodeService { get; set; }

    // ── Lien optionnel vers le ServiceContrat du client (service déclaratif) ──
    public int? ServiceContratId { get; set; }
    public virtual ServiceContrat? ServiceContrat { get; set; }

    // ── Contenu ──
    public int Ordre { get; set; } = 1; // Ordre d'affichage
    public string Description { get; set; } = string.Empty;
    public string? DescriptionDetaillee { get; set; }

    // ── Quantité & Prix ──
    public decimal Quantite { get; set; } = 1;
    public string? Unite { get; set; } // Heure, Jour, Mois, Forfait, Unité
    public decimal PrixUnitaireHT { get; set; }

    // ── Remise ──
    public decimal? RemisePourcentage { get; set; }
    public decimal? RemiseMontant { get; set; }

    // ── TVA ──
    public decimal TauxTVA { get; set; } = 20m; // 20%, 14%, 10%, 7%, 0%
    public decimal MontantTVA { get; set; }

    // ── Totaux ──
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }

    // ── Mode de facturation de la ligne (Mensuel, Trimestriel, Semestriel, Annuel, Ponctuel) ──
    public string? ModeFacturation { get; set; }

    // ── Pour Régie : suivi de consommation ──
    public decimal? QuantiteConsommee { get; set; }
    public bool? DejaFacture { get; set; } = false;
}

/// <summary>
/// Paiement — Paiements partiels ou complets liés à une facture.
/// </summary>
public class Paiement : TenantEntity
{
    public int FactureId { get; set; }
    public virtual Facture? Facture { get; set; }

    public string Reference { get; set; } = string.Empty; // PAY-2026-0001
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    public decimal Montant { get; set; }
    public string Devise { get; set; } = "MAD";

    /// <summary>Virement, Chèque, Carte, Espèces, Prélèvement</summary>
    public string ModePaiement { get; set; } = "Virement";

    public string? ReferenceBancaire { get; set; } // Numéro de virement/chèque
    public string? BanqueEmettrice { get; set; }
    public string? Notes { get; set; }

    /// <summary>Validé, EnAttente, Rejeté</summary>
    public string Statut { get; set; } = RefCodes.StatutPaiement.Valide;

    public string? PieceJointeUrl { get; set; }
    public string? EnregistreParId { get; set; }
    public virtual ApplicationUser? EnregistrePar { get; set; }
}

/// <summary>
/// Historique des actions sur les documents commerciaux.
/// Chaque changement de statut, modification ou action est tracé.
/// </summary>
public class DocumentCommercialHistorique : TenantEntity
{
    /// <summary>Devis, BonCommande, Contrat, Facture</summary>
    public string TypeDocument { get; set; } = string.Empty;
    public int DocumentId { get; set; }

    // ── Polymorphic FK (optionnel pour navigation) ──
    public int? DevisId { get; set; }
    public virtual Devis? Devis { get; set; }
    public int? BonCommandeId { get; set; }
    public virtual BonCommande? BonCommande { get; set; }
    public int? ContratId { get; set; }
    public virtual ContratCommercial? Contrat { get; set; }
    public int? FactureId { get; set; }
    public virtual Facture? Facture { get; set; }

    // ── Action ──
    public string Action { get; set; } = string.Empty; // Création, Modification, ChangementStatut, Conversion, Signature, Validation, Envoi, Paiement
    public string? AncienStatut { get; set; }
    public string? NouveauStatut { get; set; }
    public string? Commentaire { get; set; }

    // ── Acteur ──
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime DateAction { get; set; } = DateTime.UtcNow;

    // ── Données de version ──
    public int? VersionDocument { get; set; }
    public string? DonneesAvantJson { get; set; } // Snapshot avant modification
    public string? DonneesApresJson { get; set; } // Snapshot après modification
}
