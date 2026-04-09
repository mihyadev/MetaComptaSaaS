namespace MetaCompta.Domain.Common;

/// <summary>
/// Centralized claim/permission definitions following the domain.action convention.
/// Each claim corresponds to a specific action on a specific domain element.
/// Format: domain.action[.element]
/// Examples: employee.create, tva.edit, cabinet.edit
/// </summary>
public static class ClaimDefinitions
{
    // ── Employee ─────────────────────────────────────────────────
    public const string EmployeeCreate = "employee.create";
    public const string EmployeeEdit = "employee.edit";
    public const string EmployeeDetail = "employee.detail";
    public const string EmployeeDelete = "employee.delete";
    public const string EmployeeList = "employee.list";
    public const string EmployeeListByClient = "employee.list.byclient";

    // ── Client ──────────────────────────────────────────────────
    public const string ClientCreate = "client.create";
    public const string ClientEdit = "client.edit";
    public const string ClientDetail = "client.detail";
    public const string ClientDelete = "client.delete";
    public const string ClientList = "client.list";

    // ── Cabinet ─────────────────────────────────────────────────
    public const string CabinetEdit = "cabinet.edit";
    public const string CabinetDetail = "cabinet.detail";
    public const string CabinetEmployeeCreate = "cabinet.employee.create";
    public const string CabinetEmployeeEdit = "cabinet.employee.edit";
    public const string CabinetEmployeeDetail = "cabinet.employee.detail";
    public const string CabinetEmployeeDelete = "cabinet.employee.delete";
    public const string CabinetRoleManage = "cabinet.role.manage";
    public const string CabinetAuditView = "cabinet.audit.view";

    // ── TVA ──────────────────────────────────────────────────────
    public const string TvaCreate = "tva.create";
    public const string TvaEdit = "tva.edit";
    public const string TvaDetail = "tva.detail";
    public const string TvaValidate = "tva.validate";
    public const string TvaSubmit = "tva.submit";

    // ── Declaration (cross-type) ────────────────────────────────
    public const string DeclarationView = "declaration.view";
    public const string DeclarationCreate = "declaration.create";
    public const string DeclarationEdit = "declaration.edit";
    public const string DeclarationValidate = "declaration.validate";
    public const string DeclarationSubmit = "declaration.submit";
    public const string DeclarationDelete = "declaration.delete";

    // ── IS ───────────────────────────────────────────────────────
    public const string IsCreate = "is.create";
    public const string IsEdit = "is.edit";
    public const string IsDetail = "is.detail";
    public const string IsValidate = "is.validate";

    // ── IR ───────────────────────────────────────────────────────
    public const string IrCreate = "ir.create";
    public const string IrEdit = "ir.edit";
    public const string IrDetail = "ir.detail";
    public const string IrValidate = "ir.validate";

    // ── CNSS ─────────────────────────────────────────────────────
    public const string CnssCreate = "cnss.create";
    public const string CnssEdit = "cnss.edit";
    public const string CnssDetail = "cnss.detail";
    public const string CnssValidate = "cnss.validate";

    // ── Payroll ──────────────────────────────────────────────────
    public const string PayrollCreate = "payroll.create";
    public const string PayrollEdit = "payroll.edit";
    public const string PayrollValidate = "payroll.validate";
    public const string PayrollDetail = "payroll.detail";

    // ── Document ─────────────────────────────────────────────────
    public const string DocumentUpload = "document.upload";
    public const string DocumentDetail = "document.detail";
    public const string DocumentDelete = "document.delete";
    public const string DocumentShare = "document.share";

    // ── Task ─────────────────────────────────────────────────────
    public const string TaskCreate = "task.create";
    public const string TaskEdit = "task.edit";
    public const string TaskAssign = "task.assign";
    public const string TaskDetail = "task.detail";

    // ── Report ───────────────────────────────────────────────────
    public const string ReportView = "report.view";
    public const string ReportExport = "report.export";

    // ── Juridique ────────────────────────────────────────────────
    public const string JuridiqueCreate = "juridique.create";
    public const string JuridiqueEdit = "juridique.edit";
    public const string JuridiqueDetail = "juridique.detail";

    // ── Comptabilite ─────────────────────────────────────────────
    public const string ComptabiliteCreate = "comptabilite.create";
    public const string ComptabiliteEdit = "comptabilite.edit";
    public const string ComptabiliteDetail = "comptabilite.detail";

    // ── Workflow ─────────────────────────────────────────────────
    public const string WorkflowView = "workflow.view";
    public const string WorkflowCreate = "workflow.create";
    public const string WorkflowEdit = "workflow.edit";
    public const string WorkflowDelete = "workflow.delete";
    public const string WorkflowConfigure = "workflow.configure";
    public const string WorkflowAdvance = "workflow.advance";
    public const string WorkflowValidate = "workflow.validate";
    public const string WorkflowReassign = "workflow.reassign";
    public const string WorkflowCancel = "workflow.cancel";
    public const string WorkflowMonitor = "workflow.monitor";

    // ── Department ───────────────────────────────────────────────
    public const string DepartmentView = "department.view";
    public const string DepartmentCreate = "department.create";
    public const string DepartmentEdit = "department.edit";
    public const string DepartmentDelete = "department.delete";
    public const string DepartmentManageMembers = "department.manage.members";

