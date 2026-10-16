using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaCompta.API.Migrations
{
    [DbContext(typeof(MetaCompta.Infrastructure.Data.MetaComptaDbContext))]
    [Migration("20260808220000_AddExerciceComptableIdToOps")]
    public partial class AddExerciceComptableIdToOps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Declarations', 'ExerciceComptableId') IS NULL
    ALTER TABLE [Declarations] ADD [ExerciceComptableId] int NULL;
IF COL_LENGTH('ServiceContrats', 'ExerciceComptableId') IS NULL
    ALTER TABLE [ServiceContrats] ADD [ExerciceComptableId] int NULL;
IF COL_LENGTH('PayrollPeriods', 'ExerciceComptableId') IS NULL
    ALTER TABLE [PayrollPeriods] ADD [ExerciceComptableId] int NULL;
IF COL_LENGTH('WorkflowInstances', 'ExerciceComptableId') IS NULL
    ALTER TABLE [WorkflowInstances] ADD [ExerciceComptableId] int NULL;
IF OBJECT_ID(N'dbo.RapprochementsBancaires', N'U') IS NOT NULL
   AND COL_LENGTH('RapprochementsBancaires', 'ExerciceComptableId') IS NULL
    ALTER TABLE [RapprochementsBancaires] ADD [ExerciceComptableId] int NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Declarations_ExercicesComptables_ExerciceComptableId')
    AND OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL
    ALTER TABLE [Declarations] ADD CONSTRAINT [FK_Declarations_ExercicesComptables_ExerciceComptableId]
        FOREIGN KEY ([ExerciceComptableId]) REFERENCES [ExercicesComptables]([Id]) ON DELETE SET NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceContrats_ExercicesComptables_ExerciceComptableId')
    AND OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL
    ALTER TABLE [ServiceContrats] ADD CONSTRAINT [FK_ServiceContrats_ExercicesComptables_ExerciceComptableId]
        FOREIGN KEY ([ExerciceComptableId]) REFERENCES [ExercicesComptables]([Id]) ON DELETE SET NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PayrollPeriods_ExercicesComptables_ExerciceComptableId')
    AND OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL
    ALTER TABLE [PayrollPeriods] ADD CONSTRAINT [FK_PayrollPeriods_ExercicesComptables_ExerciceComptableId]
        FOREIGN KEY ([ExerciceComptableId]) REFERENCES [ExercicesComptables]([Id]) ON DELETE SET NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowInstances_ExercicesComptables_ExerciceComptableId')
    AND OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL
    ALTER TABLE [WorkflowInstances] ADD CONSTRAINT [FK_WorkflowInstances_ExercicesComptables_ExerciceComptableId]
        FOREIGN KEY ([ExerciceComptableId]) REFERENCES [ExercicesComptables]([Id]) ON DELETE SET NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RapprochementsBancaires_ExercicesComptables_ExerciceComptableId')
    AND OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.RapprochementsBancaires', N'U') IS NOT NULL
    ALTER TABLE [RapprochementsBancaires] ADD CONSTRAINT [FK_RapprochementsBancaires_ExercicesComptables_ExerciceComptableId]
        FOREIGN KEY ([ExerciceComptableId]) REFERENCES [ExercicesComptables]([Id]) ON DELETE SET NULL;

-- Type/Periode may be nvarchar(max); index only ClientId+ExerciceComptableId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Declarations_ClientId_ExerciceComptableId' AND object_id = OBJECT_ID('Declarations'))
    CREATE INDEX [IX_Declarations_ClientId_ExerciceComptableId]
        ON [Declarations]([ClientId],[ExerciceComptableId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ServiceContrats_ExerciceComptableId' AND object_id = OBJECT_ID('ServiceContrats'))
    CREATE INDEX [IX_ServiceContrats_ExerciceComptableId] ON [ServiceContrats]([ExerciceComptableId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PayrollPeriods_ExerciceComptableId' AND object_id = OBJECT_ID('PayrollPeriods'))
    CREATE INDEX [IX_PayrollPeriods_ExerciceComptableId] ON [PayrollPeriods]([ExerciceComptableId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowInstances_ExerciceComptableId' AND object_id = OBJECT_ID('WorkflowInstances'))
    CREATE INDEX [IX_WorkflowInstances_ExerciceComptableId] ON [WorkflowInstances]([ExerciceComptableId]);
IF OBJECT_ID(N'dbo.RapprochementsBancaires', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RapprochementsBancaires_ExerciceComptableId' AND object_id = OBJECT_ID('RapprochementsBancaires'))
    CREATE INDEX [IX_RapprochementsBancaires_ExerciceComptableId] ON [RapprochementsBancaires]([ExerciceComptableId]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Declarations_ExercicesComptables_ExerciceComptableId')
    ALTER TABLE [Declarations] DROP CONSTRAINT [FK_Declarations_ExercicesComptables_ExerciceComptableId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceContrats_ExercicesComptables_ExerciceComptableId')
    ALTER TABLE [ServiceContrats] DROP CONSTRAINT [FK_ServiceContrats_ExercicesComptables_ExerciceComptableId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PayrollPeriods_ExercicesComptables_ExerciceComptableId')
    ALTER TABLE [PayrollPeriods] DROP CONSTRAINT [FK_PayrollPeriods_ExercicesComptables_ExerciceComptableId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowInstances_ExercicesComptables_ExerciceComptableId')
    ALTER TABLE [WorkflowInstances] DROP CONSTRAINT [FK_WorkflowInstances_ExercicesComptables_ExerciceComptableId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RapprochementsBancaires_ExercicesComptables_ExerciceComptableId')
    ALTER TABLE [RapprochementsBancaires] DROP CONSTRAINT [FK_RapprochementsBancaires_ExercicesComptables_ExerciceComptableId];
");
        }
    }
}
