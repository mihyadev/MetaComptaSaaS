-- FULL CLEAN SLATE (DEV ONLY) — irreversible.
-- KEEPS: Cabinets, cabinet staff Users (+ roles/claims), Roles, Permissions,
--        ReferenceData, SubscriptionPlans, Departments, active cabinet WF templates,
--        payroll engine catalog (Rubriques/Constantes/Regles), UnitTypes, migrations.
-- DELETES: Clients, client users, client employees, contracts, declarations,
--          exercices, ledger, ALL workflow instances/history/steps, tasks, docs,
--          commercial, client WF copies, archived "(ancien)" WF templates.
-- Usage: scripts/wipe_clean_slate.ps1 -ConfirmWipe

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;

BEGIN TRAN;

-- ── Identify Client-role users to remove ─────────────────────────────
IF OBJECT_ID(N'tempdb..#ClientUsers') IS NOT NULL DROP TABLE #ClientUsers;
CREATE TABLE #ClientUsers (UserId NVARCHAR(450) NOT NULL PRIMARY KEY);

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
BEGIN
    INSERT INTO #ClientUsers (UserId)
    SELECT DISTINCT ur.UserId
    FROM AspNetUserRoles ur
    INNER JOIN Roles r ON r.Id = ur.RoleId
    WHERE r.Name = N'Client';
END

-- ── Unit takes / WF runtime (ALL instances — active, cancelled, archived) ─
IF OBJECT_ID(N'dbo.UnitTakes', N'U') IS NOT NULL DELETE FROM UnitTakes;
IF OBJECT_ID(N'dbo.WorkflowHistories', N'U') IS NOT NULL DELETE FROM WorkflowHistories;
IF OBJECT_ID(N'dbo.WorkflowStepInstances', N'U') IS NOT NULL DELETE FROM WorkflowStepInstances;
IF OBJECT_ID(N'dbo.WorkflowInstances', N'U') IS NOT NULL DELETE FROM WorkflowInstances;
IF OBJECT_ID(N'dbo.ClientWorkflowConfigs', N'U') IS NOT NULL DELETE FROM ClientWorkflowConfigs;

-- ── Tasks / messages / notifications / documents ─────────────────────
IF OBJECT_ID(N'dbo.TaskComments', N'U') IS NOT NULL DELETE FROM TaskComments;
IF OBJECT_ID(N'dbo.TaskItems', N'U') IS NOT NULL DELETE FROM TaskItems;
IF OBJECT_ID(N'dbo.Messages', N'U') IS NOT NULL DELETE FROM Messages;
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL DELETE FROM Notifications;
IF OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL DELETE FROM Documents;

-- ── Declarations (all types + history) ───────────────────────────────
IF OBJECT_ID(N'dbo.DeclarationHistories', N'U') IS NOT NULL DELETE FROM DeclarationHistories;
IF OBJECT_ID(N'dbo.DeclarationChecklistItems', N'U') IS NOT NULL DELETE FROM DeclarationChecklistItems;
IF OBJECT_ID(N'dbo.TVALignesFactures', N'U') IS NOT NULL DELETE FROM TVALignesFactures;
IF OBJECT_ID(N'dbo.DeclarationsTVA', N'U') IS NOT NULL DELETE FROM DeclarationsTVA;
IF OBJECT_ID(N'dbo.DeclarationsIS', N'U') IS NOT NULL DELETE FROM DeclarationsIS;
IF OBJECT_ID(N'dbo.DeclarationsCNSS', N'U') IS NOT NULL DELETE FROM DeclarationsCNSS;
IF OBJECT_ID(N'dbo.Declarations', N'U') IS NOT NULL DELETE FROM Declarations;

-- ── Payroll (client employees) ───────────────────────────────────────
IF OBJECT_ID(N'dbo.BulletinLignes', N'U') IS NOT NULL DELETE FROM BulletinLignes;
IF OBJECT_ID(N'dbo.PaySlips', N'U') IS NOT NULL DELETE FROM PaySlips;
IF OBJECT_ID(N'dbo.CumulsEmployes', N'U') IS NOT NULL DELETE FROM CumulsEmployes;
IF OBJECT_ID(N'dbo.VariablesEmployes', N'U') IS NOT NULL DELETE FROM VariablesEmployes;
IF OBJECT_ID(N'dbo.AffectationsRubriques', N'U') IS NOT NULL DELETE FROM AffectationsRubriques;
IF OBJECT_ID(N'dbo.PayrollPeriods', N'U') IS NOT NULL DELETE FROM PayrollPeriods;
IF OBJECT_ID(N'dbo.EmployeeAbsences', N'U') IS NOT NULL DELETE FROM EmployeeAbsences;
IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL DELETE FROM Employees; -- client payroll employees (not cabinet Users)