    // ── Commercial (Devis, Bons Commande, Contrats) ─────────────
    public const string CommercialCreate = "commercial.create";
    public const string CommercialEdit = "commercial.edit";
    public const string CommercialDetail = "commercial.detail";
    public const string CommercialDelete = "commercial.delete";
    public const string CommercialValidate = "commercial.validate";
    public const string CommercialConvert = "commercial.convert";
    public const string CommercialSign = "commercial.sign";

    // ── Facture ──────────────────────────────────────────────────
    public const string FactureCreate = "facture.create";
    public const string FactureDetail = "facture.detail";
    public const string FactureSend = "facture.send";
    public const string FactureCancel = "facture.cancel";
    public const string FactureManagePayments = "facture.managepayments";

    // ── System ───────────────────────────────────────────────────
    public const string SystemAdmin = "system.admin";
    public const string UserManage = "user.manage";

    /// <summary>
    /// Returns all defined claims as a flat list.
    /// Useful for seeding permissions and building policy definitions.
    /// </summary>
    public static IReadOnlyList<string> All => new[]
    {
        EmployeeCreate, EmployeeEdit, EmployeeDetail, EmployeeDelete, EmployeeList, EmployeeListByClient,
        ClientCreate, ClientEdit, ClientDetail, ClientDelete, ClientList,
        CabinetEdit, CabinetDetail, CabinetEmployeeCreate, CabinetEmployeeEdit, CabinetEmployeeDetail,
        CabinetEmployeeDelete, CabinetRoleManage, CabinetAuditView,
        TvaCreate, TvaEdit, TvaDetail, TvaValidate, TvaSubmit,
        DeclarationView, DeclarationCreate, DeclarationEdit, DeclarationValidate, DeclarationSubmit, DeclarationDelete,
        IsCreate, IsEdit, IsDetail, IsValidate,
        IrCreate, IrEdit, IrDetail, IrValidate,
        CnssCreate, CnssEdit, CnssDetail, CnssValidate,
        PayrollCreate, PayrollEdit, PayrollValidate, PayrollDetail,
        DocumentUpload, DocumentDetail, DocumentDelete, DocumentShare,
        TaskCreate, TaskEdit, TaskAssign, TaskDetail,
        ReportView, ReportExport,
        JuridiqueCreate, JuridiqueEdit, JuridiqueDetail,
        ComptabiliteCreate, ComptabiliteEdit, ComptabiliteDetail,
        WorkflowView, WorkflowCreate, WorkflowEdit, WorkflowDelete, WorkflowConfigure,
        WorkflowAdvance, WorkflowValidate, WorkflowReassign, WorkflowCancel, WorkflowMonitor,
        DepartmentView, DepartmentCreate, DepartmentEdit, DepartmentDelete, DepartmentManageMembers,
        CommercialCreate, CommercialEdit, CommercialDetail, CommercialDelete, CommercialValidate, CommercialConvert, CommercialSign,
        FactureCreate, FactureDetail, FactureSend, FactureCancel, FactureManagePayments,
        SystemAdmin, UserManage
    };

    /// <summary>
    /// Returns claims grouped by domain for display in UI (role management).
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> ByDomain => new Dictionary<string, string[]>
    {
        ["employee"] = new[] { EmployeeCreate, EmployeeEdit, EmployeeDetail, EmployeeDelete, EmployeeList, EmployeeListByClient },
        ["client"] = new[] { ClientCreate, ClientEdit, ClientDetail, ClientDelete, ClientList },
        ["cabinet"] = new[] { CabinetEdit, CabinetDetail, CabinetEmployeeCreate, CabinetEmployeeEdit, CabinetEmployeeDetail, CabinetEmployeeDelete, CabinetRoleManage, CabinetAuditView },
        ["tva"] = new[] { TvaCreate, TvaEdit, TvaDetail, TvaValidate, TvaSubmit },
        ["declaration"] = new[] { DeclarationView, DeclarationCreate, DeclarationEdit, DeclarationValidate, DeclarationSubmit, DeclarationDelete },
        ["is"] = new[] { IsCreate, IsEdit, IsDetail, IsValidate },
        ["ir"] = new[] { IrCreate, IrEdit, IrDetail, IrValidate },
        ["cnss"] = new[] { CnssCreate, CnssEdit, CnssDetail, CnssValidate },
        ["payroll"] = new[] { PayrollCreate, PayrollEdit, PayrollValidate, PayrollDetail },
        ["document"] = new[] { DocumentUpload, DocumentDetail, DocumentDelete, DocumentShare },
        ["task"] = new[] { TaskCreate, TaskEdit, TaskAssign, TaskDetail },
        ["report"] = new[] { ReportView, ReportExport },
        ["juridique"] = new[] { JuridiqueCreate, JuridiqueEdit, JuridiqueDetail },
        ["comptabilite"] = new[] { ComptabiliteCreate, ComptabiliteEdit, ComptabiliteDetail },
        ["workflow"] = new[] { WorkflowView, WorkflowCreate, WorkflowEdit, WorkflowDelete, WorkflowConfigure, WorkflowAdvance, WorkflowValidate, WorkflowReassign, WorkflowCancel, WorkflowMonitor },
        ["department"] = new[] { DepartmentView, DepartmentCreate, DepartmentEdit, DepartmentDelete, DepartmentManageMembers },
        ["commercial"] = new[] { CommercialCreate, CommercialEdit, CommercialDetail, CommercialDelete, CommercialValidate, CommercialConvert, CommercialSign },
        ["facture"] = new[] { FactureCreate, FactureDetail, FactureSend, FactureCancel, FactureManagePayments },
        ["system"] = new[] { SystemAdmin, UserManage }
    };
}
