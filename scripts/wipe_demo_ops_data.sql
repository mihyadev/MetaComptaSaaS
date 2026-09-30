-- Demo wipe for exercice-centric reset (DEV ONLY).
-- Keeps: Cabinets, Users, Roles, ReferenceData, Clients master, chart/journals templates.
-- Removes: declarations (+children), workflows on decls, payroll, écritures, rapprochements,
--          ServiceContrats, Exercices/Periodes.
-- Usage: sqlcmd ... -i wipe_demo_ops_data.sql
-- Or: scripts/wipe_demo_ops_data.ps1 -ConfirmWipe

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
BEGIN TRAN;

-- Unit takes linked to declarations / WF
IF OBJECT_ID(N'dbo.UnitTakes', N'U') IS NOT NULL
    DELETE FROM UnitTakes;

-- Workflow history / steps / instances
IF OBJECT_ID(N'dbo.WorkflowHistories', N'U') IS NOT NULL
    DELETE FROM WorkflowHistories;
IF OBJECT_ID(N'dbo.WorkflowStepInstances', N'U') IS NOT NULL
    DELETE FROM WorkflowStepInstances;
IF OBJECT_ID(N'dbo.WorkflowInstances', N'U') IS NOT NULL
    DELETE FROM WorkflowInstances;

-- Tasks / messages tied to declarations (best-effort)
IF OBJECT_ID(N'dbo.TaskItems', N'U') IS NOT NULL
    DELETE FROM TaskItems WHERE DeclarationId IS NOT NULL;
IF OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL
    DELETE FROM Documents WHERE DeclarationId IS NOT NULL;

-- Declaration children
IF OBJECT_ID(N'dbo.DeclarationHistories', N'U') IS NOT NULL DELETE FROM DeclarationHistories;
IF OBJECT_ID(N'dbo.DeclarationChecklistItems', N'U') IS NOT NULL DELETE FROM DeclarationChecklistItems;
IF OBJECT_ID(N'dbo.TVALignesFactures', N'U') IS NOT NULL DELETE FROM TVALignesFactures;
IF OBJECT_ID(N'dbo.DeclarationsTVA', N'U') IS NOT NULL DELETE FROM DeclarationsTVA;
IF OBJECT_ID(N'dbo.DeclarationsIS', N'U') IS NOT NULL DELETE FROM DeclarationsIS;
IF OBJECT_ID(N'dbo.DeclarationsCNSS', N'U') IS NOT NULL DELETE FROM DeclarationsCNSS;
IF OBJECT_ID(N'dbo.Declarations', N'U') IS NOT NULL DELETE FROM Declarations;

-- Payroll
IF OBJECT_ID(N'dbo.PaySlipLignes', N'U') IS NOT NULL DELETE FROM PaySlipLignes;
IF OBJECT_ID(N'dbo.PaySlips', N'U') IS NOT NULL DELETE FROM PaySlips;
IF OBJECT_ID(N'dbo.CumulEmployes', N'U') IS NOT NULL DELETE FROM CumulEmployes;
IF OBJECT_ID(N'dbo.PayrollPeriods', N'U') IS NOT NULL DELETE FROM PayrollPeriods;

-- Ledger movements
IF OBJECT_ID(N'dbo.RapprochementLignes', N'U') IS NOT NULL DELETE FROM RapprochementLignes;
IF OBJECT_ID(N'dbo.RapprochementsBancaires', N'U') IS NOT NULL DELETE FROM RapprochementsBancaires;
IF OBJECT_ID(N'dbo.LignesEcritures', N'U') IS NOT NULL DELETE FROM LignesEcritures;
IF OBJECT_ID(N'dbo.EcrituresComptables', N'U') IS NOT NULL DELETE FROM EcrituresComptables;

-- Services + exercices
IF OBJECT_ID(N'dbo.ServiceContrats', N'U') IS NOT NULL DELETE FROM ServiceContrats;
IF OBJECT_ID(N'dbo.PeriodesComptables', N'U') IS NOT NULL DELETE FROM PeriodesComptables;
IF OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL DELETE FROM ExercicesComptables;

COMMIT;
PRINT 'Demo ops data wiped. Clients/users/cabinets/plan kept. Create exercices manually.';