-- ── Ledger + exercices ───────────────────────────────────────────────
IF OBJECT_ID(N'dbo.RapprochementLignes', N'U') IS NOT NULL DELETE FROM RapprochementLignes;
IF OBJECT_ID(N'dbo.RapprochementsBancaires', N'U') IS NOT NULL DELETE FROM RapprochementsBancaires;
IF OBJECT_ID(N'dbo.LignesEcritures', N'U') IS NOT NULL DELETE FROM LignesEcritures;
IF OBJECT_ID(N'dbo.EcrituresComptables', N'U') IS NOT NULL DELETE FROM EcrituresComptables;
IF OBJECT_ID(N'dbo.ComptesBancaires', N'U') IS NOT NULL DELETE FROM ComptesBancaires;
IF OBJECT_ID(N'dbo.JournauxComptables', N'U') IS NOT NULL DELETE FROM JournauxComptables;
IF OBJECT_ID(N'dbo.ComptesComptables', N'U') IS NOT NULL DELETE FROM ComptesComptables;
IF OBJECT_ID(N'dbo.PeriodesComptables', N'U') IS NOT NULL DELETE FROM PeriodesComptables;
IF OBJECT_ID(N'dbo.ExercicesComptables', N'U') IS NOT NULL DELETE FROM ExercicesComptables;

-- ── Commercial ───────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.Paiements', N'U') IS NOT NULL DELETE FROM Paiements;
IF OBJECT_ID(N'dbo.DocumentsCommerciauxHistorique', N'U') IS NOT NULL DELETE FROM DocumentsCommerciauxHistorique;
IF OBJECT_ID(N'dbo.LignesDocumentsCommerciaux', N'U') IS NOT NULL DELETE FROM LignesDocumentsCommerciaux;
IF OBJECT_ID(N'dbo.ContratMembres', N'U') IS NOT NULL DELETE FROM ContratMembres;
IF OBJECT_ID(N'dbo.Factures', N'U') IS NOT NULL DELETE FROM Factures;
IF OBJECT_ID(N'dbo.ContratsCommerciaux', N'U') IS NOT NULL DELETE FROM ContratsCommerciaux;
IF OBJECT_ID(N'dbo.BonsCommande', N'U') IS NOT NULL DELETE FROM BonsCommande;
IF OBJECT_ID(N'dbo.Devis', N'U') IS NOT NULL DELETE FROM Devis;

-- ── Remove Client portal users + detach Users.ClientId before clients ─
IF EXISTS (SELECT 1 FROM #ClientUsers)
BEGIN
    DELETE FROM AspNetUserTokens WHERE UserId IN (SELECT UserId FROM #ClientUsers);
    DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT UserId FROM #ClientUsers);
    DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT UserId FROM #ClientUsers);
    DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT UserId FROM #ClientUsers);
    DELETE FROM Users WHERE Id IN (SELECT UserId FROM #ClientUsers);
END

-- Any remaining user linked to a client (safety) — unlink, keep cabinet account
IF COL_LENGTH('Users', 'ClientId') IS NOT NULL
    UPDATE Users SET ClientId = NULL WHERE ClientId IS NOT NULL;

-- Client-owned WF copies + archived "(ancien)" cabinet templates
IF OBJECT_ID(N'dbo.WorkflowTemplates', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.WorkflowStepTemplates', N'U') IS NOT NULL
    BEGIN
        DELETE FROM WorkflowStepTemplates
        WHERE WorkflowTemplateId IN (
            SELECT Id FROM WorkflowTemplates
            WHERE ClientId IS NOT NULL
               OR Name LIKE N'%(ancien)%'
               OR IsDeleted = 1
               OR IsActive = 0
        );
    END

    DELETE FROM WorkflowTemplates
    WHERE ClientId IS NOT NULL
       OR Name LIKE N'%(ancien)%'
       OR IsDeleted = 1
       OR IsActive = 0;
END

-- ── Services + clients ───────────────────────────────────────────────
IF OBJECT_ID(N'dbo.ServiceContrats', N'U') IS NOT NULL DELETE FROM ServiceContrats;
IF OBJECT_ID(N'dbo.Clients', N'U') IS NOT NULL DELETE FROM Clients;

-- ── Optional: clear audit / hangfire job noise ───────────────────────
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NOT NULL DELETE FROM AuditLogs;
IF OBJECT_ID(N'dbo.JobParameter', N'U') IS NOT NULL DELETE FROM JobParameter;
IF OBJECT_ID(N'dbo.JobQueue', N'U') IS NOT NULL DELETE FROM JobQueue;
IF OBJECT_ID(N'dbo.State', N'U') IS NOT NULL DELETE FROM State;
IF OBJECT_ID(N'dbo.Job', N'U') IS NOT NULL DELETE FROM Job;

COMMIT;

PRINT 'CLEAN SLATE OK.';
PRINT 'Kept: Cabinets, cabinet Users + roles, Permissions, ReferenceData, Departments, active cabinet WF templates.';
PRINT 'Deleted: Clients, Client users, contracts, declarations, exercices, ledger, ALL WF instances, tasks, commercial, archives.';
PRINT 'Next: create a client + contrat + open exercice, then test.';
