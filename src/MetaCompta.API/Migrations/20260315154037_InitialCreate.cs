using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizationEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabelAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CabinetId = table.Column<int>(type: "int", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxInternalUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClientCompanies = table.Column<int>(type: "int", nullable: false),
                    MaxExtranetUsersPerClient = table.Column<int>(type: "int", nullable: false),
                    MaxStorageMB = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cabinets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ICE = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionPlanId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cabinets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cabinets_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaisonSociale = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ICE = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    IF = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RC = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NumeroCNSS = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NumeroPatente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDebutActivite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodeActivite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormeJuridique = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecteurActivite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CapitalSocial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AdresseSiegeSocial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VilleSiegeSocial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelephonePrincipal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactFonction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerPrincipalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TypeTVA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoisClotureExercice = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateDebutContrat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotesInternes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CIN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LieuNaissance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelephonePortable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelephoneFixe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SituationFamiliale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreEnfantsACharge = table.Column<int>(type: "int", nullable: false),
                    DateMariage = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sexe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    NumeroCNSS = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateEmbauche = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSortie = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TypeContrat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DureeContratMois = table.Column<int>(type: "int", nullable: true),
                    Poste = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Departement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategorieSocioProfessionnelle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatriculeEmployeur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotifSortie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalaireBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrimeAnciennete = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PrimeResponsabilite = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PrimePresence = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IndemniteTransport = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IndemniteRepresentation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AvantagesNature = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TauxHoraire = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ModePaiement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RIB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomBanque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgenceBancaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUrgenceNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUrgenceRelation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUrgenceTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesInternes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sujet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsReadByRecipient = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentMessageId = table.Column<int>(type: "int", nullable: true),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CabinetId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAbsences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TypeAbsence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreJours = table.Column<int>(type: "int", nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentJustificatifUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAbsences_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAbsences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Declarations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Periode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSoumission = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotifRefus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssistantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ValidateurId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DonneesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MontantDu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontantCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExportUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Declarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Declarations_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Declarations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Declarations_Users_AssistantId",
                        column: x => x.AssistantId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Declarations_Users_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalSalairesBruts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCotisationsSalariales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCotisationsPatronales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIRRetenu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalNetAPayer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCoutEmployeur = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LivrePaieUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JournalComptableUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FichierVirementUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidateurId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Users_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeclarationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeclarationId = table.Column<int>(type: "int", nullable: false),
                    AncienStatut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NouveauStatut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateChangement = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_DeclarationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationHistories_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationHistories_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeclarationsCNSS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeclarationId = table.Column<int>(type: "int", nullable: false),
                    TotalSalairesBruts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCotisationSalariale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCotisationPatronale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAMOSalariale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAMOPatronale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFormationProfessionnelle = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NombreEmployes = table.Column<int>(type: "int", nullable: false),
                    DetailEmployesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_DeclarationsCNSS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationsCNSS_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationsCNSS_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeclarationsIS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeclarationId = table.Column<int>(type: "int", nullable: false),
                    AnneeFiscale = table.Column<int>(type: "int", nullable: false),
                    ResultatComptable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reintegrations = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ResultatFiscal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeficitsAnterieurs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseImposable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TauxIS = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ISDu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AcomptesVerses = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ISNet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Acompte1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Acompte2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Acompte3 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Acompte4 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_DeclarationsIS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationsIS_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationsIS_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeclarationsTVA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeclarationId = table.Column<int>(type: "int", nullable: false),
                    CAHT_20 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CAHT_14 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CAHT_10 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CAHT_7 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CAExonere = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVA_20 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVA_14 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVA_10 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVA_7 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVACollecteeTotale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVARecuperableCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVARecuperableImmobilisations = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVARecuperableTotale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditPrecedent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVANette = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TVAaPayer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditTVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_DeclarationsTVA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationsTVA_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationsTVA_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categorie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheminStockage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TailleFichier = table.Column<long>(type: "bigint", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDocument = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    DeclarationId = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotifRejet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ParentDocumentId = table.Column<int>(type: "int", nullable: true),
                    PartageExtranet = table.Column<bool>(type: "bit", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Documents_ParentDocumentId",
                        column: x => x.ParentDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    DeclarationId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priorite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PiecesJointesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_TaskItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskItems_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskItems_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskItems_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskItems_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskItems_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaySlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaireBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrimesFixesTotales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HeuresSupp25 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HeuresSupp50 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HeuresSupp100 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantHeuresSupp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrimesExceptionnelles = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IndemnitesExceptionnelles = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvancesSalaire = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AbsencesNonRemunerees = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantAbsences = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalaireBrutGlobal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CotisationCNSS = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CotisationAMO = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CotisationCIMR = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalaireNetImposable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IRRetenu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AutresRetenues = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalaireNetAPayer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CotisationPatronaleCNSS = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CotisationPatronaleAMO = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FormationProfessionnelle = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AssurancesComplementaires = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CoutTotalEmployeur = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CumulBrutAnnuel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CumulNetImposableAnnuel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CumulIRAnnuel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CumulCNSSAnnuel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PaySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaySlips_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaySlips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaySlips_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_TaskComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskComments_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskComments_TaskItems_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Module", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(1765), null, null, null, "Permission to View in Client module", false, "Client", "Client.View", null, null },
                    { 2, "Create", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2278), null, null, null, "Permission to Create in Client module", false, "Client", "Client.Create", null, null },
                    { 3, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2283), null, null, null, "Permission to Edit in Client module", false, "Client", "Client.Edit", null, null },
                    { 4, "Delete", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2285), null, null, null, "Permission to Delete in Client module", false, "Client", "Client.Delete", null, null },
                    { 5, "Archive", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2288), null, null, null, "Permission to Archive in Client module", false, "Client", "Client.Archive", null, null },
                    { 6, "Configure", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2291), null, null, null, "Permission to Configure in Client module", false, "Client", "Client.Configure", null, null },
                    { 7, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2295), null, null, null, "Permission to View in Declaration module", false, "Declaration", "Declaration.View", null, null },
                    { 8, "Create", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2298), null, null, null, "Permission to Create in Declaration module", false, "Declaration", "Declaration.Create", null, null },
                    { 9, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2315), null, null, null, "Permission to Edit in Declaration module", false, "Declaration", "Declaration.Edit", null, null },
                    { 10, "Validate", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2318), null, null, null, "Permission to Validate in Declaration module", false, "Declaration", "Declaration.Validate", null, null },
                    { 11, "Submit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2321), null, null, null, "Permission to Submit in Declaration module", false, "Declaration", "Declaration.Submit", null, null },
                    { 12, "Delete", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2323), null, null, null, "Permission to Delete in Declaration module", false, "Declaration", "Declaration.Delete", null, null },
                    { 13, "Revert", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2325), null, null, null, "Permission to Revert in Declaration module", false, "Declaration", "Declaration.Revert", null, null },
                    { 14, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2328), null, null, null, "Permission to View in Employee module", false, "Employee", "Employee.View", null, null },
                    { 15, "Create", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2330), null, null, null, "Permission to Create in Employee module", false, "Employee", "Employee.Create", null, null },
                    { 16, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2333), null, null, null, "Permission to Edit in Employee module", false, "Employee", "Employee.Edit", null, null },
                    { 17, "Delete", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2335), null, null, null, "Permission to Delete in Employee module", false, "Employee", "Employee.Delete", null, null },
                    { 18, "Archive", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2338), null, null, null, "Permission to Archive in Employee module", false, "Employee", "Employee.Archive", null, null },
                    { 19, "ManageAbsences", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2340), null, null, null, "Permission to ManageAbsences in Employee module", false, "Employee", "Employee.ManageAbsences", null, null },
                    { 20, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2343), null, null, null, "Permission to View in Payroll module", false, "Payroll", "Payroll.View", null, null },
                    { 21, "Create", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2345), null, null, null, "Permission to Create in Payroll module", false, "Payroll", "Payroll.Create", null, null },
                    { 22, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2347), null, null, null, "Permission to Edit in Payroll module", false, "Payroll", "Payroll.Edit", null, null },
                    { 23, "Validate", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2349), null, null, null, "Permission to Validate in Payroll module", false, "Payroll", "Payroll.Validate", null, null },
                    { 24, "Process", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2352), null, null, null, "Permission to Process in Payroll module", false, "Payroll", "Payroll.Process", null, null },
                    { 25, "GenerateSlips", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2354), null, null, null, "Permission to GenerateSlips in Payroll module", false, "Payroll", "Payroll.GenerateSlips", null, null },
                    { 26, "GenerateBankFile", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2356), null, null, null, "Permission to GenerateBankFile in Payroll module", false, "Payroll", "Payroll.GenerateBankFile", null, null },
                    { 27, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2359), null, null, null, "Permission to View in Document module", false, "Document", "Document.View", null, null },
                    { 28, "Upload", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2361), null, null, null, "Permission to Upload in Document module", false, "Document", "Document.Upload", null, null },
                    { 29, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2363), null, null, null, "Permission to Edit in Document module", false, "Document", "Document.Edit", null, null },
                    { 30, "Validate", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2365), null, null, null, "Permission to Validate in Document module", false, "Document", "Document.Validate", null, null },
                    { 31, "Delete", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2367), null, null, null, "Permission to Delete in Document module", false, "Document", "Document.Delete", null, null },
                    { 32, "Share", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2370), null, null, null, "Permission to Share in Document module", false, "Document", "Document.Share", null, null },
                    { 33, "Download", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2379), null, null, null, "Permission to Download in Document module", false, "Document", "Document.Download", null, null },
                    { 34, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2383), null, null, null, "Permission to View in Task module", false, "Task", "Task.View", null, null },
                    { 35, "Create", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2385), null, null, null, "Permission to Create in Task module", false, "Task", "Task.Create", null, null },
                    { 36, "Edit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2387), null, null, null, "Permission to Edit in Task module", false, "Task", "Task.Edit", null, null },
                    { 37, "Delete", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2389), null, null, null, "Permission to Delete in Task module", false, "Task", "Task.Delete", null, null },
                    { 38, "Assign", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2391), null, null, null, "Permission to Assign in Task module", false, "Task", "Task.Assign", null, null },
                    { 39, "ManageAll", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2393), null, null, null, "Permission to ManageAll in Task module", false, "Task", "Task.ManageAll", null, null },
                    { 40, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2396), null, null, null, "Permission to View in Report module", false, "Report", "Report.View", null, null },
                    { 41, "Export", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2398), null, null, null, "Permission to Export in Report module", false, "Report", "Report.Export", null, null },
                    { 42, "ViewStrategic", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2400), null, null, null, "Permission to ViewStrategic in Report module", false, "Report", "Report.ViewStrategic", null, null },
                    { 43, "View", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2403), null, null, null, "Permission to View in Notification module", false, "Notification", "Notification.View", null, null },
                    { 44, "Manage", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2406), null, null, null, "Permission to Manage in Notification module", false, "Notification", "Notification.Manage", null, null },
                    { 45, "Configure", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2408), null, null, null, "Permission to Configure in Notification module", false, "Notification", "Notification.Configure", null, null },
                    { 46, "Admin", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2411), null, null, null, "Permission to Admin in System module", false, "System", "System.Admin", null, null },
                    { 47, "UserManage", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2413), null, null, null, "Permission to UserManage in System module", false, "System", "System.UserManage", null, null },
                    { 48, "ProfileManage", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2415), null, null, null, "Permission to ProfileManage in System module", false, "System", "System.ProfileManage", null, null },
                    { 49, "AuditView", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2417), null, null, null, "Permission to AuditView in System module", false, "System", "System.AuditView", null, null },
                    { 50, "ConfigurationView", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2420), null, null, null, "Permission to ConfigurationView in System module", false, "System", "System.ConfigurationView", null, null },
                    { 51, "ConfigurationEdit", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2422), null, null, null, "Permission to ConfigurationEdit in System module", false, "System", "System.ConfigurationEdit", null, null }
                });

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, null, "TVA_RATE", "TVA_20", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(3323), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "TVA 20%", null, null, null, 20m, 0, null, null, null },
                    { 2, null, "TVA_RATE", "TVA_14", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4232), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "TVA 14%", null, null, null, 14m, 0, null, null, null },
                    { 3, null, "TVA_RATE", "TVA_10", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4234), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "TVA 10%", null, null, null, 10m, 0, null, null, null },
                    { 4, null, "TVA_RATE", "TVA_7", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4236), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "TVA 7%", null, null, null, 7m, 0, null, null, null },
                    { 5, null, "CNSS", "PLAFOND", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4238), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Plafond CNSS", null, null, null, 6000m, 0, null, null, null },
                    { 6, null, "CNSS", "TAUX_SALARIAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4250), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux CNSS Salariale", null, null, null, 6.74m, 0, null, null, null },
                    { 7, null, "CNSS", "TAUX_PATRONAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4274), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux CNSS Patronale", null, null, null, 20.48m, 0, null, null, null },
                    { 8, null, "AMO", "TAUX_SALARIAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4275), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux AMO Salariale", null, null, null, 2.26m, 0, null, null, null },
                    { 9, null, "AMO", "TAUX_PATRONAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4277), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux AMO Patronale", null, null, null, 4.11m, 0, null, null, null },
                    { 10, null, "FP", "TAUX", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4279), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux Formation Prof.", null, null, null, 1.6m, 0, null, null, null },
                    { 11, null, "IR_BRACKET", "T1", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4281), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "0 - 30 000 DH", null, null, null, 0m, 1, null, null, "{\"min\":0,\"max\":30000,\"rate\":0}" },
                    { 12, null, "IR_BRACKET", "T2", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4455), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "30 001 - 50 000 DH", null, null, null, 10m, 2, null, null, "{\"min\":30001,\"max\":50000,\"rate\":10}" },
                    { 13, null, "IR_BRACKET", "T3", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4460), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "50 001 - 60 000 DH", null, null, null, 20m, 3, null, null, "{\"min\":50001,\"max\":60000,\"rate\":20}" },
                    { 14, null, "IR_BRACKET", "T4", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4463), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "60 001 - 80 000 DH", null, null, null, 30m, 4, null, null, "{\"min\":60001,\"max\":80000,\"rate\":30}" },
                    { 15, null, "IR_BRACKET", "T5", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4465), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "80 001 - 180 000 DH", null, null, null, 34m, 5, null, null, "{\"min\":80001,\"max\":180000,\"rate\":34}" },
                    { 16, null, "IR_BRACKET", "T6", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4477), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "> 180 000 DH", null, null, null, 38m, 6, null, null, "{\"min\":180001,\"max\":999999999,\"rate\":38}" },
                    { 17, null, "IS_RATE", "NORMAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4479), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux IS Normal", null, null, null, 31m, 0, null, null, null },
                    { 18, null, "IS_RATE", "REDUIT", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4481), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux IS Réduit (PME)", null, null, null, 20m, 0, null, null, null },
                    { 19, null, "IS_RATE", "MINIMUM", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4482), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Taux IS Minimum", null, null, null, 10m, 0, null, null, null },
                    { 20, null, "IR_DEDUCTION", "CHARGE_FAMILLE", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4484), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Déduction par enfant", null, null, null, 360m, 0, null, null, null },
                    { 21, null, "IR_DEDUCTION", "PLAFOND_ENFANTS", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4486), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Plafond nombre enfants", null, null, null, 6m, 0, null, null, null },
                    { 22, null, "DOC_CATEGORY", "LEGAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4487), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents Légaux", null, null, null, null, 1, null, null, null },
                    { 23, null, "DOC_CATEGORY", "FISCAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4489), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents Fiscaux", null, null, null, null, 2, null, null, null },
                    { 24, null, "DOC_CATEGORY", "SOCIAL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4490), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents Sociaux", null, null, null, null, 3, null, null, null },
                    { 25, null, "DOC_CATEGORY", "COMPTABLE", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4492), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents Comptables", null, null, null, null, 4, null, null, null },
                    { 26, null, "DOC_CATEGORY", "RH", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4493), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents RH", null, null, null, null, 5, null, null, null },
                    { 27, null, "DOC_CATEGORY", "ADMIN", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4495), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Documents Administratifs", null, null, null, null, 6, null, null, null },
                    { 28, null, "FORME_JURIDIQUE", "SARL", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4496), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "SARL", null, null, null, null, 1, null, null, null },
                    { 29, null, "FORME_JURIDIQUE", "SA", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4498), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "SA", null, null, null, null, 2, null, null, null },
                    { 30, null, "FORME_JURIDIQUE", "SNC", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4500), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "SNC", null, null, null, null, 3, null, null, null },
                    { 31, null, "FORME_JURIDIQUE", "EI", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4501), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Entreprise Individuelle", null, null, null, null, 4, null, null, null },
                    { 32, null, "TYPE_CONTRAT", "CDI", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4504), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "CDI", null, null, null, null, 1, null, null, null },
                    { 33, null, "TYPE_CONTRAT", "CDD", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4505), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "CDD", null, null, null, null, 2, null, null, null },
                    { 34, null, "TYPE_CONTRAT", "INTERIM", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4507), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Intérim", null, null, null, null, 3, null, null, null },
                    { 35, null, "TYPE_CONTRAT", "STAGE", new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4508), null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Stage", null, null, null, null, 4, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "AnnualPrice", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "Features", "IsActive", "IsDeleted", "MaxClientCompanies", "MaxExtranetUsersPerClient", "MaxInternalUsers", "MaxStorageMB", "MonthlyPrice", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 4990m, new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(6442), null, null, null, "Pour les petits cabinets", null, true, false, 10, 5, 3, 2048, 499m, "Starter", null, null },
                    { 2, 9990m, new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(7303), null, null, null, "Pour les cabinets en croissance", null, true, false, 50, 10, 10, 10240, 999m, "Professionnel", null, null },
                    { 3, 19990m, new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(7305), null, null, null, "Pour les grands cabinets", null, true, false, 200, 20, 50, 51200, 1999m, "Entreprise", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cabinets_ICE",
                table: "Cabinets",
                column: "ICE",
                unique: true,
                filter: "[ICE] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cabinets_SubscriptionPlanId",
                table: "Cabinets",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CabinetId_ICE",
                table: "Clients",
                columns: new[] { "CabinetId", "ICE" },
                unique: true,
                filter: "[ICE] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CabinetId_IF",
                table: "Clients",
                columns: new[] { "CabinetId", "IF" },
                unique: true,
                filter: "[IF] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CabinetId_NumeroCNSS",
                table: "Clients",
                columns: new[] { "CabinetId", "NumeroCNSS" },
                unique: true,
                filter: "[NumeroCNSS] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CabinetId_RC",
                table: "Clients",
                columns: new[] { "CabinetId", "RC" },
                unique: true,
                filter: "[RC] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ManagerPrincipalId",
                table: "Clients",
                column: "ManagerPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationHistories_CabinetId",
                table: "DeclarationHistories",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationHistories_DeclarationId",
                table: "DeclarationHistories",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_Declarations_AssistantId",
                table: "Declarations",
                column: "AssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Declarations_CabinetId",
                table: "Declarations",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_Declarations_ClientId",
                table: "Declarations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Declarations_ValidateurId",
                table: "Declarations",
                column: "ValidateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsCNSS_CabinetId",
                table: "DeclarationsCNSS",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsCNSS_DeclarationId",
                table: "DeclarationsCNSS",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsIS_CabinetId",
                table: "DeclarationsIS",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsIS_DeclarationId",
                table: "DeclarationsIS",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsTVA_CabinetId",
                table: "DeclarationsTVA",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsTVA_DeclarationId",
                table: "DeclarationsTVA",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CabinetId",
                table: "Documents",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId",
                table: "Documents",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeclarationId",
                table: "Documents",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EmployeeId",
                table: "Documents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ParentDocumentId",
                table: "Documents",
                column: "ParentDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_CabinetId",
                table: "EmployeeAbsences",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_EmployeeId",
                table: "EmployeeAbsences",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CabinetId",
                table: "Employees",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ClientId_NumeroCNSS",
                table: "Employees",
                columns: new[] { "ClientId", "NumeroCNSS" },
                unique: true,
                filter: "[NumeroCNSS] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationEntries_ResourceKey_Culture_CabinetId",
                table: "LocalizationEntries",
                columns: new[] { "ResourceKey", "Culture", "CabinetId" },
                unique: true,
                filter: "[CabinetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CabinetId",
                table: "Messages",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ClientId",
                table: "Messages",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CabinetId",
                table: "Notifications",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_CabinetId",
                table: "PayrollPeriods",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_ClientId_Annee_Mois",
                table: "PayrollPeriods",
                columns: new[] { "ClientId", "Annee", "Mois" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_ValidateurId",
                table: "PayrollPeriods",
                column: "ValidateurId");

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_CabinetId",
                table: "PaySlips",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_EmployeeId",
                table: "PaySlips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_PayrollPeriodId",
                table: "PaySlips",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceData_Category_Code",
                table: "ReferenceData",
                columns: new[] { "Category", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_CabinetId",
                table: "TaskComments",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskItemId",
                table: "TaskComments",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_AssignedByUserId",
                table: "TaskItems",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_AssignedToUserId",
                table: "TaskItems",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CabinetId",
                table: "TaskItems",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_ClientId",
                table: "TaskItems",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_DeclarationId",
                table: "TaskItems",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CabinetId",
                table: "Users",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClientId",
                table: "Users",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Users_ManagerPrincipalId",
                table: "Clients",
                column: "ManagerPrincipalId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Users_ManagerPrincipalId",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DeclarationHistories");

            migrationBuilder.DropTable(
                name: "DeclarationsCNSS");

            migrationBuilder.DropTable(
                name: "DeclarationsIS");

            migrationBuilder.DropTable(
                name: "DeclarationsTVA");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "EmployeeAbsences");

            migrationBuilder.DropTable(
                name: "LocalizationEntries");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaySlips");

            migrationBuilder.DropTable(
                name: "ReferenceData");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "TaskComments");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TaskItems");

            migrationBuilder.DropTable(
                name: "Declarations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Cabinets");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
