using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Base controller providing access to facade services, session helpers,
/// and reference data loading. Authentication is handled by [RequireAuthentication] filter.
/// </summary>
[RequireAuthentication]
public abstract class BaseController : Controller
{
    protected readonly IReferenceDataFacade ReferenceData;
    private readonly IHttpContextAccessor _contextAccessor;

    protected BaseController(IReferenceDataFacade referenceData, IHttpContextAccessor contextAccessor)
    {
        ReferenceData = referenceData;
        _contextAccessor = contextAccessor;
    }

    // ── Auto-refresh ActiveServices + live permissions + contract access on every page load ──
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        var clientIdStr = session.GetString("SelectedClientId");
        if (int.TryParse(clientIdStr, out var cid))
        {
            try
            {
                var clients = context.HttpContext.RequestServices.GetService<IClientFacade>();
                if (clients != null)
                {
                    var access = await clients.GetContractAccessAsync(cid);
                    session.SetString("ActiveServices", System.Text.Json.JsonSerializer.Serialize(access.ActiveServices));
                    session.SetString("HasActiveContract", access.AllowsManage.ToString());
                    session.SetString("ContractAccessMode", access.Mode);
                    session.SetString("ContractAccessReason", access.Reason);
                    session.SetString("ContractAccessReasonLabel", access.ReasonLabel ?? "");
                }
            }
            catch { /* API unavailable — keep stale session values */ }
        }

        // Live permissions: reload from API/DB so removing a claim blocks the next click
        try
        {
            if (!string.IsNullOrEmpty(session.GetString("JwtToken")))
            {
                var cabinet = context.HttpContext.RequestServices.GetService<ICabinetFacade>();
                var profile = cabinet != null ? await cabinet.GetProfileAsync() : null;
                if (profile?.Permissions != null)
                    PermissionHelper.SetPermissions(session, profile.Permissions);
            }
        }
        catch { /* keep previous session permissions if profile call fails */ }

