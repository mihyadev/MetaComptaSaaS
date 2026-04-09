using MetaCompta.Domain.Common;

namespace MetaCompta.Application.DTOs;

// ===== Auth =====
public record LoginDto(string Email, string Password);
public record RegisterDto(string Email, string Password, string FirstName, string LastName, string? Role, int? CabinetId, int? ClientId);
public record TokenDto(string AccessToken, string RefreshToken, DateTime Expiration, UserInfoDto User);
public record UserInfoDto(string Id, string Email, string FullName, string? Role, int? CabinetId, string? CabinetName, string? ProfileImageUrl, string PreferredLanguage);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);

// ===== Client =====
public class ClientDto
{
    public int Id { get; set; }
    public string RaisonSociale { get; set; } = string.Empty;
    public string? ICE { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? NumeroCNSS { get; set; }
    public string? NumeroPatente { get; set; }
    public DateTime? DateDebutActivite { get; set; }
    public string? CodeActivite { get; set; }
    public string? FormeJuridique { get; set; }
    public string? SecteurActivite { get; set; }
    public DateTime? DateCreation { get; set; }
    public decimal? CapitalSocial { get; set; }
    public string? AdresseSiegeSocial { get; set; }
    public string? VilleSiegeSocial { get; set; }
    public string? TelephonePrincipal { get; set; }
    public string? EmailPrincipal { get; set; }
    public string? ContactNom { get; set; }
    public string? ContactFonction { get; set; }
    public string? ContactTelephone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ManagerPrincipalId { get; set; }
    public string? ManagerPrincipalName { get; set; }
    public string TypeTVA { get; set; } = RefCodes.TypeTva.Mensuelle;
    public int MoisClotureExercice { get; set; } = 12;
    public string Statut { get; set; } = RefCodes.StatutClient.Active;
    public DateTime? DateDebutContrat { get; set; }
    public string? NotesInternes { get; set; }
    public int EmployeeCount { get; set; }
    public int DeclarationCount { get; set; }
    public List<ServiceContratDto> ServicesFiduciaires { get; set; } = new();
}

// ===== Dossier Fiduciaire / Service Contrat =====
public class ServiceContratDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string TypeService { get; set; } = string.Empty;
    public string LibelleService { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public string Frequence { get; set; } = "Mensuelle";
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int? ExerciceFiscal { get; set; }
    public DateTime? ExerciceDebut { get; set; }
    public DateTime? ExerciceFin { get; set; }
    public int? JourLimiteDeclaration { get; set; }
    public int? DelaiDeclarationMois { get; set; }
    public string? ConfigurationJson { get; set; }
    public decimal? HonorairesMensuels { get; set; }
    public decimal? HonorairesAnnuels { get; set; }
    public bool Actif { get; set; } = true;
    public bool AutoGenererDeclarations { get; set; } = true;
    public string? ResponsableId { get; set; }
    public string? Notes { get; set; }
    // Computed
    public bool EstActifAujourdhui => Actif && DateDebut <= DateTime.UtcNow && (DateFin == null || DateFin >= DateTime.UtcNow);
}

public class ServiceContratCreateDto
{
    public string TypeService { get; set; } = string.Empty;
    public string Frequence { get; set; } = "Mensuelle";
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int? ExerciceFiscal { get; set; }
    public string? ConfigurationJson { get; set; }
    public decimal? HonorairesMensuels { get; set; }
    public decimal? HonorairesAnnuels { get; set; }
    public bool Actif { get; set; } = true;
    public bool AutoGenererDeclarations { get; set; } = true;
    public string? ResponsableId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>DTO for auto-generating declarations from active service contracts</summary>
public class GenerateDeclarationsRequestDto
{
    public int ClientId { get; set; }
    public int ExerciceFiscal { get; set; }
    public List<string>? TypeServices { get; set; } // null = all active services
}

public class GenerateDeclarationsResultDto
{
    public int TotalGenerated { get; set; }
    public List<DeclarationDto> Declarations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ClientCreateDto
{
    public string RaisonSociale { get; set; } = string.Empty;
    public string? ICE { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? NumeroCNSS { get; set; }
    public string? NumeroPatente { get; set; }
    public DateTime? DateDebutActivite { get; set; }
    public string? CodeActivite { get; set; }
    public string? FormeJuridique { get; set; }
    public string? SecteurActivite { get; set; }
    public DateTime? DateCreation { get; set; }
    public decimal? CapitalSocial { get; set; }
    public string? AdresseSiegeSocial { get; set; }
    public string? VilleSiegeSocial { get; set; }
    public string? TelephonePrincipal { get; set; }
    public string? EmailPrincipal { get; set; }
    public string? ContactNom { get; set; }
    public string? ContactFonction { get; set; }
    public string? ContactTelephone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ManagerPrincipalId { get; set; }
    public string TypeTVA { get; set; } = "Mensuelle";
    public int MoisClotureExercice { get; set; } = 12;
    public DateTime? DateDebutContrat { get; set; }
    public string? NotesInternes { get; set; }
    public List<ServiceContratCreateDto> ServicesFiduciaires { get; set; } = new();
}

// ===== Employee =====
public class EmployeeDto
{
    public int Id { get; set; }
    public string CIN { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string FullName => $"{Prenom} {Nom}";
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime DateEmbauche { get; set; }
    public DateTime? DateSortie { get; set; }
    public string? TypeContrat { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public decimal SalaireBase { get; set; }
    public string Statut { get; set; } = RefCodes.StatutEmploye.Actif;
    public string? SituationFamiliale { get; set; }
    public int NombreEnfantsACharge { get; set; }
    public string? TelephonePortable { get; set; }
    public string? Email { get; set; }
    public string? ModePaiement { get; set; }
    public string? RIB { get; set; }
    public string? NomBanque { get; set; }
}

public class EmployeeCreateDto
{
    public string CIN { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Adresse { get; set; }
    public string? TelephonePortable { get; set; }
    public string? Email { get; set; }
    public string? SituationFamiliale { get; set; }
    public int NombreEnfantsACharge { get; set; }
    public string? Sexe { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime DateEmbauche { get; set; }
    public string? TypeContrat { get; set; }
    public int? DureeContratMois { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public decimal SalaireBase { get; set; }
    public decimal? PrimeAnciennete { get; set; }
    public decimal? IndemniteTransport { get; set; }
    public string? ModePaiement { get; set; }
    public string? RIB { get; set; }
    public string? NomBanque { get; set; }
    public string? AgenceBancaire { get; set; }
}

// ===== Declaration =====
public class DeclarationDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Periode { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public DateTime DateLimite { get; set; }
    public string Statut { get; set; } = RefCodes.StatutDeclaration.Brouillon;
    public DateTime? DateSoumission { get; set; }
    public string? AssistantName { get; set; }
    public string? ValidateurName { get; set; }
    public decimal? MontantDu { get; set; }
    public decimal? MontantCredit { get; set; }
    public string? PdfUrl { get; set; }
    public int DaysUntilDeadline { get; set; }
}

public class DeclarationTVADto
{
    public int DeclarationId { get; set; }
    public decimal CAHT_20 { get; set; }
    public decimal CAHT_14 { get; set; }
    public decimal CAHT_10 { get; set; }
    public decimal CAHT_7 { get; set; }
    public decimal CAExonere { get; set; }
    public decimal TVARecuperableCharges { get; set; }
    public decimal TVARecuperableImmobilisations { get; set; }
    // Computed (read-only, filled by API)
    public decimal TVA_20 { get; set; }
    public decimal TVA_14 { get; set; }
    public decimal TVA_10 { get; set; }
    public decimal TVA_7 { get; set; }
    public decimal TVACollecteeTotale { get; set; }
    public decimal TVARecuperableTotale { get; set; }
    public decimal CreditPrecedent { get; set; }
    public decimal TVANette { get; set; }
    public decimal TVAaPayer { get; set; }
    public decimal CreditTVA { get; set; }
    // Invoice lines (lignes de factures)
    public List<TVALigneFactureDto> LignesFactures { get; set; } = new();
}

/// <summary>
/// Request DTO for saving only TVA manual adjustments (Immobilisations + Credit Precedent).
/// </summary>
public class TVAAjustementsRequest
{
    public decimal TVARecuperableImmobilisations { get; set; }
    public decimal CreditPrecedent { get; set; }
}

// ===== TVA Ligne Facture =====
public class TVALigneFactureDto
{
    public int Id { get; set; }
    public int DeclarationTVAId { get; set; }
    public string TypePiece { get; set; } = "Facture";
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
    public string TypeOperation { get; set; } = "Vente";
    public bool IsProrata { get; set; }
    public decimal? ProrataPercent { get; set; }
    public string? Regime { get; set; }
}

// ===== Payroll =====
public class PayrollPeriodDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Status { get; set; } = RefCodes.StatutPaie.Brouillon;
    public int TotalEmployees { get; set; }
    public decimal? TotalBrutGlobal { get; set; }
    public decimal? TotalNetPay { get; set; }
    public decimal? TotalChargesPatronales { get; set; }
    public DateTime? DateValidation { get; set; }
}

public class PaySlipDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Matricule { get; set; }
    public string? PeriodLabel { get; set; }
    public decimal SalaireBase { get; set; }
    public decimal PrimesFixesTotales { get; set; }
    public decimal HeuresSupp25 { get; set; }
    public decimal HeuresSupp50 { get; set; }
    public decimal HeuresSupp100 { get; set; }
    public decimal MontantHeuresSupp { get; set; }
    public decimal PrimesExceptionnelles { get; set; }
    public decimal AvancesSalaire { get; set; }
    public decimal AbsencesNonRemunerees { get; set; }
    public decimal SalaireBrut { get; set; }
    public decimal CotisationCNSS { get; set; }
    public decimal CotisationAMO { get; set; }
    public decimal CotisationCIMR { get; set; }
    public decimal SalaireNetImposable { get; set; }
    public decimal IR { get; set; }
    public decimal SalaireNet { get; set; }
    public decimal CNSSPatronal { get; set; }
    public decimal AMOPatronal { get; set; }
    public decimal FormationProfessionnelle { get; set; }
    public decimal TotalChargesPatronales { get; set; }
    public string? PdfUrl { get; set; }
}

public class PaySlipVariablesDto
{
    public int EmployeeId { get; set; }
    public decimal HeuresSupp25 { get; set; }
    public decimal HeuresSupp50 { get; set; }
    public decimal HeuresSupp100 { get; set; }
    public decimal PrimesExceptionnelles { get; set; }
    public decimal IndemnitesExceptionnelles { get; set; }
    public decimal AvancesSalaire { get; set; }
    public decimal AbsencesNonRemunerees { get; set; } // in days
}

// ===== Document =====
public class DocumentDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Categorie { get; set; } = string.Empty;
    public string NomFichier { get; set; } = string.Empty;
    public string? TypeDocument { get; set; }
    public string TailleFichier { get; set; } = string.Empty;
    public DateTime? DateDocument { get; set; }
    public DateTime DateUpload { get; set; }
    public int? ClientId { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientName { get => ClientNom; set => ClientNom = value; }
    public string Statut { get; set; } = "EnAttente";
    public bool EstPartage { get; set; }
    public bool PartageExtranet { get => EstPartage; set => EstPartage = value; }
    public int Version { get; set; }
    public string? UploadePar { get; set; }
    public string? UploadedByUserName { get => UploadePar; set => UploadePar = value; }
    public DateTime CreatedAt { get; set; }
}

// ===== Notification =====
public class NotificationDto
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string? Module { get; set; }
    public string? Url { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== Task =====
public class TaskItemDto
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ClientId { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientName { get => ClientNom; set => ClientNom = value; }
    public string? AssigneA { get; set; }
    public string? AssignedToUserName { get => AssigneA; set => AssigneA = value; }
    public string? AssignedByUserName { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string Priorite { get; set; } = "Moyenne";
    public string Statut { get; set; } = "AFaire";
    public DateTime DateCreation { get; set; }
    public int CommentCount { get; set; }
}

// ===== Dashboard =====
public class DashboardDto
{
    public int TotalClientsActifs { get; set; }
    public int DeclarationsMoisEnCours { get; set; }
    public int DeclarationsEnAttente { get; set; }
    public int DeclarationsSoumises { get; set; }
    public int DocumentsEnAttente { get; set; }
    public int TachesEnCours { get; set; }
    public List<DeclarationDto> ProchainsDelais { get; set; } = new();
    public List<NotificationDto> NotificationsRecentes { get; set; } = new();
    public DashboardChartsDto Charts { get; set; } = new();
}

public class DashboardChartsDto
{
    public object? DeclarationsEvolution { get; set; } // for ECharts
    public object? ClientsDistribution { get; set; }
    public object? TeamPerformance { get; set; }
    public object? DeclarationStatusGlobal { get; set; }
}

// ===== Paged result =====
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ===== Reference Data DTO =====
public class ReferenceDataDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public decimal? NumericValue { get; set; }
    public int SortOrder { get; set; }
    public string? Couleur { get; set; }
    public string? CssClass { get; set; }
    public string? Icone { get; set; }
}

// ===== Comptabilité (Accounting) =====
public class EcritureComptableDto
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientName { get => ClientNom; set => ClientNom = value; }
    public string Journal { get; set; } = string.Empty;
    public string NumeroPiece { get; set; } = string.Empty;
    public string? NumPiece { get => NumeroPiece; set => NumeroPiece = value ?? string.Empty; }
    public DateTime DateEcriture { get; set; }
    public string NumCompte { get; set; } = string.Empty;
    public string LibelleCompte { get; set; } = string.Empty;
    public string? Libelle { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal TotalDebit { get => Debit; set => Debit = value; }
    public decimal TotalCredit { get => Credit; set => Credit = value; }
    public string? Reference { get; set; }
    public string Statut { get; set; } = RefCodes.StatutDeclaration.Brouillon;
    public DateTime CreatedAt { get; set; }
}

public class EcritureCreateDto
{
    public int ClientId { get; set; }
    public string Journal { get; set; } = string.Empty;
    public DateTime DateEcriture { get; set; }
    public string? Libelle { get; set; }
    public string? NumeroPiece { get; set; }
    public string? Reference { get; set; }
    public List<LigneEcritureDto> Lignes { get; set; } = new();
}

public class LigneEcritureDto
{
    public string NumCompte { get; set; } = string.Empty;
    public string LibelleCompte { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class JournalDto
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Libelle { get => Nom; set => Nom = value; }
    public string Type { get; set; } = string.Empty;
    public int NombreEcritures { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class GrandLivreDto
{
    public string NumeroCompte { get; set; } = string.Empty;
    public string NumCompte { get => NumeroCompte; set => NumeroCompte = value; }
    public string LibelleCompte { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Solde => TotalDebit - TotalCredit;
    public decimal SoldeDebiteur { get; set; }
    public decimal SoldeCrediteur { get; set; }
    public List<EcritureComptableDto> Ecritures { get; set; } = new();
}

public class BalanceDto
{
    public string NumeroCompte { get; set; } = string.Empty;
    public string NumCompte { get => NumeroCompte; set => NumeroCompte = value; }
    public string LibelleCompte { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal SoldeDebiteur { get; set; }
    public decimal SoldeCrediteur { get; set; }
}

public class BalanceAgeeDto
{
    public string NumeroCompte { get; set; } = string.Empty;
    public string NumCompte { get => NumeroCompte; set => NumeroCompte = value; }
    public string NomTiers { get; set; } = string.Empty;
    public string Tiers { get => NomTiers; set => NomTiers = value; }
    public decimal Total { get; set; }
    public decimal Tranche0_30 { get; set; }
    public decimal Courant { get => Tranche0_30; set => Tranche0_30 = value; }
    public decimal Tranche30_60 { get; set; }
    public decimal Tranche60_90 { get; set; }
    public decimal TrancheSuperieur90 { get; set; }
    public decimal TrancheOver90 { get => TrancheSuperieur90; set => TrancheSuperieur90 = value; }
}

public class RapprochementBancaireDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientName { get => ClientNom; set => ClientNom = value; }
    public string CompteBancaire { get; set; } = string.Empty;
    public string Periode { get; set; } = string.Empty;
    public decimal SoldeCompta { get; set; }
    public decimal SoldeComptable { get => SoldeCompta; set => SoldeCompta = value; }
    public decimal SoldeBanque { get; set; }
    public decimal SoldeBancaire { get => SoldeBanque; set => SoldeBanque = value; }
    public decimal Ecart { get; set; }
    public bool EstRapproche { get; set; }
    public string Statut { get; set; } = "EnCours";
    public int NombreLignesNonRapprochees { get; set; }
    public DateTime DateRapprochement { get; set; }
}

// ===== Juridique (Legal) =====
public class DossierJuridiqueDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientName { get => ClientNom; set => ClientNom = value; }
    public string TypeDossier { get; set; } = string.Empty;
    public string? Type { get => TypeDossier; set => TypeDossier = value ?? string.Empty; }
    public string Reference { get; set; } = string.Empty;
    public string? NumeroReference { get => Reference; set => Reference = value ?? string.Empty; }
    public string Objet { get; set; } = string.Empty;
    public string Titre { get => Objet; set => Objet = value; }
    public string? Description { get; set; }
    public string Statut { get; set; } = "EnCours";
    public DateTime DateOuverture { get; set; }
    public DateTime DateCreation { get => DateOuverture; set => DateOuverture = value; }
    public DateTime? DateCloture { get; set; }
    public DateTime? DateDepot { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string? Responsable { get; set; }
    public string? ResponsableName { get => Responsable; set => Responsable = value; }
    public decimal Montant { get; set; }
    public string? Notes { get; set; }
    public List<string>? Documents { get; set; }
}

public class DossierJuridiqueCreateDto
{
    public int ClientId { get; set; }
    public string TypeDossier { get; set; } = string.Empty;
    public string? Type { get => TypeDossier; set => TypeDossier = value ?? string.Empty; }
    public string Objet { get; set; } = string.Empty;
    public string Titre { get => Objet; set => Objet = value; }
    public string? Description { get; set; }
    public string? Responsable { get; set; }
    public decimal Montant { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string? Notes { get; set; }
}

// ===== Planning & Calendar =====
public class PlanningEventDto
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string Type { get; set; } = string.Empty;       // EcheanceFiscale, Reunion, Tache, Rappel
    public string? Couleur { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsRecurrent { get; set; }
    public string? Statut { get; set; }
}

// ===== Cabinet (Firm management) =====
public class CabinetEmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NomComplet => $"{FirstName} {LastName}";
    public string FullName => NomComplet;
    public string? Role { get; set; }
    public string? Telephone { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public string? Equipe { get => Departement; set => Departement = value; }
    public string? CIN { get; set; }
    public string? Adresse { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Sexe { get; set; }
    public string? SituationFamiliale { get; set; }
    public string? NumeroCNSS { get; set; }
    public string? TypeContrat { get; set; }
    public bool EstActif { get; set; }
    public bool IsActive { get => EstActif; set => EstActif = value; }
    public DateTime? DateEmbauche { get; set; }
    public DateTime? DerniereConnexion { get; set; }
    public int ClientsAssignes { get; set; }
    public int TachesEnCours { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string PreferredLanguage { get; set; } = "fr-FR";
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for creating a new cabinet employee (BackOffice user).
/// </summary>
public class CabinetEmployeeCreateDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CIN { get; set; }
    public string? Telephone { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Adresse { get; set; }
    public string? Sexe { get; set; }
    public string? SituationFamiliale { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime? DateEmbauche { get; set; }
    public string? TypeContrat { get; set; }
    public string Role { get; set; } = "Assistant";
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing cabinet employee.
/// </summary>
public class CabinetEmployeeUpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CIN { get; set; }
    public string? Telephone { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Adresse { get; set; }
    public string? Sexe { get; set; }
    public string? SituationFamiliale { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime? DateEmbauche { get; set; }
    public string? TypeContrat { get; set; }
    public string Role { get; set; } = "Assistant";
    public bool IsActive { get; set; } = true;
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for editing cabinet information.
/// </summary>
public class CabinetEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ICE { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

/// <summary>
/// DTO for the cabinet detail view.
/// </summary>
public class CabinetDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ICE { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EmployeeCount { get; set; }
    public int ClientCount { get; set; }
    public string? SubscriptionPlanName { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

/// <summary>
/// DTO for the user profile / claims display.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Role { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string PreferredLanguage { get; set; } = "fr-FR";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nom => Name;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public int NombreUtilisateurs => UserCount;
    public List<string> Permissions { get; set; } = new();
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Utilisateur => UserName;
    public string Action { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? AdresseIP => IpAddress;
    public DateTime CreatedAt { get; set; }
    public DateTime DateAction => CreatedAt;
}

// ===== Rapports (Reports) =====
public class RapportDto
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;       // Financier, Fiscal, Social, Activite, Personnalise
    public string? Description { get; set; }
    public DateTime DateGeneration { get; set; }
    public string Periode { get; set; } = string.Empty;
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientNom => ClientName;
    public string Statut { get; set; } = "Genere";
    public string? PdfUrl { get; set; }
    public string? GenerePar { get; set; }
    public string Format { get; set; } = "PDF";
    public string? TailleFichier { get; set; }
}

// ===== Paramètres (Settings) =====
public class SettingsDto
{
    public int Id { get; set; }
    public string Categorie { get; set; } = string.Empty;
    public string Cle { get; set; } = string.Empty;
    public string? Valeur { get; set; }
    public string? Description { get; set; }
    public string TypeValeur { get; set; } = "string";
}

public class DocumentTemplateDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Categorie => Type;
    public string? Description { get; set; }
    public string? Contenu { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DerniereModification { get; set; }
    public DateTime DateModification => DerniereModification;
}

// ===== Fiscal States (Déclarations & États) =====
public class EtatFiscalDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientNom => ClientName;
    public string Type { get; set; } = string.Empty;   // Etat9421, LiasseFiscale, Bilan, CPC, Amortissements, EtatSynthese
    public string TypeEtat => Type;
    public string Periode { get; set; } = string.Empty;
    public int Exercice { get; set; }
    public string Statut { get; set; } = RefCodes.StatutDeclaration.Brouillon;
    public DateTime? DateGeneration { get; set; }
    public DateTime? DateValidation { get; set; }
    public string? PdfUrl { get; set; }
    public string? GenerePar { get; set; }
    public string? ValidePar { get; set; }
    public decimal? MontantTotal { get; set; }
    public string? Reference { get; set; }
    public string? Donnees { get; set; }
}

// ===== Create Declaration DTO =====
public class CreateDeclarationDto
{
    public int ClientId { get; set; }
    public string Type { get; set; } = string.Empty; // TVA, IS, IR, CNSS
    public string? Periode { get; set; } // optional override "2026-01", "2026-Q1", "2026"
    public int? AnneeFiscale { get; set; } // for IS/IR annual declarations
}

// ===== Fiscalité Extended =====
public class DeclarationISDto
{
    public int DeclarationId { get; set; }
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
    public decimal Acompte1 { get; set; }
    public decimal Acompte2 { get; set; }
    public decimal Acompte3 { get; set; }
    public decimal Acompte4 { get; set; }
    // Computed helpers
    public decimal CotisationMinimale { get; set; }
    public decimal ISAPayer => Math.Max(0, ISNet);
}

public class DeclarationIRDto
{
    public int DeclarationId { get; set; }
    public decimal RevenuBrutGlobal { get; set; }
    public decimal FraisProfessionnels { get; set; }
    public decimal Deductions { get; set; }
    public decimal RevenuNetImposable { get; set; }
    public decimal IRBrut { get; set; }
    public decimal ChargesFamille { get; set; }
    public decimal IRNet { get; set; }
    public int NombrePersonnesACharge { get; set; }
    public decimal DeductionPersonnesACharge { get; set; }
    public decimal TauxIR { get; set; }
    public decimal SommeADeduire { get; set; }
}

public class DeclarationCNSSDto
{
    public int DeclarationId { get; set; }
    public decimal TotalSalairesBruts { get; set; }
    public decimal TotalCotisationSalariale { get; set; }
    public decimal TotalCotisationPatronale { get; set; }
    public decimal TotalAMOSalariale { get; set; }
    public decimal TotalAMOPatronale { get; set; }
    public decimal TotalFormationProfessionnelle { get; set; }
    public decimal MontantTotal { get; set; }
    public int NombreEmployes { get; set; }
    public string? DetailEmployesJson { get; set; }
}

// ===== API Response =====
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new() { Success = true, Data = data, Message = message };
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new() { Success = false, Message = message, Errors = errors };
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW ENGINE — DTOs                              ║
// ╚══════════════════════════════════════════════════════════════════════════╝

// ===== Department =====
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? HeadOfDepartmentUserId { get; set; }
    public string? HeadOfDepartmentName { get; set; }
    public int MemberCount { get; set; }
    public List<DepartmentMemberDto> Members { get; set; } = new();
}

public class DepartmentCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeadOfDepartmentUserId { get; set; }
}

public class DepartmentMemberDto
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public bool IsLead { get; set; }
    public DateTime JoinedAt { get; set; }
}

// ===== Workflow Template =====
public class WorkflowTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DeclarationType { get; set; } = "All";
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public int StepCount { get; set; }
    public int InstanceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkflowStepTemplateDto> Steps { get; set; } = new();
}

public class WorkflowTemplateCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DeclarationType { get; set; } = "All";
    public int? ClientId { get; set; }
    public bool IsDefault { get; set; } = true;
    public List<WorkflowStepTemplateCreateDto> Steps { get; set; } = new();
}

public class WorkflowStepTemplateDto
{
    public int Id { get; set; }
    public int WorkflowTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string AssignmentType { get; set; } = "Employee";
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedToRoleName { get; set; }
    public int? AssignedToDepartmentId { get; set; }
    public string? AssignedToDepartmentName { get; set; }
    public bool IsClientStep { get; set; }
    public string? RequiredPermission { get; set; }
    public int? SLAHours { get; set; }
    public bool IsRequired { get; set; }
    public bool IsBlocking { get; set; }
    public bool AutoCreateTask { get; set; }
    public string TaskType { get; set; } = "Standard";
    public string TaskPriority { get; set; } = "Moyenne";
    public bool AutoNotify { get; set; }
    public string? ValidationRulesJson { get; set; }
    public string? DeclarationStatusOnComplete { get; set; }
    public string? DeclarationStatusOnEntry { get; set; }

    // ── Action Engine ──
    public string ActionOnEntry { get; set; } = "None";
    public string ActionOnComplete { get; set; } = "None";
    public string? ActionConfigJson { get; set; }

    // ── CRON / Scheduling ──
    public string? CronExpression { get; set; }
    public string? CronDescription { get; set; }
    public bool IsScheduled { get; set; }

    // ── Retry / Relance ──
    public int MaxRetries { get; set; }
    public int? RetryIntervalHours { get; set; }
    public string? OnMaxRetriesAction { get; set; }

    // ── Routing / Pipeline ──
    public string RoutingMode { get; set; } = "Sequential";
    public string? RoutingConditionsJson { get; set; }
    public int? GotoStepOrder { get; set; }

    // ── Trigger ──
    public string TriggerSource { get; set; } = "Manual";

    // ── Employee Affectation per Service ──
    public bool UseServiceContratAffectation { get; set; }
    public string? ServiceContratTypeCode { get; set; }
}

public class WorkflowStepTemplateCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public string StepType { get; set; } = "Initialisation";
    public string AssignmentType { get; set; } = "Employee";
    public string? AssignedToUserId { get; set; }
    public string? AssignedToRoleName { get; set; }
    public int? AssignedToDepartmentId { get; set; }
    public bool IsClientStep { get; set; }
    public string? RequiredPermission { get; set; }
    public int? SLAHours { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsBlocking { get; set; } = true;
    public bool AutoCreateTask { get; set; } = true;
    public string TaskType { get; set; } = "Standard";
    public string TaskPriority { get; set; } = "Moyenne";
    public bool AutoNotify { get; set; } = true;
    public string? ValidationRulesJson { get; set; }
    public string? DeclarationStatusOnComplete { get; set; }
    public string? DeclarationStatusOnEntry { get; set; }

    // ── Action Engine ──
    public string ActionOnEntry { get; set; } = "None";
    public string ActionOnComplete { get; set; } = "None";
    public string? ActionConfigJson { get; set; }

    // ── CRON / Scheduling ──
    public string? CronExpression { get; set; }
    public string? CronDescription { get; set; }
    public bool IsScheduled { get; set; }

    // ── Retry / Relance ──
    public int MaxRetries { get; set; } = 0;
    public int? RetryIntervalHours { get; set; }
    public string? OnMaxRetriesAction { get; set; }

    // ── Routing / Pipeline ──
    public string RoutingMode { get; set; } = "Sequential";
    public string? RoutingConditionsJson { get; set; }
    public int? GotoStepOrder { get; set; }

    // ── Trigger ──
    public string TriggerSource { get; set; } = "Manual";

    // ── Employee Affectation per Service ──
    public bool UseServiceContratAffectation { get; set; }
    public string? ServiceContratTypeCode { get; set; }
}

// ===== Workflow Instance =====
public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int WorkflowTemplateId { get; set; }
    public string? WorkflowTemplateName { get; set; }
    public int? DeclarationId { get; set; }
    public string? DeclarationType { get; set; }
    public string? DeclarationPeriode { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public int CurrentStepOrder { get; set; }
    public string? CurrentStepName { get; set; }
    public string Status { get; set; } = "Active";
    public string? StartedByUserName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public double ProgressPercent => TotalSteps > 0 ? Math.Round((double)CompletedSteps / TotalSteps * 100, 1) : 0;
    public List<WorkflowStepInstanceDto> StepInstances { get; set; } = new();
    public List<WorkflowHistoryDto> RecentHistory { get; set; } = new();
}

public class WorkflowStepInstanceDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public int WorkflowStepTemplateId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? CompletedByUserName { get; set; }

    // Context fields (Client / Declaration)
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public int? DeclarationId { get; set; }
    public string? DeclarationType { get; set; }
    public string? AssignmentType { get; set; }

    public DateTime? ActivatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string? BlockReason { get; set; }
    public int? TaskItemId { get; set; }
    public bool IsClientStep { get; set; }
    public bool IsBlocking { get; set; }
    public bool IsRequired { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != "Completed" && Status != "Skipped";

    // ── Retry / Relance ──
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? TriggerSource { get; set; }
    public int? RoutedToStepOrder { get; set; }

    // ── From Template (for display) ──
    public string ActionOnEntry { get; set; } = "None";
    public string ActionOnComplete { get; set; } = "None";
    public int MaxRetries { get; set; }
    public string RoutingMode { get; set; } = "Sequential";
    public string? CronExpression { get; set; }
    public string? CronDescription { get; set; }
}

public class WorkflowHistoryDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public int? WorkflowStepInstanceId { get; set; }
    public string? StepName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Comment { get; set; }
    public string? UserName { get; set; }
    public DateTime ActionDate { get; set; }
}

// ===== Workflow Operations =====
public class StartWorkflowDto
{
    public int? DeclarationId { get; set; }
    public int ClientId { get; set; }
    public string? DeclarationType { get; set; }
    public int? WorkflowTemplateId { get; set; }   // Override: use specific template
}

public class AdvanceStepDto
{
    public string? Notes { get; set; }
    public string? Comment { get; set; }
    public bool ForceSkip { get; set; } = false;    // Skip non-required step
}

public class ReassignStepDto
{
    public string NewAssigneeUserId { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

// ===== Client Workflow Config =====
public class ClientWorkflowConfigDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string DeclarationType { get; set; } = "All";
    public int? WorkflowTemplateId { get; set; }
    public string? WorkflowTemplateName { get; set; }
    public bool ClientValidationEnabled { get; set; } = true;
    public bool DocumentRequestEnabled { get; set; } = true;
    public decimal SLAMultiplier { get; set; } = 1.0m;
    public bool AutoStartWorkflow { get; set; } = true;
    public string? PrimaryAssigneeUserId { get; set; }
    public string? PrimaryAssigneeName { get; set; }
    public bool IsActive { get; set; }
}

public class ClientWorkflowConfigCreateDto
{
    public int ClientId { get; set; }
    public string DeclarationType { get; set; } = "All";
    public int? WorkflowTemplateId { get; set; }
    public bool ClientValidationEnabled { get; set; } = true;
    public bool DocumentRequestEnabled { get; set; } = true;
    public decimal SLAMultiplier { get; set; } = 1.0m;
    public bool AutoStartWorkflow { get; set; } = true;
    public string? PrimaryAssigneeUserId { get; set; }
}

// ===== Workflow Dashboard / Summary =====
public class WorkflowDashboardDto
{
    public int ActiveWorkflows { get; set; }
    public int CompletedThisMonth { get; set; }
    public int OverdueSteps { get; set; }
    public int PendingMyAction { get; set; }
    public int WaitingForClient { get; set; }
    public List<WorkflowInstanceDto> RecentWorkflows { get; set; } = new();
    public List<WorkflowStepInstanceDto> MyPendingSteps { get; set; } = new();
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> StepTypeDistribution { get; set; } = new();
}

// ════════════════════════════════════════════════════════════════════════════════
//  DOCUMENTS COMMERCIAUX — DTOs
//  Devis → Bon de Commande → Contrat → Facture
//  Règle : AUCUNE mention de "Honoraires" nulle part
// ════════════════════════════════════════════════════════════════════════════════

// ===== Ligne Document Commercial =====
public class LigneDocumentCommercialDto
{
    public int Id { get; set; }
    public string TypeLigne { get; set; } = "Service"; // Service, Régie, Forfait
    public string? TypeService { get; set; }
    public string? CodeService { get; set; }
    public int? ServiceContratId { get; set; }
    public string? ServiceContratLibelle { get; set; }
    public int Ordre { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionDetaillee { get; set; }
    public decimal Quantite { get; set; } = 1;
    public string? Unite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal? RemisePourcentage { get; set; }
    public decimal? RemiseMontant { get; set; }
    public decimal TauxTVA { get; set; } = 20m;
    public decimal MontantTVA { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal? QuantiteConsommee { get; set; }
    public bool? DejaFacture { get; set; }
    public int? LigneSourceId { get; set; }
}

public class LigneDocumentCommercialCreateDto
{
    public string TypeLigne { get; set; } = "Service";
    public string? TypeService { get; set; }
    public string? CodeService { get; set; }
    public int? ServiceContratId { get; set; }
    public int Ordre { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionDetaillee { get; set; }
    public decimal Quantite { get; set; } = 1;
    public string? Unite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal? RemisePourcentage { get; set; }
    public decimal? RemiseMontant { get; set; }
    public decimal TauxTVA { get; set; } = 20m;
    public int? LigneSourceId { get; set; }
}

// ===== Devis =====
public class DevisDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; }
    public DateTime DateEmission { get; set; }
    public DateTime? DateValidite { get; set; }
    public DateTime? DateApprobation { get; set; }
    public string Statut { get; set; } = RefCodes.StatutDevis.Brouillon;
    public string? MotifRejet { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }
    public string? ConditionsParticulieres { get; set; }
    public string? NotesPourClient { get; set; }
    public int Version { get; set; }
    public string? CommercialId { get; set; }
    public string? CommercialName { get; set; }
    public string? PdfUrl { get; set; }
    public string? Notes { get; set; }
    public bool PeutConvertir => Statut == RefCodes.StatutDevis.Approuve;
    public int NombreLignes { get; set; }
    public List<LigneDocumentCommercialDto> Lignes { get; set; } = new();
    public List<DocumentCommercialHistoriqueDto> Historique { get; set; } = new();
}

public class DevisCreateDto
{
    public int ClientId { get; set; }
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public decimal? TauxChange { get; set; }
    public DateTime? DateValidite { get; set; }
    public string? ConditionsParticulieres { get; set; }
    public string? NotesPourClient { get; set; }
    public string? CommercialId { get; set; }
    public string? Notes { get; set; }
    public List<LigneDocumentCommercialCreateDto> Lignes { get; set; } = new();
}

// ===== Bon de Commande =====
public class BonCommandeDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public int DevisId { get; set; }
    public string? DevisReference { get; set; }
    public string? Objet { get; set; }
    public string Devise { get; set; } = "MAD";
    public DateTime DateEmission { get; set; }
    public string Statut { get; set; } = RefCodes.StatutBonCommande.Brouillon;
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }
    public int Version { get; set; }
    public string? PdfUrl { get; set; }
    public string? Notes { get; set; }
    public List<LigneDocumentCommercialDto> Lignes { get; set; } = new();
    public List<DocumentCommercialHistoriqueDto> Historique { get; set; } = new();
}

// ===== Contrat Commercial =====
public class ContratCommercialDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public int? BonCommandeId { get; set; }
    public string? BonCommandeReference { get; set; }
    public int? DevisId { get; set; }
    public string? DevisReference { get; set; }
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public DateTime DateEmission { get; set; }
    public DateTime? DateDebutContrat { get; set; }
    public DateTime? DateFinContrat { get; set; }
    public DateTime? DateSignature { get; set; }
    public DateTime? DateValidation { get; set; }
    public string Statut { get; set; } = RefCodes.StatutContrat.Brouillon;
    public bool EstSigne { get; set; }
    public string? SignataireNom { get; set; }
    public string ModeleFacturation { get; set; } = RefCodes.ModeleFacturation.Forfait;
    public string? FrequenceFacturation { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }
    public int Version { get; set; }
    public string? PdfUrl { get; set; }
    public string? ContratSigneUrl { get; set; }
    public string? Notes { get; set; }
    public bool PeutValider => EstSigne && Statut == RefCodes.StatutContrat.Signe;
    public int NombreFactures { get; set; }
    public decimal TotalFacture { get; set; }
    public List<LigneDocumentCommercialDto> Lignes { get; set; } = new();
    public List<DocumentCommercialHistoriqueDto> Historique { get; set; } = new();
}

public class ContratSignatureDto
{
    public string SignataireNom { get; set; } = string.Empty;
    public string? SignataireFonction { get; set; }
    public string? SignatureUrl { get; set; }
}

// ===== Facture =====
public class FactureDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public int? ContratId { get; set; }
    public string? ContratReference { get; set; }
    public string? Objet { get; set; }
    public string Devise { get; set; } = "MAD";
    public DateTime? PeriodeDebut { get; set; }
    public DateTime? PeriodeFin { get; set; }
    public DateTime DateEmission { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DateEnvoi { get; set; }
    public string Statut { get; set; } = RefCodes.StatutFacture.Brouillon;
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal MontantPaye { get; set; }
    public decimal MontantRestant { get; set; }
    public int Version { get; set; }
    public string? PdfUrl { get; set; }
    public string? Notes { get; set; }
    public int JoursRetard => Statut != RefCodes.StatutFacture.Payee && DateTime.UtcNow > DateEcheance ? (DateTime.UtcNow - DateEcheance).Days : 0;
    public List<LigneDocumentCommercialDto> Lignes { get; set; } = new();
    public List<PaiementDto> Paiements { get; set; } = new();
    public List<DocumentCommercialHistoriqueDto> Historique { get; set; } = new();
}

public class FactureCreateDto
{
    public int? ContratId { get; set; }
    public int? ClientId { get; set; } // Pour factures manuelles (sans contrat)
    public string? Objet { get; set; }
    public DateTime? PeriodeDebut { get; set; }
    public DateTime? PeriodeFin { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string? Notes { get; set; }
    public List<int>? LigneContratIds { get; set; } // IDs des lignes du contrat à facturer
    public List<LigneDocumentCommercialCreateDto> Lignes { get; set; } = new(); // Lignes de la facture
}

// ===== Paiement =====
public class PaiementDto
{
    public int Id { get; set; }
    public int FactureId { get; set; }
    public string? FactureReference { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime DatePaiement { get; set; }
    public decimal Montant { get; set; }
    public string Devise { get; set; } = "MAD";
    public string ModePaiement { get; set; } = "Virement";
    public string? ReferenceBancaire { get; set; }
    public string? BanqueEmettrice { get; set; }
    public string Statut { get; set; } = RefCodes.StatutPaiement.Valide;
    public string? Notes { get; set; }
    public string? PieceJointeUrl { get; set; }
}

public class PaiementCreateDto
{
    public int FactureId { get; set; }
    public DateTime? DatePaiement { get; set; }
    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = "Virement";
    public string? ReferenceBancaire { get; set; }
    public string? BanqueEmettrice { get; set; }
    public string? Notes { get; set; }
}

// ===== Historique Document Commercial =====
public class DocumentCommercialHistoriqueDto
{
    public int Id { get; set; }
    public string TypeDocument { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? AncienStatut { get; set; }
    public string? NouveauStatut { get; set; }
    public string? Commentaire { get; set; }
    public string? UserName { get; set; }
    public DateTime DateAction { get; set; }
    public int? VersionDocument { get; set; }
}

// ===== Conversion DTOs =====
public class ConvertDevisEnBonCommandeDto
{
    public int DevisId { get; set; }
    public string? Notes { get; set; }
}

public class LigneServiceOverrideDto
{
    public int LigneId { get; set; }
    public int? ServiceContratId { get; set; }
}

public class ConvertDevisEnContratDto
{
    public int DevisId { get; set; }
    public DateTime? DateDebutContrat { get; set; }
    public DateTime? DateFinContrat { get; set; }
    public string ModeleFacturation { get; set; } = "Forfait";
    public string? FrequenceFacturation { get; set; }
    public string? ClausesSpecifiques { get; set; }
    public string? Notes { get; set; }
    public List<LigneServiceOverrideDto> LigneServiceOverrides { get; set; } = new();
}

public class ContratCommercialCreateDto
{
    public int ClientId { get; set; }
    public string? Objet { get; set; }
    public string? Description { get; set; }
    public string Devise { get; set; } = "MAD";
    public DateTime? DateDebutContrat { get; set; }
    public DateTime? DateFinContrat { get; set; }
    public string ModeleFacturation { get; set; } = "Forfait";
    public string? FrequenceFacturation { get; set; }
    public string? ClausesSpecifiques { get; set; }
    public string? ConditionsParticulieres { get; set; }
    public string? Notes { get; set; }
    public List<LigneDocumentCommercialCreateDto> Lignes { get; set; } = new();
}

public class ConvertBonCommandeEnContratDto
{
    public int BonCommandeId { get; set; }
    public DateTime? DateDebutContrat { get; set; }
    public DateTime? DateFinContrat { get; set; }
    public string ModeleFacturation { get; set; } = "Forfait";
    public string? FrequenceFacturation { get; set; }
    public string? ClausesSpecifiques { get; set; }
    public string? Notes { get; set; }
    public List<LigneServiceOverrideDto> LigneServiceOverrides { get; set; } = new();
}

// ===== Dashboard Commercial =====
public class DashboardCommercialDto
{
    public int TotalDevis { get; set; }
    public int DevisEnAttente { get; set; }
    public int TotalContrats { get; set; }
    public int ContratsActifs { get; set; }
    public int TotalFactures { get; set; }
    public int FacturesEnRetard { get; set; }
    public decimal ChiffreAffairesMois { get; set; }
    public decimal MontantImpaye { get; set; }
    public decimal TauxConversion { get; set; } // Devis → Contrat
    public List<FactureDto> DernieresFactures { get; set; } = new();
    public List<DevisDto> DerniersDevis { get; set; } = new();
}
