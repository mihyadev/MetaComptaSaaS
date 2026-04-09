using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Objet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    DateEmission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateValidite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateApprobation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateRejet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MotifRejet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConditionsParticulieres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesPourClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ParentDevisId = table.Column<int>(type: "int", nullable: true),
                    CommercialId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ValidateurId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devis_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devis_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devis_Devis_ParentDevisId",
                        column: x => x.ParentDevisId,
                        principalTable: "Devis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Devis_Users_CommercialId",
                        column: x => x.CommercialId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Devis_Users_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BonsCommande",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DevisId = table.Column<int>(type: "int", nullable: false),
                    Objet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    DateEmission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateConfirmation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateApprobation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConditionsParticulieres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsCommande", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonsCommande_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonsCommande_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonsCommande_Devis_DevisId",
                        column: x => x.DevisId,
                        principalTable: "Devis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonsCommande_Users_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContratsCommerciaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    BonCommandeId = table.Column<int>(type: "int", nullable: false),
                    Objet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    DateEmission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDebutContrat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFinContrat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSignature = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SignataireNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignataireFonction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignatureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstSigne = table.Column<bool>(type: "bit", nullable: false),
                    ModeleFacturation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FrequenceFacturation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConditionsParticulieres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClausesSpecifiques = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ValidateurId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContratSigneUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratsCommerciaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratsCommerciaux_BonsCommande_BonCommandeId",
                        column: x => x.BonCommandeId,
                        principalTable: "BonsCommande",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratsCommerciaux_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratsCommerciaux_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratsCommerciaux_Users_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContratsCommerciaux_Users_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ContratId = table.Column<int>(type: "int", nullable: false),
                    Objet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    PeriodeDebut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodeFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEmission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateVue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantPaye = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRestant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EmetteurId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factures_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Factures_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Factures_ContratsCommerciaux_ContratId",
                        column: x => x.ContratId,
                        principalTable: "ContratsCommerciaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Factures_Users_EmetteurId",
                        column: x => x.EmetteurId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentsCommerciauxHistorique",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeDocument = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    DevisId = table.Column<int>(type: "int", nullable: true),
                    BonCommandeId = table.Column<int>(type: "int", nullable: true),
                    ContratId = table.Column<int>(type: "int", nullable: true),
                    FactureId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AncienStatut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NouveauStatut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionDocument = table.Column<int>(type: "int", nullable: true),
                    DonneesAvantJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonneesApresJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsCommerciauxHistorique", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentsCommerciauxHistorique_BonsCommande_BonCommandeId",
                        column: x => x.BonCommandeId,
                        principalTable: "BonsCommande",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentsCommerciauxHistorique_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentsCommerciauxHistorique_ContratsCommerciaux_ContratId",
                        column: x => x.ContratId,
                        principalTable: "ContratsCommerciaux",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentsCommerciauxHistorique_Devis_DevisId",
                        column: x => x.DevisId,
                        principalTable: "Devis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentsCommerciauxHistorique_Factures_FactureId",
                        column: x => x.FactureId,
                        principalTable: "Factures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LignesDocumentsCommerciaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevisId = table.Column<int>(type: "int", nullable: true),
                    BonCommandeId = table.Column<int>(type: "int", nullable: true),
                    ContratId = table.Column<int>(type: "int", nullable: true),
                    FactureId = table.Column<int>(type: "int", nullable: true),
                    LigneSourceId = table.Column<int>(type: "int", nullable: true),
                    TypeLigne = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TypeService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescriptionDetaillee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemisePourcentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RemiseMontant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TauxTVA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantiteConsommee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DejaFacture = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesDocumentsCommerciaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesDocumentsCommerciaux_BonsCommande_BonCommandeId",
                        column: x => x.BonCommandeId,
                        principalTable: "BonsCommande",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LignesDocumentsCommerciaux_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LignesDocumentsCommerciaux_ContratsCommerciaux_ContratId",
                        column: x => x.ContratId,
                        principalTable: "ContratsCommerciaux",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LignesDocumentsCommerciaux_Devis_DevisId",
                        column: x => x.DevisId,
                        principalTable: "Devis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LignesDocumentsCommerciaux_Factures_FactureId",
                        column: x => x.FactureId,
                        principalTable: "Factures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactureId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Devise = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ModePaiement = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferenceBancaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BanqueEmettrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PieceJointeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnregistreParId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_Factures_FactureId",
                        column: x => x.FactureId,
                        principalTable: "Factures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_Users_EnregistreParId",
                        column: x => x.EnregistreParId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(6572));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7181));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7186));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7189));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7192));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7196));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7202));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7206));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7209));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7216));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7219));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7227));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7229));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7232));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7235));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7238));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7241));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7245));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7247));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7250));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7260));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7262));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7266));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7269));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7273));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7274));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7277));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7282));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7285));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7288));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7293));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7295));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7297));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7303));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7306));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7309));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7312));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7314));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7318));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7321));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7324));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7328));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7335));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7338));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7341));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7345));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7348));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7352));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7354));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7357));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7359));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7362));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7365));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7368));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7370));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7373));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7376));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7380));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7383));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7386));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7389));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7393));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Module", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 67, "View", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7397), null, null, null, "Permission to View in Commercial module", false, "Commercial", "Commercial.View", null, null },
                    { 68, "Create", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7400), null, null, null, "Permission to Create in Commercial module", false, "Commercial", "Commercial.Create", null, null },
                    { 69, "Edit", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7409), null, null, null, "Permission to Edit in Commercial module", false, "Commercial", "Commercial.Edit", null, null },
                    { 70, "Delete", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7412), null, null, null, "Permission to Delete in Commercial module", false, "Commercial", "Commercial.Delete", null, null },
                    { 71, "Validate", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7415), null, null, null, "Permission to Validate in Commercial module", false, "Commercial", "Commercial.Validate", null, null },
                    { 72, "Convert", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7418), null, null, null, "Permission to Convert in Commercial module", false, "Commercial", "Commercial.Convert", null, null },
                    { 73, "Sign", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7421), null, null, null, "Permission to Sign in Commercial module", false, "Commercial", "Commercial.Sign", null, null },
                    { 74, "GenerateInvoice", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7424), null, null, null, "Permission to GenerateInvoice in Commercial module", false, "Commercial", "Commercial.GenerateInvoice", null, null },
                    { 75, "View", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7427), null, null, null, "Permission to View in Facture module", false, "Facture", "Facture.View", null, null },
                    { 76, "Create", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7430), null, null, null, "Permission to Create in Facture module", false, "Facture", "Facture.Create", null, null },
                    { 77, "Edit", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7433), null, null, null, "Permission to Edit in Facture module", false, "Facture", "Facture.Edit", null, null },
                    { 78, "Send", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7435), null, null, null, "Permission to Send in Facture module", false, "Facture", "Facture.Send", null, null },
                    { 79, "Cancel", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7438), null, null, null, "Permission to Cancel in Facture module", false, "Facture", "Facture.Cancel", null, null },
                    { 80, "Refund", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7440), null, null, null, "Permission to Refund in Facture module", false, "Facture", "Facture.Refund", null, null },
                    { 81, "ManagePayments", new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(7443), null, null, null, "Permission to ManagePayments in Facture module", false, "Facture", "Facture.ManagePayments", null, null }
                });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(8724));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9846));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9868));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9871));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9873));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9905));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9907));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9909));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9912));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 475, DateTimeKind.Utc).AddTicks(9914));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(135));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(138));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(152));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(155));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(158));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(160));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(162));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(165));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(167));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(169));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(172));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(364));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(377));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(380));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(395));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(398));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(400));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(402));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(387));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(389));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(391));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(393));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(416));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(427));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(429));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(431));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(433));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(435));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(438));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(548));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(551));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(554));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(556));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(559));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(572));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(574));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(577));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(579));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(582));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(584));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(586));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(589));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(591));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(594));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(596));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(598));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(611));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(614));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(616));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(618));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(621));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(623));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(625));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(627));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(629));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(631));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(632));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(634));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(643));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(655));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(657));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(659));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(661));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(663));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(665));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(667));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(669));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(671));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(673));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(675));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(677));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(679));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(681));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(683));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(696));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(698));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(700));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(702));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(704));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(706));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(708));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(710));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(713));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(715));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(717));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(719));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(722));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(734));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(737));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(740));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(742));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(744));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(747));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(755));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(757));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(759));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(762));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(764));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(775));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(778));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(780));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(783));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(785));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(787));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(789));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(791));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(793));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(795));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(796));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(800));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(802));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(818));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(820));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(822));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(825));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(827));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(830));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(832));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(834));

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "Couleur", "CreatedAt", "CreatedBy", "CssClass", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "Icone", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 200, null, "STATUT_DEVIS", "Brouillon", "#94a3b8", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(836), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 201, null, "STATUT_DEVIS", "Soumis", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(839), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-send", true, false, "Soumis", null, null, null, null, 2, null, null, null },
                    { 202, null, "STATUT_DEVIS", "Approuve", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(841), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Approuvé", null, null, null, null, 3, null, null, null },
                    { 203, null, "STATUT_DEVIS", "Rejete", "#dc2626", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(843), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Rejeté", null, null, null, null, 4, null, null, null },
                    { 204, null, "STATUT_DEVIS", "Expire", "#f59e0b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(856), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clock-history", true, false, "Expiré", null, null, null, null, 5, null, null, null },
                    { 205, null, "STATUT_DEVIS", "Annule", "#64748b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(859), null, "bg-dark-subtle text-dark", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-slash-circle", true, false, "Annulé", null, null, null, null, 6, null, null, null },
                    { 210, null, "STATUT_BON_COMMANDE", "Brouillon", "#94a3b8", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(861), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 211, null, "STATUT_BON_COMMANDE", "Confirme", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(864), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check", true, false, "Confirmé", null, null, null, null, 2, null, null, null },
                    { 212, null, "STATUT_BON_COMMANDE", "Approuve", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(866), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Approuvé", null, null, null, null, 3, null, null, null },
                    { 213, null, "STATUT_BON_COMMANDE", "Annule", "#dc2626", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(876), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Annulé", null, null, null, null, 4, null, null, null },
                    { 220, null, "STATUT_CONTRAT", "Brouillon", "#94a3b8", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(878), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 221, null, "STATUT_CONTRAT", "EnCours", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(881), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-repeat", true, false, "En cours", null, null, null, null, 2, null, null, null },
                    { 222, null, "STATUT_CONTRAT", "Signe", "#7c3aed", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(883), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pen", true, false, "Signé", null, null, null, null, 3, null, null, null },
                    { 223, null, "STATUT_CONTRAT", "Confirme", "#0891b2", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(885), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check", true, false, "Confirmé", null, null, null, null, 4, null, null, null },
                    { 224, null, "STATUT_CONTRAT", "Valide", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(887), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle-fill", true, false, "Validé", null, null, null, null, 5, null, null, null },
                    { 225, null, "STATUT_CONTRAT", "Actif", "#166534", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(890), null, "bg-success text-white", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-play-circle", true, false, "Actif", null, null, null, null, 6, null, null, null },
                    { 226, null, "STATUT_CONTRAT", "Suspendu", "#f59e0b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(902), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pause-circle", true, false, "Suspendu", null, null, null, null, 7, null, null, null },
                    { 227, null, "STATUT_CONTRAT", "Termine", "#64748b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(904), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-stop-circle", true, false, "Terminé", null, null, null, null, 8, null, null, null },
                    { 228, null, "STATUT_CONTRAT", "Annule", "#dc2626", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(907), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Annulé", null, null, null, null, 9, null, null, null },
                    { 230, null, "STATUT_FACTURE", "Brouillon", "#94a3b8", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(909), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 231, null, "STATUT_FACTURE", "Generee", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(912), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-file-earmark-text", true, false, "Générée", null, null, null, null, 2, null, null, null },
                    { 232, null, "STATUT_FACTURE", "Envoyee", "#7c3aed", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(914), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-send", true, false, "Envoyée", null, null, null, null, 3, null, null, null },
                    { 233, null, "STATUT_FACTURE", "Vue", "#0891b2", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(916), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-eye", true, false, "Vue", null, null, null, null, 4, null, null, null },
                    { 234, null, "STATUT_FACTURE", "PartiellementPayee", "#f59e0b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(919), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash", true, false, "Partiellement payée", null, null, null, null, 5, null, null, null },
                    { 235, null, "STATUT_FACTURE", "Payee", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(922), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Payée", null, null, null, null, 6, null, null, null },
                    { 236, null, "STATUT_FACTURE", "EnRetard", "#dc2626", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(924), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-exclamation-triangle", true, false, "En retard", null, null, null, null, 7, null, null, null },
                    { 237, null, "STATUT_FACTURE", "Annulee", "#64748b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(927), null, "bg-dark-subtle text-dark", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Annulée", null, null, null, null, 8, null, null, null },
                    { 238, null, "STATUT_FACTURE", "Remboursee", "#ec4899", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(939), null, "bg-pink-subtle text-pink", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-counterclockwise", true, false, "Remboursée", null, null, null, null, 9, null, null, null },
                    { 240, null, "STATUT_PAIEMENT", "Valide", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(942), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Validé", null, null, null, null, 1, null, null, null },
                    { 241, null, "STATUT_PAIEMENT", "EnAttente", "#f59e0b", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(944), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clock", true, false, "En attente", null, null, null, null, 2, null, null, null },
                    { 242, null, "STATUT_PAIEMENT", "Rejete", "#dc2626", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(947), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Rejeté", null, null, null, null, 3, null, null, null },
                    { 250, null, "MODELE_FACTURATION", "Forfait", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(949), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-box", true, false, "Forfait (Prix fixe)", null, null, null, null, 1, null, null, null },
                    { 251, null, "MODELE_FACTURATION", "Regie", "#7c3aed", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(952), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clock-history", true, false, "Régie (Temps & Matériel)", null, null, null, null, 2, null, null, null },
                    { 255, null, "TYPE_LIGNE", "Service", "#2563eb", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(954), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Prestation de service", null, null, null, null, 1, null, null, null },
                    { 256, null, "TYPE_LIGNE", "Regie", "#7c3aed", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(956), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Régie (Temps & Matériel)", null, null, null, null, 2, null, null, null },
                    { 257, null, "TYPE_LIGNE", "Forfait", "#059669", new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(958), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Forfait (Prix fixe)", null, null, null, null, 3, null, null, null },
                    { 260, null, "MODE_PAIEMENT_COMMERCIAL", "Virement", null, new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(960), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-bank", true, false, "Virement bancaire", null, null, null, null, 1, null, null, null },
                    { 261, null, "MODE_PAIEMENT_COMMERCIAL", "Cheque", null, new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(962), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-credit-card", true, false, "Chèque", null, null, null, null, 2, null, null, null },
                    { 262, null, "MODE_PAIEMENT_COMMERCIAL", "Carte", null, new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(965), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-credit-card-2-front", true, false, "Carte bancaire", null, null, null, null, 3, null, null, null },
                    { 263, null, "MODE_PAIEMENT_COMMERCIAL", "Especes", null, new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(967), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash", true, false, "Espèces", null, null, null, null, 4, null, null, null },
                    { 264, null, "MODE_PAIEMENT_COMMERCIAL", "Prelevement", null, new DateTime(2026, 3, 22, 16, 51, 42, 476, DateTimeKind.Utc).AddTicks(969), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-repeat", true, false, "Prélèvement automatique", null, null, null, null, 5, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 474, DateTimeKind.Utc).AddTicks(8620));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 474, DateTimeKind.Utc).AddTicks(9858));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 16, 51, 42, 474, DateTimeKind.Utc).AddTicks(9861));

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommande_CabinetId_Reference",
                table: "BonsCommande",
                columns: new[] { "CabinetId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommande_ClientId",
                table: "BonsCommande",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommande_DevisId",
                table: "BonsCommande",
                column: "DevisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommande_ResponsableId",
                table: "BonsCommande",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratsCommerciaux_BonCommandeId",
                table: "ContratsCommerciaux",
                column: "BonCommandeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratsCommerciaux_CabinetId_Reference",
                table: "ContratsCommerciaux",
                columns: new[] { "CabinetId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratsCommerciaux_ClientId",
                table: "ContratsCommerciaux",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratsCommerciaux_ResponsableId",
                table: "ContratsCommerciaux",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratsCommerciaux_ValidateurId",
                table: "ContratsCommerciaux",
                column: "ValidateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Devis_CabinetId_Reference",
                table: "Devis",
                columns: new[] { "CabinetId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devis_ClientId",
                table: "Devis",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Devis_CommercialId",
                table: "Devis",
                column: "CommercialId");

            migrationBuilder.CreateIndex(
                name: "IX_Devis_ParentDevisId",
                table: "Devis",
                column: "ParentDevisId");

            migrationBuilder.CreateIndex(
                name: "IX_Devis_ValidateurId",
                table: "Devis",
                column: "ValidateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_BonCommandeId",
                table: "DocumentsCommerciauxHistorique",
                column: "BonCommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_CabinetId",
                table: "DocumentsCommerciauxHistorique",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_ContratId",
                table: "DocumentsCommerciauxHistorique",
                column: "ContratId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_DateAction",
                table: "DocumentsCommerciauxHistorique",
                column: "DateAction");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_DevisId",
                table: "DocumentsCommerciauxHistorique",
                column: "DevisId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_FactureId",
                table: "DocumentsCommerciauxHistorique",
                column: "FactureId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsCommerciauxHistorique_TypeDocument_DocumentId",
                table: "DocumentsCommerciauxHistorique",
                columns: new[] { "TypeDocument", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Factures_CabinetId_Reference",
                table: "Factures",
                columns: new[] { "CabinetId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ClientId",
                table: "Factures",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ContratId",
                table: "Factures",
                column: "ContratId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_EmetteurId",
                table: "Factures",
                column: "EmetteurId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_BonCommandeId",
                table: "LignesDocumentsCommerciaux",
                column: "BonCommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_CabinetId",
                table: "LignesDocumentsCommerciaux",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_ContratId",
                table: "LignesDocumentsCommerciaux",
                column: "ContratId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_DevisId",
                table: "LignesDocumentsCommerciaux",
                column: "DevisId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_FactureId",
                table: "LignesDocumentsCommerciaux",
                column: "FactureId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_CabinetId_Reference",
                table: "Paiements",
                columns: new[] { "CabinetId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_EnregistreParId",
                table: "Paiements",
                column: "EnregistreParId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_FactureId",
                table: "Paiements",
                column: "FactureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentsCommerciauxHistorique");

            migrationBuilder.DropTable(
                name: "LignesDocumentsCommerciaux");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "ContratsCommerciaux");

            migrationBuilder.DropTable(
                name: "BonsCommande");

            migrationBuilder.DropTable(
                name: "Devis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 220);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 221);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 222);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 223);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 224);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 225);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 226);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 227);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 228);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 230);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 231);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 232);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 233);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 234);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 235);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 236);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 237);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 238);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 240);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 241);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 242);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 250);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 251);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 256);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 257);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 260);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 261);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 262);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 263);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 264);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(848));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1343));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1347));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1350));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1355));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1359));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1363));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1367));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1369));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1373));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1375));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1383));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1385));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1436));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1438));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1441));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1444));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1447));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1449));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1451));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1454));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1456));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1458));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1460));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1464));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1468));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1470));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1473));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1475));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1478));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1482));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1484));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1487));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1489));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1492));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1494));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1529));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1531));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1534));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1537));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1540));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1543));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1545));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1547));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1550));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1553));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1555));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1558));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1560));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1562));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1565));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1567));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1572));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1574));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1577));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1579));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1582));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1584));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1587));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1613));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1616));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(2598));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3483));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3486));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3488));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3489));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3491));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3518));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3520));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3539));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3541));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3543));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3722));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3765));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3767));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3769));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3782));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3784));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3786));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3963));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3965));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3967));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3969));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3992));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3994));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3995));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3972));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3974));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3987));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3997));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3999));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4001));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4002));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4004));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4006));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4008));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4097));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4099));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4101));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4103));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4105));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4134));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4136));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4138));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4140));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4142));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4144));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4156));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4158));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4166));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4168));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4170));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4172));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4174));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4175));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4177));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4189));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4190));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4192));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4193));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4195));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4196));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4198));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4199));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4201));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4203));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4204));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4206));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4207));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4209));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4210));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4223));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4224));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4226));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4245));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4247));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4249));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4250));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4252));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4254));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4255));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4257));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4259));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4260));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4262));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4274));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4276));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4278));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4281));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4283));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4287));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4288));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4290));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4292));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4294));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4305));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4307));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4308));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4310));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4312));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4314));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4316));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4318));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4319));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4321));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4323));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4369));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4381));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4385));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4387));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4388));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4390));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4392));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4393));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4394));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4396));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4398));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4400));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4402));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4404));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(3496));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(4515));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(4518));
        }
    }
}