        PopulatePermissionViewBag(session);
        await next();
    }

    /// <summary>Expose Can* flags for Razor so every button can hide without per-controller wiring.</summary>
    protected void PopulatePermissionViewBag(ISession session)
    {
        bool Can(string claim) => PermissionHelper.HasPermission(session, claim);
        bool CanAny(params string[] claims) => PermissionHelper.HasPermission(session, claims);

        ViewBag.CanClientList = Can(ClaimDefinitions.ClientList);
        ViewBag.CanClientCreate = Can(ClaimDefinitions.ClientCreate);
        ViewBag.CanClientEdit = Can(ClaimDefinitions.ClientEdit);
        ViewBag.CanClientDelete = Can(ClaimDefinitions.ClientDelete);
        ViewBag.CanClientDetail = Can(ClaimDefinitions.ClientDetail);

        ViewBag.CanEmployeeList = CanAny(ClaimDefinitions.EmployeeList, ClaimDefinitions.EmployeeListByClient);
        ViewBag.CanEmployeeCreate = Can(ClaimDefinitions.EmployeeCreate);
        ViewBag.CanEmployeeEdit = Can(ClaimDefinitions.EmployeeEdit);
        ViewBag.CanEmployeeDelete = Can(ClaimDefinitions.EmployeeDelete);
        ViewBag.CanEmployeeDetail = Can(ClaimDefinitions.EmployeeDetail);

        ViewBag.CanCabinetEmployeeCreate = Can(ClaimDefinitions.CabinetEmployeeCreate);
        ViewBag.CanCabinetEmployeeEdit = Can(ClaimDefinitions.CabinetEmployeeEdit);
        ViewBag.CanCabinetEmployeeDetail = Can(ClaimDefinitions.CabinetEmployeeDetail);
        ViewBag.CanCabinetEdit = Can(ClaimDefinitions.CabinetEdit);
        ViewBag.CanCabinetRoleManage = Can(ClaimDefinitions.CabinetRoleManage);
        ViewBag.CanCabinetAuditView = Can(ClaimDefinitions.CabinetAuditView);

        ViewBag.CanCommercialDetail = Can(ClaimDefinitions.CommercialDetail);
        ViewBag.CanCommercialCreate = Can(ClaimDefinitions.CommercialCreate);
        ViewBag.CanCommercialEdit = Can(ClaimDefinitions.CommercialEdit);
        ViewBag.CanCommercialDelete = Can(ClaimDefinitions.CommercialDelete);
        ViewBag.CanCommercialValidate = Can(ClaimDefinitions.CommercialValidate);
        ViewBag.CanCommercialConvert = Can(ClaimDefinitions.CommercialConvert);
        ViewBag.CanCommercialSign = Can(ClaimDefinitions.CommercialSign);

        ViewBag.CanFactureCreate = Can(ClaimDefinitions.FactureCreate);
        ViewBag.CanFactureDetail = Can(ClaimDefinitions.FactureDetail);
        ViewBag.CanFactureSend = Can(ClaimDefinitions.FactureSend);
        ViewBag.CanFactureCancel = Can(ClaimDefinitions.FactureCancel);
        ViewBag.CanFactureManagePayments = Can(ClaimDefinitions.FactureManagePayments);

        ViewBag.CanPayrollDetail = Can(ClaimDefinitions.PayrollDetail);
        ViewBag.CanPayrollCreate = Can(ClaimDefinitions.PayrollCreate);
        ViewBag.CanPayrollEdit = Can(ClaimDefinitions.PayrollEdit);
        ViewBag.CanPayrollValidate = Can(ClaimDefinitions.PayrollValidate);

        ViewBag.CanComptabiliteDetail = Can(ClaimDefinitions.ComptabiliteDetail);
        ViewBag.CanComptabiliteCreate = Can(ClaimDefinitions.ComptabiliteCreate);
        ViewBag.CanComptabiliteEdit = Can(ClaimDefinitions.ComptabiliteEdit);

        ViewBag.CanJuridiqueDetail = Can(ClaimDefinitions.JuridiqueDetail);
        ViewBag.CanJuridiqueCreate = Can(ClaimDefinitions.JuridiqueCreate);
        ViewBag.CanJuridiqueEdit = Can(ClaimDefinitions.JuridiqueEdit);

        ViewBag.CanTaskDetail = Can(ClaimDefinitions.TaskDetail);
        ViewBag.CanTaskCreate = Can(ClaimDefinitions.TaskCreate);
        ViewBag.CanTaskEdit = Can(ClaimDefinitions.TaskEdit);
        ViewBag.CanTaskAssign = Can(ClaimDefinitions.TaskAssign);

        ViewBag.CanDocumentDetail = Can(ClaimDefinitions.DocumentDetail);
        ViewBag.CanDocumentUpload = Can(ClaimDefinitions.DocumentUpload);
        ViewBag.CanDocumentShare = Can(ClaimDefinitions.DocumentShare);
        ViewBag.CanDocumentDelete = Can(ClaimDefinitions.DocumentDelete);

        ViewBag.CanWorkflowView = Can(ClaimDefinitions.WorkflowView);
        ViewBag.CanWorkflowCreate = Can(ClaimDefinitions.WorkflowCreate);
        ViewBag.CanWorkflowEdit = Can(ClaimDefinitions.WorkflowEdit);
        ViewBag.CanWorkflowDelete = Can(ClaimDefinitions.WorkflowDelete);
        ViewBag.CanWorkflowConfigure = Can(ClaimDefinitions.WorkflowConfigure);
        ViewBag.CanWorkflowAdvance = Can(ClaimDefinitions.WorkflowAdvance);
        ViewBag.CanWorkflowValidate = Can(ClaimDefinitions.WorkflowValidate);
        ViewBag.CanWorkflowCancel = Can(ClaimDefinitions.WorkflowCancel);
        ViewBag.CanWorkflowReassign = Can(ClaimDefinitions.WorkflowReassign);
        ViewBag.CanWorkflowMonitor = Can(ClaimDefinitions.WorkflowMonitor);

        ViewBag.CanDepartmentView = Can(ClaimDefinitions.DepartmentView);
        ViewBag.CanDepartmentCreate = Can(ClaimDefinitions.DepartmentCreate);
        ViewBag.CanDepartmentEdit = Can(ClaimDefinitions.DepartmentEdit);
        ViewBag.CanDepartmentDelete = Can(ClaimDefinitions.DepartmentDelete);
        ViewBag.CanDepartmentManageMembers = Can(ClaimDefinitions.DepartmentManageMembers);

        ViewBag.CanReportView = Can(ClaimDefinitions.ReportView);
        ViewBag.CanReportExport = Can(ClaimDefinitions.ReportExport);
        ViewBag.CanSystemAdmin = Can(ClaimDefinitions.SystemAdmin);
        ViewBag.CanUserManage = Can(ClaimDefinitions.UserManage);

        ViewBag.CanDeclarationView = Can(ClaimDefinitions.DeclarationView);
        ViewBag.CanDeclarationCreatePerm = Can(ClaimDefinitions.DeclarationCreate);
        ViewBag.CanDeclarationEditPerm = Can(ClaimDefinitions.DeclarationEdit);
        ViewBag.CanDeclarationValidatePerm = Can(ClaimDefinitions.DeclarationValidate);
        ViewBag.CanDeclarationSubmitPerm = Can(ClaimDefinitions.DeclarationSubmit);
        ViewBag.CanDeclarationDeletePerm = Can(ClaimDefinitions.DeclarationDelete);

        // Aliases used by Cabinet/Index
        ViewBag.CanCreateEmployee = ViewBag.CanCabinetEmployeeCreate;
        ViewBag.CanEditEmployee = ViewBag.CanCabinetEmployeeEdit;
    }

    // ── Session helpers ─────────────────────────────────────────
    protected string? Token => _contextAccessor.HttpContext?.Session.GetString("JwtToken");
    protected string? UserId => _contextAccessor.HttpContext?.Session.GetString("UserId");
    protected string? UserName => _contextAccessor.HttpContext?.Session.GetString("UserName");
    protected string? UserRole => _contextAccessor.HttpContext?.Session.GetString("UserRole");
    protected string? CabinetName => _contextAccessor.HttpContext?.Session.GetString("CabinetName");
    protected int? CabinetId => int.TryParse(_contextAccessor.HttpContext?.Session.GetString("CabinetId"), out var id) ? id : null;
    protected bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    // ── Global Client Context (selected in topbar) ──────────────
    protected int? SelectedClientId => int.TryParse(_contextAccessor.HttpContext?.Session.GetString("SelectedClientId"), out var cid) ? cid : null;
    protected string? SelectedClientName => _contextAccessor.HttpContext?.Session.GetString("SelectedClientName");

    // ── Reference data helpers ──────────────────────────────────

    protected async Task LoadReferenceDataToViewBagAsync(params string[] categories)
    {
        try
        {
            var data = await ReferenceData.GetMultipleAsync(categories);
            foreach (var kv in data)
            {
                ViewData[$"Ref_{kv.Key}"] = kv.Value;
            }
        }
        catch (Exception)
        {
            // Reference data unavailable – views will use fallback values
        }
    }

    protected void SetApiError<T>(ApiResponse<T>? result, string? fallbackMessage = null)
    {
        var message = result?.Message ?? fallbackMessage ?? "Une erreur inattendue s'est produite.";
        TempData["Error"] = message;

        if (result?.Errors?.Count > 0)
        {
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        }
    }

    protected void SetApiResult<T>(ApiResponse<T>? result, string? successFallback = null, string? errorFallback = null)
    {
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? successFallback;
        }
        else
        {
            SetApiError(result, errorFallback);
        }
    }

    protected void PopulateContractAccessViewBag()
    {
        var session = HttpContext.Session;
        var allowsManage = bool.TryParse(session.GetString("HasActiveContract"), out var hac) && hac;
        var mode = session.GetString("ContractAccessMode")
            ?? (allowsManage ? ContractAccessMode.Manage : ContractAccessMode.Consult);
        var reasonLabel = session.GetString("ContractAccessReasonLabel")
            ?? (allowsManage
                ? "Contrat validé — mode gestion."
                : "Mode consultation uniquement.");

        ViewBag.HasActiveContract = allowsManage;
        ViewBag.ContractAccessMode = mode;
        ViewBag.ContractAccessReason = session.GetString("ContractAccessReason") ?? "";
        ViewBag.ContractAccessReasonLabel = reasonLabel;
        ViewBag.IsConsultMode = !allowsManage;

        var canCreate = allowsManage && PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Create);
        var canEdit = allowsManage && PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Edit);
        var canValidate = allowsManage && PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Validate);
        var canSubmit = allowsManage && PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Submit);
        var canDelete = allowsManage && (
            PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Delete)
            || PermissionHelper.HasPermission(session, PermissionHelper.Declaration.Edit));

        ViewBag.CanCreateDeclaration = canCreate;
        ViewBag.CanEditDeclaration = canEdit;
        ViewBag.CanValidateDeclaration = canValidate;
        ViewBag.CanSubmitDeclaration = canSubmit;
        ViewBag.CanDeleteDeclaration = canDelete;
    }

    protected bool RequiresManageAccess(string? redirectAction = "Index")
    {
        var allowsManage = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        if (allowsManage) return true;
        TempData["Error"] = HttpContext.Session.GetString("ContractAccessReasonLabel")
            ?? "Un contrat validé est requis pour cette action.";
        return false;
    }

    protected bool RequiresPermission(string permission, string? errorMessage = null)
    {
        if (PermissionHelper.HasPermission(HttpContext.Session, permission))
            return true;
        TempData["Error"] = errorMessage ?? "Vous n'avez pas la permission d'effectuer cette action.";
        return false;
    }

    protected bool RequiresAnyPermission(params string[] permissions)
    {
        if (PermissionHelper.HasPermission(HttpContext.Session, permissions))
            return true;
        TempData["Error"] = "Vous n'avez pas la permission d'effectuer cette action.";
        return false;
    }
}
