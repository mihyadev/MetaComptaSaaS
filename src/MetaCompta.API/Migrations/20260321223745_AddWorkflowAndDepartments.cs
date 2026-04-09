using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAndDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HeadOfDepartmentUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadOfDepartmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Users_HeadOfDepartmentId",
                        column: x => x.HeadOfDepartmentId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarationType = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsLead = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_DepartmentMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentMembers_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentMembers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientWorkflowConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DeclarationType = table.Column<int>(type: "int", nullable: false),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: true),
                    ClientValidationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DocumentRequestEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SLAMultiplier = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AutoStartWorkflow = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryAssigneeUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ClientWorkflowConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientWorkflowConfigs_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientWorkflowConfigs_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientWorkflowConfigs_Users_PrimaryAssigneeUserId",
                        column: x => x.PrimaryAssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientWorkflowConfigs_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: false),
                    DeclarationId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CurrentStepOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Declarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "Declarations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Users_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStepTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepType = table.Column<int>(type: "int", nullable: false),
                    AssignmentType = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedToRoleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToDepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsClientStep = table.Column<bool>(type: "bit", nullable: false),
                    RequiredPermission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SLAHours = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsBlocking = table.Column<bool>(type: "bit", nullable: false),
                    AutoCreateTask = table.Column<bool>(type: "bit", nullable: false),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    TaskPriority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutoNotify = table.Column<bool>(type: "bit", nullable: false),
                    ValidationRulesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarationStatusOnComplete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarationStatusOnEntry = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WorkflowStepTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStepTemplates_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStepTemplates_Departments_AssignedToDepartmentId",
                        column: x => x.AssignedToDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowStepTemplates_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowStepTemplates_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStepInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    WorkflowStepTemplateId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlockReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskItemId = table.Column<int>(type: "int", nullable: true),
                    ResultDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WorkflowStepInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_TaskItems_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "TaskItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStepInstances_WorkflowStepTemplates_WorkflowStepTemplateId",
                        column: x => x.WorkflowStepTemplateId,
                        principalTable: "WorkflowStepTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    WorkflowStepInstanceId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_Cabinets_CabinetId",
                        column: x => x.CabinetId,
                        principalTable: "Cabinets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowStepInstances_WorkflowStepInstanceId",
                        column: x => x.WorkflowStepInstanceId,
                        principalTable: "WorkflowStepInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(1537));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2318));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2329));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2331));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2334));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2338));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2343));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2346));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2349));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2352));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2355));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2357));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2360));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2364));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2366));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2369));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2371));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2374));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2377));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2412));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2414));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2417));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2422));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2425));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2428));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2431));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2433));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2435));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2438));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2442));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2444));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2448));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2453));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2455));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2457));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2459));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2462));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2464));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2466));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2469));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2472));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2500));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2505));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2507));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2512));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2515));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2517));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Module", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 52, "View", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2520), null, null, null, "Permission to View in Workflow module", false, "Workflow", "Workflow.View", null, null },
                    { 53, "Create", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2522), null, null, null, "Permission to Create in Workflow module", false, "Workflow", "Workflow.Create", null, null },
                    { 54, "Edit", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2525), null, null, null, "Permission to Edit in Workflow module", false, "Workflow", "Workflow.Edit", null, null },
                    { 55, "Delete", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2527), null, null, null, "Permission to Delete in Workflow module", false, "Workflow", "Workflow.Delete", null, null },
                    { 56, "Configure", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2529), null, null, null, "Permission to Configure in Workflow module", false, "Workflow", "Workflow.Configure", null, null },
                    { 57, "Advance", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2531), null, null, null, "Permission to Advance in Workflow module", false, "Workflow", "Workflow.Advance", null, null },
                    { 58, "Validate", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2533), null, null, null, "Permission to Validate in Workflow module", false, "Workflow", "Workflow.Validate", null, null },
                    { 59, "Reassign", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2536), null, null, null, "Permission to Reassign in Workflow module", false, "Workflow", "Workflow.Reassign", null, null },
                    { 60, "Cancel", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2538), null, null, null, "Permission to Cancel in Workflow module", false, "Workflow", "Workflow.Cancel", null, null },
                    { 61, "Monitor", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2540), null, null, null, "Permission to Monitor in Workflow module", false, "Workflow", "Workflow.Monitor", null, null },
                    { 62, "View", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2543), null, null, null, "Permission to View in Department module", false, "Department", "Department.View", null, null },
                    { 63, "Create", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2546), null, null, null, "Permission to Create in Department module", false, "Department", "Department.Create", null, null },
                    { 64, "Edit", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2548), null, null, null, "Permission to Edit in Department module", false, "Department", "Department.Edit", null, null },
                    { 65, "Delete", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2551), null, null, null, "Permission to Delete in Department module", false, "Department", "Department.Delete", null, null },
                    { 66, "ManageMembers", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(2553), null, null, null, "Permission to ManageMembers in Department module", false, "Department", "Department.ManageMembers", null, null }
                });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(3557));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4423));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4426));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4428));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4445));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4447));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4471));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4472));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4474));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4476));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4478));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4646));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4648));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4651));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4663));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4665));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4667));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4669));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4670));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4702));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4704));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4706), "bg-primary-subtle text-primary" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4901), "bg-purple-subtle text-purple" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4903), "bg-success-subtle text-success" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#0891b2", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4905), "bg-info-subtle text-info" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#d97706", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4917), "bg-warning-subtle text-warning" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4919), "bg-secondary-subtle text-secondary" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4928), "bg-primary-subtle text-primary" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4930), "bg-purple-subtle text-purple" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4932), "bg-success-subtle text-success" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Label" },
                values: new object[] { "SCS", "#0891b2", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4934), "bg-info-subtle text-info", "SCS" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4921), "bg-success-subtle text-success" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4923), "bg-warning-subtle text-warning" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#0891b2", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4925), "bg-info-subtle text-info" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4926), "bg-purple-subtle text-purple" });

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "Couleur", "CreatedAt", "CreatedBy", "CssClass", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "Icone", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 36, null, "FORME_JURIDIQUE", "SCA", "#d97706", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4935), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "SCA", null, null, null, null, 5, null, null, null },
                    { 37, null, "FORME_JURIDIQUE", "AUTO_ENTREPRENEUR", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4937), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Auto-Entrepreneur", null, null, null, null, 6, null, null, null },
                    { 38, null, "FORME_JURIDIQUE", "PERSONNE_PHYSIQUE", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4947), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Personne Physique", null, null, null, null, 7, null, null, null },
                    { 39, null, "FORME_JURIDIQUE", "ASSOCIATION", "#ec4899", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4949), null, "bg-pink-subtle text-pink", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Association", null, null, null, null, 8, null, null, null },
                    { 40, null, "FORME_JURIDIQUE", "COOPERATIVE", "#14b8a6", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4951), null, "bg-teal-subtle text-teal", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Coopérative", null, null, null, null, 9, null, null, null },
                    { 41, null, "FORME_JURIDIQUE", "EI", "#6366f1", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4953), null, "bg-indigo-subtle text-indigo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Entreprise Individuelle", null, null, null, null, 10, null, null, null },
                    { 42, null, "STATUT_CLIENT", "Active", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(4955), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Actif", null, null, null, null, 1, null, null, null },
                    { 43, null, "STATUT_CLIENT", "Inactive", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5036), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Inactif", null, null, null, null, 2, null, null, null },
                    { 44, null, "STATUT_CLIENT", "Dormante", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5039), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pause-circle", true, false, "Dormant", null, null, null, null, 3, null, null, null },
                    { 45, null, "STATUT_CLIENT", "Dissoute", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5041), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-slash-circle", true, false, "Dissout", null, null, null, null, 4, null, null, null },
                    { 46, null, "STATUT_EMPLOYE", "Actif", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5043), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Actif", null, null, null, null, 1, null, null, null },
                    { 47, null, "STATUT_EMPLOYE", "Inactif", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5045), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Inactif", null, null, null, null, 2, null, null, null },
                    { 48, null, "STATUT_EMPLOYE", "Suspendu", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5046), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pause-circle", true, false, "Suspendu", null, null, null, null, 3, null, null, null },
                    { 49, null, "TYPE_DECLARATION", "TVA", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5048), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-receipt", true, false, "TVA", null, null, null, null, 1, null, null, null },
                    { 50, null, "TYPE_DECLARATION", "IS", "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5059), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-building", true, false, "Impôt sur les Sociétés", null, null, null, null, 2, null, null, null },
                    { 51, null, "TYPE_DECLARATION", "IR", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5061), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person", true, false, "Impôt sur le Revenu", null, null, null, null, 3, null, null, null },
                    { 52, null, "TYPE_DECLARATION", "CNSS", "#0891b2", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5063), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-shield", true, false, "CNSS", null, null, null, null, 4, null, null, null },
                    { 53, null, "TYPE_DECLARATION", "IR_SALAIRES", "#d97706", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5065), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash", true, false, "IR Salaires", null, null, null, null, 5, null, null, null },
                    { 54, null, "STATUT_DECLARATION", "Brouillon", "#94a3b8", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5067), null, "badge-brouillon", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 55, null, "STATUT_DECLARATION", "ACompleter", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5086), null, "badge-acompleter", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-exclamation-circle", true, false, "À Compléter", null, null, null, null, 2, null, null, null },
                    { 56, null, "STATUT_DECLARATION", "Completee", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5088), null, "badge-completee", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check", true, false, "Complétée", null, null, null, null, 3, null, null, null },
                    { 57, null, "STATUT_DECLARATION", "EnRevision", "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5090), null, "badge-enrevision", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-eye", true, false, "En Révision", null, null, null, null, 4, null, null, null },
                    { 58, null, "STATUT_DECLARATION", "Validee", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5092), null, "badge-validee", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Validée", null, null, null, null, 5, null, null, null },
                    { 59, null, "STATUT_DECLARATION", "Soumise", "#166534", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5094), null, "badge-soumise", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-send", true, false, "Soumise", null, null, null, null, 6, null, null, null },
                    { 60, null, "STATUT_DECLARATION", "Refusee", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5096), null, "badge-refusee", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Refusée", null, null, null, null, 7, null, null, null },
                    { 61, null, "TYPE_TVA", "Mensuelle", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5106), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Mensuelle", null, null, null, null, 1, null, null, null },
                    { 62, null, "TYPE_TVA", "Trimestrielle", "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5108), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Trimestrielle", null, null, null, null, 2, null, null, null },
                    { 63, null, "TYPE_TVA", "Exonere", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5110), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Exonéré", null, null, null, null, 3, null, null, null },
                    { 64, null, "VILLE", "CASABLANCA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5112), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Casablanca", null, null, null, null, 1, null, null, null },
                    { 65, null, "VILLE", "RABAT", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5114), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Rabat", null, null, null, null, 2, null, null, null },
                    { 66, null, "VILLE", "MARRAKECH", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5115), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Marrakech", null, null, null, null, 3, null, null, null },
                    { 67, null, "VILLE", "FES", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5117), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Fès", null, null, null, null, 4, null, null, null },
                    { 68, null, "VILLE", "TANGER", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5118), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Tanger", null, null, null, null, 5, null, null, null },
                    { 69, null, "VILLE", "AGADIR", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5120), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Agadir", null, null, null, null, 6, null, null, null },
                    { 70, null, "VILLE", "MEKNES", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5122), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Meknès", null, null, null, null, 7, null, null, null },
                    { 71, null, "VILLE", "OUJDA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5123), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Oujda", null, null, null, null, 8, null, null, null },
                    { 72, null, "VILLE", "KENITRA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5125), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Kénitra", null, null, null, null, 9, null, null, null },
                    { 73, null, "VILLE", "TETOUAN", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5126), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Tétouan", null, null, null, null, 10, null, null, null },
                    { 74, null, "VILLE", "SALE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5128), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Salé", null, null, null, null, 11, null, null, null },
                    { 75, null, "VILLE", "NADOR", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5129), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Nador", null, null, null, null, 12, null, null, null },
                    { 76, null, "VILLE", "MOHAMMEDIA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5139), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Mohammédia", null, null, null, null, 13, null, null, null },
                    { 77, null, "VILLE", "EL_JADIDA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5141), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "El Jadida", null, null, null, null, 14, null, null, null },
                    { 78, null, "VILLE", "BENI_MELLAL", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5142), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Béni Mellal", null, null, null, null, 15, null, null, null },
                    { 79, null, "VILLE", "KHOURIBGA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5144), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Khouribga", null, null, null, null, 16, null, null, null },
                    { 80, null, "VILLE", "SETTAT", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5145), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Settat", null, null, null, null, 17, null, null, null },
                    { 81, null, "VILLE", "SAFI", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5147), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Safi", null, null, null, null, 18, null, null, null },
                    { 82, null, "VILLE", "LAAYOUNE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5149), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Laâyoune", null, null, null, null, 19, null, null, null },
                    { 83, null, "VILLE", "DAKHLA", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5150), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Dakhla", null, null, null, null, 20, null, null, null },
                    { 84, null, "SITUATION_FAMILIALE", "Celibataire", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5152), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person", true, false, "Célibataire", null, null, null, null, 1, null, null, null },
                    { 85, null, "SITUATION_FAMILIALE", "Marie", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5153), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-people", true, false, "Marié(e)", null, null, null, null, 2, null, null, null },
                    { 86, null, "SITUATION_FAMILIALE", "Divorce", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5155), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person-dash", true, false, "Divorcé(e)", null, null, null, null, 3, null, null, null },
                    { 87, null, "SITUATION_FAMILIALE", "Veuf", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5157), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person-x", true, false, "Veuf(ve)", null, null, null, null, 4, null, null, null },
                    { 88, null, "SEXE", "M", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5159), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Masculin", null, null, null, null, 1, null, null, null },
                    { 89, null, "SEXE", "F", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5160), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Féminin", null, null, null, null, 2, null, null, null },
                    { 90, null, "MODE_PAIEMENT", "Virement", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5214), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-bank", true, false, "Virement bancaire", null, null, null, null, 1, null, null, null },
                    { 91, null, "MODE_PAIEMENT", "Cheque", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5216), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-credit-card", true, false, "Chèque", null, null, null, null, 2, null, null, null },
                    { 92, null, "MODE_PAIEMENT", "Especes", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5218), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash", true, false, "Espèces", null, null, null, null, 3, null, null, null },
                    { 93, null, "TYPE_ABSENCE", "Annuel", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5220), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Congé annuel", null, null, null, null, 1, null, null, null },
                    { 94, null, "TYPE_ABSENCE", "Maladie", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5221), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Congé maladie", null, null, null, null, 2, null, null, null },
                    { 95, null, "TYPE_ABSENCE", "SansSolde", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5223), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Congé sans solde", null, null, null, null, 3, null, null, null },
                    { 96, null, "TYPE_ABSENCE", "Maternite", "#ec4899", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5225), null, "bg-pink-subtle text-pink", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Congé maternité", null, null, null, null, 4, null, null, null },
                    { 97, null, "TYPE_ABSENCE", "Exceptionnel", "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5227), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Congé exceptionnel", null, null, null, null, 5, null, null, null },
                    { 98, null, "STATUT_DOCUMENT", "EnAttente", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5229), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clock", true, false, "En attente", null, null, null, null, 1, null, null, null },
                    { 99, null, "STATUT_DOCUMENT", "Valide", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5230), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Validé", null, null, null, null, 2, null, null, null },
                    { 100, null, "STATUT_DOCUMENT", "Rejete", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5232), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Rejeté", null, null, null, null, 3, null, null, null },
                    { 101, null, "STATUT_DOCUMENT", "Archive", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5234), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-archive", true, false, "Archivé", null, null, null, null, 4, null, null, null },
                    { 102, null, "PRIORITE_TACHE", "Faible", "#94a3b8", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5245), null, "badge-faible", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-down", true, false, "Faible", null, null, null, null, 1, null, null, null },
                    { 103, null, "PRIORITE_TACHE", "Moyenne", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5247), null, "badge-moyenne", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-right", true, false, "Moyenne", null, null, null, null, 2, null, null, null },
                    { 104, null, "PRIORITE_TACHE", "Haute", "#ea580c", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5249), null, "badge-haute", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-up", true, false, "Haute", null, null, null, null, 3, null, null, null },
                    { 105, null, "PRIORITE_TACHE", "Urgente", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5251), null, "badge-urgente", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-exclamation-triangle", true, false, "Urgente", null, null, null, null, 4, null, null, null },
                    { 106, null, "STATUT_TACHE", "AFaire", "#94a3b8", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5252), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-circle", true, false, "À Faire", null, null, null, null, 1, null, null, null },
                    { 107, null, "STATUT_TACHE", "EnCours", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5254), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-play-circle", true, false, "En Cours", null, null, null, null, 2, null, null, null },
                    { 108, null, "STATUT_TACHE", "EnAttenteRevision", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5256), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-eye", true, false, "En Attente Révision", null, null, null, null, 3, null, null, null },
                    { 109, null, "STATUT_TACHE", "Termine", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5258), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Terminé", null, null, null, null, 4, null, null, null },
                    { 110, null, "STATUT_TACHE", "Annule", "#dc2626", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5260), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-circle", true, false, "Annulé", null, null, null, null, 5, null, null, null },
                    { 111, null, "STATUT_PAIE", "Brouillon", "#94a3b8", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5262), null, "badge-brouillon", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-pencil", true, false, "Brouillon", null, null, null, null, 1, null, null, null },
                    { 112, null, "STATUT_PAIE", "EnCours", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5264), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-play-circle", true, false, "En Cours", null, null, null, null, 2, null, null, null },
                    { 113, null, "STATUT_PAIE", "EnAttenteValidation", "#f59e0b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5266), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clock", true, false, "En Attente Validation", null, null, null, null, 3, null, null, null },
                    { 114, null, "STATUT_PAIE", "Validee", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5276), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-check-circle", true, false, "Validée", null, null, null, null, 4, null, null, null },
                    { 115, null, "STATUT_PAIE", "Cloturee", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5278), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-lock", true, false, "Clôturée", null, null, null, null, 5, null, null, null },
                    { 116, null, "SECTEUR_ACTIVITE", "COMMERCE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5280), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Commerce", null, null, null, null, 1, null, null, null },
                    { 117, null, "SECTEUR_ACTIVITE", "SERVICES", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5282), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Services", null, null, null, null, 2, null, null, null },
                    { 118, null, "SECTEUR_ACTIVITE", "INDUSTRIE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5283), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Industrie", null, null, null, null, 3, null, null, null },
                    { 119, null, "SECTEUR_ACTIVITE", "BTP", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5285), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "BTP", null, null, null, null, 4, null, null, null },
                    { 120, null, "SECTEUR_ACTIVITE", "IMMOBILIER", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5286), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Immobilier", null, null, null, null, 5, null, null, null },
                    { 121, null, "SECTEUR_ACTIVITE", "TRANSPORT", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5288), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Transport & Logistique", null, null, null, null, 6, null, null, null },
                    { 122, null, "SECTEUR_ACTIVITE", "RESTAURATION", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5290), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Hôtellerie & Restauration", null, null, null, null, 7, null, null, null },
                    { 123, null, "SECTEUR_ACTIVITE", "TECH", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5291), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Technologies & IT", null, null, null, null, 8, null, null, null },
                    { 124, null, "SECTEUR_ACTIVITE", "AGRICULTURE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5293), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Agriculture", null, null, null, null, 9, null, null, null },
                    { 125, null, "SECTEUR_ACTIVITE", "SANTE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5330), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Santé", null, null, null, null, 10, null, null, null },
                    { 126, null, "SECTEUR_ACTIVITE", "EDUCATION", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5331), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Éducation & Formation", null, null, null, null, 11, null, null, null },
                    { 127, null, "SECTEUR_ACTIVITE", "AUTRE", null, new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5333), null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "Autre", null, null, null, null, 99, null, null, null },
                    { 128, null, "CATEGORIE_DOCUMENT", "Legal", "#2563eb", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5334), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-file-earmark-text", true, false, "Documents Légaux", null, null, null, null, 1, null, null, null },
                    { 129, null, "CATEGORIE_DOCUMENT", "Fiscal", "#7c3aed", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5336), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calculator", true, false, "Documents Fiscaux", null, null, null, null, 2, null, null, null },
                    { 130, null, "CATEGORIE_DOCUMENT", "Social", "#059669", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5338), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-people", true, false, "Documents Sociaux", null, null, null, null, 3, null, null, null },
                    { 131, null, "CATEGORIE_DOCUMENT", "Comptable", "#0891b2", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5340), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-journal-text", true, false, "Documents Comptables", null, null, null, null, 4, null, null, null },
                    { 132, null, "CATEGORIE_DOCUMENT", "RH", "#d97706", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5342), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person-badge", true, false, "Documents RH", null, null, null, null, 5, null, null, null },
                    { 133, null, "CATEGORIE_DOCUMENT", "Admin", "#64748b", new DateTime(2026, 3, 21, 22, 37, 44, 411, DateTimeKind.Utc).AddTicks(5344), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-folder", true, false, "Documents Administratifs", null, null, null, null, 6, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 410, DateTimeKind.Utc).AddTicks(5976));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 410, DateTimeKind.Utc).AddTicks(6977));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 22, 37, 44, 410, DateTimeKind.Utc).AddTicks(6979));

            migrationBuilder.CreateIndex(
                name: "IX_ClientWorkflowConfigs_CabinetId_ClientId_DeclarationType",
                table: "ClientWorkflowConfigs",
                columns: new[] { "CabinetId", "ClientId", "DeclarationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientWorkflowConfigs_ClientId",
                table: "ClientWorkflowConfigs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWorkflowConfigs_PrimaryAssigneeUserId",
                table: "ClientWorkflowConfigs",
                column: "PrimaryAssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWorkflowConfigs_WorkflowTemplateId",
                table: "ClientWorkflowConfigs",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMembers_CabinetId",
                table: "DepartmentMembers",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMembers_DepartmentId_UserId",
                table: "DepartmentMembers",
                columns: new[] { "DepartmentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMembers_UserId",
                table: "DepartmentMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CabinetId_Code",
                table: "Departments",
                columns: new[] { "CabinetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_ActionDate",
                table: "WorkflowHistories",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_CabinetId",
                table: "WorkflowHistories",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowInstanceId",
                table: "WorkflowHistories",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowStepInstanceId",
                table: "WorkflowHistories",
                column: "WorkflowStepInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CabinetId_Status",
                table: "WorkflowInstances",
                columns: new[] { "CabinetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ClientId",
                table: "WorkflowInstances",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_DeclarationId",
                table: "WorkflowInstances",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_StartedByUserId",
                table: "WorkflowInstances",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowTemplateId",
                table: "WorkflowInstances",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_AssignedToUserId",
                table: "WorkflowStepInstances",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_CabinetId",
                table: "WorkflowStepInstances",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_CompletedByUserId",
                table: "WorkflowStepInstances",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_TaskItemId",
                table: "WorkflowStepInstances",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_WorkflowInstanceId_StepOrder",
                table: "WorkflowStepInstances",
                columns: new[] { "WorkflowInstanceId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepInstances_WorkflowStepTemplateId",
                table: "WorkflowStepInstances",
                column: "WorkflowStepTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepTemplates_AssignedToDepartmentId",
                table: "WorkflowStepTemplates",
                column: "AssignedToDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepTemplates_AssignedToUserId",
                table: "WorkflowStepTemplates",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepTemplates_CabinetId",
                table: "WorkflowStepTemplates",
                column: "CabinetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder",
                table: "WorkflowStepTemplates",
                columns: new[] { "WorkflowTemplateId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_CabinetId_DeclarationType_ClientId_IsDefault",
                table: "WorkflowTemplates",
                columns: new[] { "CabinetId", "DeclarationType", "ClientId", "IsDefault" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_ClientId",
                table: "WorkflowTemplates",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientWorkflowConfigs");

            migrationBuilder.DropTable(
                name: "DepartmentMembers");

            migrationBuilder.DropTable(
                name: "WorkflowHistories");

            migrationBuilder.DropTable(
                name: "WorkflowStepInstances");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowStepTemplates");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(1765));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2278));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2283));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2285));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2288));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2291));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2295));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2298));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2315));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2318));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2321));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2323));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2325));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2328));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2330));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2333));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2335));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2338));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2340));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2343));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2345));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2347));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2349));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2352));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2354));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2356));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2359));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2361));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2363));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2365));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2367));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2370));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2379));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2383));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2385));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2387));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2389));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2391));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2393));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2396));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2398));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2400));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2403));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2406));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2408));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2411));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2413));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2415));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2417));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(2422));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(3323));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4232));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4234));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4236));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4238));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4250));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4274));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4275));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4277));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4281));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4455));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4460));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4463));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4465));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4477));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4479));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4481));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4482));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4484));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4486));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4487), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4489), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4490), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4492), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4493), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4495), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4496), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4498), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4500), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Label" },
                values: new object[] { "EI", null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4501), null, "Entreprise Individuelle" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4504), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4505), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4507), null });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Couleur", "CreatedAt", "CssClass" },
                values: new object[] { null, new DateTime(2026, 3, 15, 15, 40, 36, 809, DateTimeKind.Utc).AddTicks(4508), null });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(6442));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(7303));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 15, 40, 36, 808, DateTimeKind.Utc).AddTicks(7305));
        }
    }
}
