using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MetaCompta.Infrastructure.Data;

#nullable disable

namespace MetaCompta.API.Migrations;

/// <summary>Focused migration: domiciliation tables + TaskItem link columns + TYPE_SERVICE seed.</summary>
[DbContext(typeof(MetaComptaDbContext))]
[Migration("20261003180000_AddDomiciliationModule")]
public partial class AddDomiciliationModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent cleanup if a previous attempt failed mid-way
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[DemandesActionCourrier]', N'U') IS NOT NULL DROP TABLE [DemandesActionCourrier];
IF OBJECT_ID(N'[DemandesDocumentDomiciliation]', N'U') IS NOT NULL DROP TABLE [DemandesDocumentDomiciliation];
IF OBJECT_ID(N'[DomiciliationTimelineEvents]', N'U') IS NOT NULL DROP TABLE [DomiciliationTimelineEvents];
IF OBJECT_ID(N'[CourriersDomiciliation]', N'U') IS NOT NULL DROP TABLE [CourriersDomiciliation];
");

        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'TaskItems') AND name = N'RelatedEntityType')
ALTER TABLE [TaskItems] ADD [RelatedEntityType] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'TaskItems') AND name = N'RelatedEntityId')
ALTER TABLE [TaskItems] ADD [RelatedEntityId] int NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'TaskItems') AND name = N'TaskType')
ALTER TABLE [TaskItems] ADD [TaskType] nvarchar(50) NULL;
");

        migrationBuilder.CreateTable(
            name: "CourriersDomiciliation",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ClientId = table.Column<int>(type: "int", nullable: false),
                DateReception = table.Column<DateTime>(type: "datetime2", nullable: false),
                Expediteur = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                TypeCourrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Confidentialite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ScanDocumentId = table.Column<int>(type: "int", nullable: true),
                ActionRequise = table.Column<bool>(type: "bit", nullable: false),
                ResponsableUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ClientNotifie = table.Column<bool>(type: "bit", nullable: false),
                ClientNotifieAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                PartageDocumentClient = table.Column<bool>(type: "bit", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                LinkedTaskItemId = table.Column<int>(type: "int", nullable: true),
                CabinetId = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_CourriersDomiciliation", x => x.Id);
                table.ForeignKey(
                    name: "FK_CourriersDomiciliation_Cabinets_CabinetId",
                    column: x => x.CabinetId,
                    principalTable: "Cabinets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CourriersDomiciliation_Clients_ClientId",
                    column: x => x.ClientId,
                    principalTable: "Clients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CourriersDomiciliation_Documents_ScanDocumentId",
                    column: x => x.ScanDocumentId,
                    principalTable: "Documents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_CourriersDomiciliation_TaskItems_LinkedTaskItemId",
                    column: x => x.LinkedTaskItemId,
                    principalTable: "TaskItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_CourriersDomiciliation_Users_ResponsableUserId",
                    column: x => x.ResponsableUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "DemandesDocumentDomiciliation",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ClientId = table.Column<int>(type: "int", nullable: false),
                DocumentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                ResponsableUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CurrentDocumentId = table.Column<int>(type: "int", nullable: true),
                RejectionReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                RejectionReasonText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                LinkedTaskItemId = table.Column<int>(type: "int", nullable: true),
                RootDocumentId = table.Column<int>(type: "int", nullable: true),
                CabinetId = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_DemandesDocumentDomiciliation", x => x.Id);
                table.ForeignKey(
                    name: "FK_DemandesDocumentDomiciliation_Cabinets_CabinetId",
                    column: x => x.CabinetId,
                    principalTable: "Cabinets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DemandesDocumentDomiciliation_Clients_ClientId",
                    column: x => x.ClientId,
                    principalTable: "Clients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DemandesDocumentDomiciliation_Documents_CurrentDocumentId",
                    column: x => x.CurrentDocumentId,
                    principalTable: "Documents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_DemandesDocumentDomiciliation_Users_ResponsableUserId",
                    column: x => x.ResponsableUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "DomiciliationTimelineEvents",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ClientId = table.Column<int>(type: "int", nullable: false),
                RelatedType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RelatedId = table.Column<int>(type: "int", nullable: false),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PreviousStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                DocumentId = table.Column<int>(type: "int", nullable: true),
                At = table.Column<DateTime>(type: "datetime2", nullable: false),
                CabinetId = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_DomiciliationTimelineEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomiciliationTimelineEvents_Cabinets_CabinetId",
                    column: x => x.CabinetId,
                    principalTable: "Cabinets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DomiciliationTimelineEvents_Clients_ClientId",
                    column: x => x.ClientId,
                    principalTable: "Clients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DemandesActionCourrier",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CourrierId = table.Column<int>(type: "int", nullable: false),
                TypeAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                AdresseForward = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ShippingReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                ClientUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                StaffUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                IdentityCheckDone = table.Column<bool>(type: "bit", nullable: false),
                ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LinkedTaskItemId = table.Column<int>(type: "int", nullable: true),
                CabinetId = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_DemandesActionCourrier", x => x.Id);
                table.ForeignKey(
                    name: "FK_DemandesActionCourrier_Cabinets_CabinetId",
                    column: x => x.CabinetId,
                    principalTable: "Cabinets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DemandesActionCourrier_CourriersDomiciliation_CourrierId",
                    column: x => x.CourrierId,
                    principalTable: "CourriersDomiciliation",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_CourriersDomiciliation_CabinetId", table: "CourriersDomiciliation", column: "CabinetId");
        migrationBuilder.CreateIndex(name: "IX_CourriersDomiciliation_ClientId", table: "CourriersDomiciliation", column: "ClientId");
        migrationBuilder.CreateIndex(name: "IX_CourriersDomiciliation_ScanDocumentId", table: "CourriersDomiciliation", column: "ScanDocumentId");
        migrationBuilder.CreateIndex(name: "IX_CourriersDomiciliation_ResponsableUserId", table: "CourriersDomiciliation", column: "ResponsableUserId");
        migrationBuilder.CreateIndex(name: "IX_CourriersDomiciliation_LinkedTaskItemId", table: "CourriersDomiciliation", column: "LinkedTaskItemId");
        migrationBuilder.CreateIndex(name: "IX_DemandesDocumentDomiciliation_CabinetId", table: "DemandesDocumentDomiciliation", column: "CabinetId");
        migrationBuilder.CreateIndex(name: "IX_DemandesDocumentDomiciliation_ClientId", table: "DemandesDocumentDomiciliation", column: "ClientId");
        migrationBuilder.CreateIndex(name: "IX_DemandesDocumentDomiciliation_CurrentDocumentId", table: "DemandesDocumentDomiciliation", column: "CurrentDocumentId");
        migrationBuilder.CreateIndex(name: "IX_DemandesDocumentDomiciliation_ResponsableUserId", table: "DemandesDocumentDomiciliation", column: "ResponsableUserId");
        migrationBuilder.CreateIndex(name: "IX_DomiciliationTimelineEvents_CabinetId", table: "DomiciliationTimelineEvents", column: "CabinetId");
        migrationBuilder.CreateIndex(name: "IX_DomiciliationTimelineEvents_ClientId", table: "DomiciliationTimelineEvents", column: "ClientId");
        migrationBuilder.CreateIndex(name: "IX_DemandesActionCourrier_CabinetId", table: "DemandesActionCourrier", column: "CabinetId");
        migrationBuilder.CreateIndex(name: "IX_DemandesActionCourrier_CourrierId", table: "DemandesActionCourrier", column: "CourrierId");
        migrationBuilder.CreateIndex(name: "IX_TaskItems_RelatedEntity", table: "TaskItems", columns: new[] { "RelatedEntityType", "RelatedEntityId" });

        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM ReferenceData WHERE Category = 'TYPE_SERVICE' AND Code = 'DOMICILIATION')
INSERT INTO ReferenceData (Category, Code, Label, SortOrder, IsActive, EffectiveFrom, CreatedAt, IsDeleted)
VALUES ('TYPE_SERVICE', 'DOMICILIATION', N'Domiciliation – Siège social et courrier', 90, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DemandesActionCourrier");
        migrationBuilder.DropTable(name: "DemandesDocumentDomiciliation");
        migrationBuilder.DropTable(name: "DomiciliationTimelineEvents");
        migrationBuilder.DropTable(name: "CourriersDomiciliation");
        migrationBuilder.DropIndex(name: "IX_TaskItems_RelatedEntity", table: "TaskItems");
        migrationBuilder.DropColumn(name: "RelatedEntityType", table: "TaskItems");
        migrationBuilder.DropColumn(name: "RelatedEntityId", table: "TaskItems");
        migrationBuilder.DropColumn(name: "TaskType", table: "TaskItems");
        migrationBuilder.Sql("DELETE FROM ReferenceData WHERE Category = 'TYPE_SERVICE' AND Code = 'DOMICILIATION';");
    }
}
