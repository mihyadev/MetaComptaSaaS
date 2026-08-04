using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaCompta.API.Migrations
{
    [DbContext(typeof(MetaComptaDbContext))]
    [Migration("20260802194500_FilterWorkflowStepTemplateOrderIndex")]
    public partial class FilterWorkflowStepTemplateOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder'
      AND object_id = OBJECT_ID(N'dbo.WorkflowStepTemplates'))
BEGIN
    DROP INDEX [IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder] ON [dbo].[WorkflowStepTemplates];
END

-- Park soft-deleted rows that still occupy StepOrder slots (legacy data)
UPDATE wst
SET wst.StepOrder = -ABS(wst.Id)
FROM [dbo].[WorkflowStepTemplates] wst
WHERE wst.IsDeleted = 1 AND wst.StepOrder >= 0;

CREATE UNIQUE INDEX [IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder]
ON [dbo].[WorkflowStepTemplates] ([WorkflowTemplateId], [StepOrder])
WHERE [IsDeleted] = 0;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder'
      AND object_id = OBJECT_ID(N'dbo.WorkflowStepTemplates'))
BEGIN
    DROP INDEX [IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder] ON [dbo].[WorkflowStepTemplates];
END

CREATE UNIQUE INDEX [IX_WorkflowStepTemplates_WorkflowTemplateId_StepOrder]
ON [dbo].[WorkflowStepTemplates] ([WorkflowTemplateId], [StepOrder]);
");
        }
    }
}
