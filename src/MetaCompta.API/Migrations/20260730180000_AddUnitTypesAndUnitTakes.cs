using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MetaCompta.Infrastructure.Data.MetaComptaDbContext))]
    [Migration("20260730180000_AddUnitTypesAndUnitTakes")]
    public partial class AddUnitTypesAndUnitTakes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UnitTypes')
BEGIN
    CREATE TABLE [UnitTypes] (
        [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [EntityTable] nvarchar(100) NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_UnitTypes_IsActive] DEFAULT 1,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_UnitTypes_IsDeleted] DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_UnitTypes_CreatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX [IX_UnitTypes_Code] ON [UnitTypes]([Code]);
END

IF NOT EXISTS (SELECT 1 FROM [UnitTypes] WHERE [Code] = N'DECLARATION')
    INSERT INTO [UnitTypes] ([Code],[Name],[Description],[EntityTable],[IsActive],[IsDeleted],[CreatedAt])
    VALUES (N'DECLARATION', N'Déclaration', N'Unité déclaration (TVA, IS, CNSS, …)', N'Declarations', 1, 0, '2026-01-01');
IF NOT EXISTS (SELECT 1 FROM [UnitTypes] WHERE [Code] = N'WORKFLOW_STEP')
    INSERT INTO [UnitTypes] ([Code],[Name],[Description],[EntityTable],[IsActive],[IsDeleted],[CreatedAt])
    VALUES (N'WORKFLOW_STEP', N'Étape workflow', N'Prise d''une étape d''instance workflow', N'WorkflowStepInstances', 1, 0, '2026-01-01');
IF NOT EXISTS (SELECT 1 FROM [UnitTypes] WHERE [Code] = N'TASK')
    INSERT INTO [UnitTypes] ([Code],[Name],[Description],[EntityTable],[IsActive],[IsDeleted],[CreatedAt])
    VALUES (N'TASK', N'Tâche', N'Prise d''une tâche (TaskItem)', N'TaskItems', 1, 0, '2026-01-01');

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UnitTakes')
BEGIN
    CREATE TABLE [UnitTakes] (
        [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CabinetId] int NOT NULL,
        [UnitTypeId] int NOT NULL,
        [UnitEntityId] int NOT NULL,
        [TakenByUserId] nvarchar(450) NOT NULL,
        [TakenAt] datetime2 NOT NULL,
        [ClosedAt] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [TaskItemId] int NULL,
        [WorkflowStepInstanceId] int NULL,
        [DeclarationId] int NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_UnitTakes_IsDeleted] DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_UnitTakes_CreatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [FK_UnitTakes_UnitTypes_UnitTypeId] FOREIGN KEY ([UnitTypeId]) REFERENCES [UnitTypes]([Id]),
        CONSTRAINT [FK_UnitTakes_Cabinets_CabinetId] FOREIGN KEY ([CabinetId]) REFERENCES [Cabinets]([Id]),
        CONSTRAINT [FK_UnitTakes_Users_TakenByUserId] FOREIGN KEY ([TakenByUserId]) REFERENCES [Users]([Id])
    );
    CREATE INDEX [IX_UnitTakes_Cabinet_Type_Entity_Closed] ON [UnitTakes]([CabinetId],[UnitTypeId],[UnitEntityId],[ClosedAt]);
    CREATE INDEX [IX_UnitTakes_Cabinet_User_Status] ON [UnitTakes]([CabinetId],[TakenByUserId],[Status]);
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TaskItems')
        ALTER TABLE [UnitTakes] ADD CONSTRAINT [FK_UnitTakes_TaskItems_TaskItemId]
            FOREIGN KEY ([TaskItemId]) REFERENCES [TaskItems]([Id]);
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowStepInstances')
        ALTER TABLE [UnitTakes] ADD CONSTRAINT [FK_UnitTakes_WorkflowStepInstances_WorkflowStepInstanceId]
            FOREIGN KEY ([WorkflowStepInstanceId]) REFERENCES [WorkflowStepInstances]([Id]);
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Declarations')
        ALTER TABLE [UnitTakes] ADD CONSTRAINT [FK_UnitTakes_Declarations_DeclarationId]
            FOREIGN KEY ([DeclarationId]) REFERENCES [Declarations]([Id]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UnitTakes') DROP TABLE [UnitTakes];
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UnitTypes') DROP TABLE [UnitTypes];
");
        }
    }
}
