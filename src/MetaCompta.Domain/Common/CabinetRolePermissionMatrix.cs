namespace MetaCompta.Domain.Common;

/// <summary>
/// Fiduciary cabinet role → claim matrices (single source for startup seed).
/// </summary>
public static class CabinetRolePermissionMatrix
{
    public static readonly string[] SystemRoles =
    {
        "PlatformAdmin", "SuperAdmin", "Manager", "Assistant", "Client",
        "Comptable", "Fiscaliste", "Socialiste", "Juriste", "Commercial"
    };

    public static readonly string[] BackOfficeRoles =
    {
        "SuperAdmin", "Manager", "Assistant", "Comptable", "Fiscaliste",
        "Socialiste", "Juriste", "Commercial"
    };

    /// <summary>Role name → explicit claim list. SuperAdmin handled as all claims.</summary>
    public static IReadOnlyDictionary<string, string[]> ByRole { get; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Manager"] = ClaimDefinitions.All
            .Where(c => !string.Equals(c, ClaimDefinitions.SystemAdmin, StringComparison.OrdinalIgnoreCase))
            .ToArray(),

        ["Comptable"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail,
            ClaimDefinitions.ComptabiliteCreate, ClaimDefinitions.ComptabiliteEdit, ClaimDefinitions.ComptabiliteDetail, ClaimDefinitions.ComptabiliteCloture,
            ClaimDefinitions.DeclarationView, ClaimDefinitions.DeclarationEdit,
            ClaimDefinitions.TvaDetail, ClaimDefinitions.TvaEdit,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail, ClaimDefinitions.DocumentShare,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.ReportView,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowAdvance, ClaimDefinitions.WorkflowReassign, ClaimDefinitions.WorkflowMonitor,
            ClaimDefinitions.CabinetDetail,
        },

        ["Fiscaliste"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail,
            ClaimDefinitions.DeclarationView, ClaimDefinitions.DeclarationCreate, ClaimDefinitions.DeclarationEdit,
            ClaimDefinitions.DeclarationValidate, ClaimDefinitions.DeclarationSubmit,
            ClaimDefinitions.TvaCreate, ClaimDefinitions.TvaEdit, ClaimDefinitions.TvaDetail, ClaimDefinitions.TvaValidate, ClaimDefinitions.TvaSubmit,
            ClaimDefinitions.IsCreate, ClaimDefinitions.IsEdit, ClaimDefinitions.IsDetail, ClaimDefinitions.IsValidate,
            ClaimDefinitions.IrCreate, ClaimDefinitions.IrEdit, ClaimDefinitions.IrDetail, ClaimDefinitions.IrValidate,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail, ClaimDefinitions.DocumentShare,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.ReportView, ClaimDefinitions.ReportExport,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowAdvance, ClaimDefinitions.WorkflowReassign,
            ClaimDefinitions.CabinetDetail,
        },

        ["Socialiste"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail,
            ClaimDefinitions.CnssCreate, ClaimDefinitions.CnssEdit, ClaimDefinitions.CnssDetail, ClaimDefinitions.CnssValidate,
            ClaimDefinitions.PayrollCreate, ClaimDefinitions.PayrollEdit, ClaimDefinitions.PayrollValidate, ClaimDefinitions.PayrollDetail,
            ClaimDefinitions.EmployeeList, ClaimDefinitions.EmployeeListByClient, ClaimDefinitions.EmployeeDetail,
            ClaimDefinitions.DeclarationView,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowReassign,
            ClaimDefinitions.CabinetDetail,
        },

        ["Juriste"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail,
            ClaimDefinitions.JuridiqueCreate, ClaimDefinitions.JuridiqueEdit, ClaimDefinitions.JuridiqueDetail,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail, ClaimDefinitions.DocumentDelete, ClaimDefinitions.DocumentShare,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.ReportView,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowReassign,
            ClaimDefinitions.CabinetDetail,
        },

        ["Commercial"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail, ClaimDefinitions.ClientCreate, ClaimDefinitions.ClientEdit,
            ClaimDefinitions.CommercialCreate, ClaimDefinitions.CommercialEdit, ClaimDefinitions.CommercialDetail,
            ClaimDefinitions.CommercialDelete, ClaimDefinitions.CommercialValidate, ClaimDefinitions.CommercialConvert, ClaimDefinitions.CommercialSign,
            ClaimDefinitions.FactureCreate, ClaimDefinitions.FactureDetail, ClaimDefinitions.FactureSend,
            ClaimDefinitions.FactureCancel, ClaimDefinitions.FactureManagePayments,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail, ClaimDefinitions.DocumentShare,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowReassign,
            ClaimDefinitions.CabinetDetail,
        },

        ["Assistant"] = new[]
        {
            ClaimDefinitions.ClientList, ClaimDefinitions.ClientDetail, ClaimDefinitions.ClientCreate, ClaimDefinitions.ClientEdit,
            ClaimDefinitions.DeclarationView, ClaimDefinitions.DeclarationCreate, ClaimDefinitions.DeclarationEdit,
            ClaimDefinitions.TvaCreate, ClaimDefinitions.TvaEdit, ClaimDefinitions.TvaDetail,
            ClaimDefinitions.IsCreate, ClaimDefinitions.IsEdit, ClaimDefinitions.IsDetail,
            ClaimDefinitions.IrCreate, ClaimDefinitions.IrEdit, ClaimDefinitions.IrDetail,
            ClaimDefinitions.CnssCreate, ClaimDefinitions.CnssEdit, ClaimDefinitions.CnssDetail,
            ClaimDefinitions.PayrollCreate, ClaimDefinitions.PayrollEdit, ClaimDefinitions.PayrollDetail,
            ClaimDefinitions.ComptabiliteCreate, ClaimDefinitions.ComptabiliteEdit, ClaimDefinitions.ComptabiliteDetail, ClaimDefinitions.ComptabiliteCloture,
            ClaimDefinitions.JuridiqueCreate, ClaimDefinitions.JuridiqueEdit, ClaimDefinitions.JuridiqueDetail,
            ClaimDefinitions.CommercialCreate, ClaimDefinitions.CommercialEdit, ClaimDefinitions.CommercialDetail,
            ClaimDefinitions.FactureCreate, ClaimDefinitions.FactureDetail,
            ClaimDefinitions.DocumentUpload, ClaimDefinitions.DocumentDetail, ClaimDefinitions.DocumentShare,
            ClaimDefinitions.TaskCreate, ClaimDefinitions.TaskEdit, ClaimDefinitions.TaskAssign, ClaimDefinitions.TaskDetail,
            ClaimDefinitions.EmployeeList, ClaimDefinitions.EmployeeListByClient, ClaimDefinitions.EmployeeDetail,
            ClaimDefinitions.ReportView, ClaimDefinitions.ReportExport,
            ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowAdvance, ClaimDefinitions.WorkflowReassign,
            ClaimDefinitions.CabinetDetail, ClaimDefinitions.CabinetEmployeeDetail,
            ClaimDefinitions.DepartmentView,
        },
    };
}
