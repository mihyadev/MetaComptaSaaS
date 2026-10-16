using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Controllers;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Services;
using MetaCompta.BackOffice.Tests.Helpers;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Unit tests for CabinetController — employee CRUD, roles, cabinet info, profile.
/// </summary>
public class CabinetControllerTests
{
    private readonly Mock<ICabinetFacade> _cabinetMock;
    private readonly CabinetController _controller;

    public CabinetControllerTests()
    {
        _cabinetMock = new Mock<ICabinetFacade>();
        var refData = ControllerTestHelper.CreateMockReferenceData();
        var accessor = ControllerTestHelper.CreateMockContextAccessor();

        _controller = new CabinetController(refData.Object, accessor.Object, _cabinetMock.Object);
        ControllerTestHelper.SetupController(_controller, sharedContext: accessor.Object.HttpContext);
        PermissionHelper.SetPermissions(_controller.HttpContext.Session, new[]
        {
            ClaimDefinitions.CabinetEmployeeCreate,
            ClaimDefinitions.CabinetEmployeeEdit,
            ClaimDefinitions.CabinetEmployeeDetail,
            ClaimDefinitions.CabinetRoleManage,
            ClaimDefinitions.CabinetEdit,
            ClaimDefinitions.CabinetAuditView
        });
    }

    // ═══════════════════════════════════════════════════════════
    // INDEX (Employee list)
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task Index_ReturnsViewWithEmployees()
    {
        var page = TestDataFactory.CreateCabinetEmployeePage();
        _cabinetMock.Setup(x => x.GetEmployeesAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(page);

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(page, view.Model);
    }

    [Fact]
    public async Task Index_WithFilters_PassesFiltersToFacade()
    {
        _cabinetMock.Setup(x => x.GetEmployeesAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ReturnsAsync(TestDataFactory.CreateCabinetEmployeePage());

        var result = await _controller.Index("Fiscal", 2, "test");

        Assert.IsType<ViewResult>(result);
        _cabinetMock.Verify(x => x.GetEmployeesAsync("Fiscal", 2, "test", It.IsAny<int>()), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════
    // CREATE EMPLOYEE
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateEmployee_Get_ReturnsViewWithEmptyDto()
    {
        _cabinetMock.Setup(x => x.GetClaimsByDomainAsync()).ReturnsAsync(new Dictionary<string, List<string>>());
        _cabinetMock.Setup(x => x.GetRolesAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(TestDataFactory.CreateRolePage());

        var result = await _controller.CreateEmployee();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CabinetEmployeeCreateDto>(view.Model);
    }

    [Fact]
    public async Task CreateEmployee_Post_Success_RedirectsToIndex()
    {
        var dto = TestDataFactory.CreateCabinetEmployeeCreateDto();
        var response = ApiResponse<CabinetEmployeeDto>.Ok(TestDataFactory.CreateCabinetEmployee());
        _cabinetMock.Setup(x => x.CreateEmployeeAsync(dto)).ReturnsAsync(response);

        var result = await _controller.CreateEmployee(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task CreateEmployee_Post_Failure_ReturnsView()
    {
        var dto = TestDataFactory.CreateCabinetEmployeeCreateDto();
        var response = ApiResponse<CabinetEmployeeDto>.Fail("Email déjà utilisé");
        _cabinetMock.Setup(x => x.CreateEmployeeAsync(dto)).ReturnsAsync(response);
        _cabinetMock.Setup(x => x.GetClaimsByDomainAsync()).ReturnsAsync(new Dictionary<string, List<string>>());
        _cabinetMock.Setup(x => x.GetRolesAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(TestDataFactory.CreateRolePage());

        var result = await _controller.CreateEmployee(dto);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(dto, view.Model);
    }

    // ═══════════════════════════════════════════════════════════
    // EDIT EMPLOYEE
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task EditEmployee_Get_ReturnsViewWithDto()
    {
        var employee = TestDataFactory.CreateCabinetEmployee("emp-1");
        _cabinetMock.Setup(x => x.GetEmployeeByIdAsync("emp-1")).ReturnsAsync(employee);
        _cabinetMock.Setup(x => x.GetClaimsByDomainAsync()).ReturnsAsync(new Dictionary<string, List<string>>());
        _cabinetMock.Setup(x => x.GetRolesAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(TestDataFactory.CreateRolePage());

        var result = await _controller.EditEmployee("emp-1");

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CabinetEmployeeUpdateDto>(view.Model);
    }

    [Fact]
    public async Task EditEmployee_Get_NotFound_Returns404()
    {
        _cabinetMock.Setup(x => x.GetEmployeeByIdAsync("bad-id")).ReturnsAsync((CabinetEmployeeDto?)null);

        var result = await _controller.EditEmployee("bad-id");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditEmployee_Post_Success_RedirectsToIndex()
    {
        var dto = TestDataFactory.CreateCabinetEmployeeUpdateDto();
        var response = ApiResponse<CabinetEmployeeDto>.Ok(TestDataFactory.CreateCabinetEmployee());
        _cabinetMock.Setup(x => x.UpdateEmployeeAsync("emp-1", dto)).ReturnsAsync(response);

        var result = await _controller.EditEmployee("emp-1", dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    // ═══════════════════════════════════════════════════════════
    // EMPLOYEE DETAILS
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task EmployeeDetails_ReturnsViewWithEmployee()
    {
        var employee = TestDataFactory.CreateCabinetEmployee("emp-1");
        _cabinetMock.Setup(x => x.GetEmployeeByIdAsync("emp-1")).ReturnsAsync(employee);

        var result = await _controller.EmployeeDetails("emp-1");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(employee, view.Model);
    }

    [Fact]
    public async Task EmployeeDetails_NotFound_Returns404()
    {
        _cabinetMock.Setup(x => x.GetEmployeeByIdAsync("bad")).ReturnsAsync((CabinetEmployeeDto?)null);

        var result = await _controller.EmployeeDetails("bad");

        Assert.IsType<NotFoundResult>(result);
    }

    // ═══════════════════════════════════════════════════════════
    // TOGGLE EMPLOYEE
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task ToggleEmployee_RedirectsToIndex()
    {
        var response = ApiResponse<bool>.Ok(true, "Employé désactivé.");
        _cabinetMock.Setup(x => x.ToggleEmployeeActiveAsync("emp-1")).ReturnsAsync(response);

        var result = await _controller.ToggleEmployee("emp-1");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    // ═══════════════════════════════════════════════════════════
    // ROLES
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task Roles_ReturnsViewWithRoles()
    {
        var page = TestDataFactory.CreateRolePage();
        _cabinetMock.Setup(x => x.GetRolesAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(page);
        _cabinetMock.Setup(x => x.GetClaimsByDomainAsync()).ReturnsAsync(new Dictionary<string, List<string>>());

        var result = await _controller.Roles();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(page, view.Model);
    }

    [Fact]
    public async Task CreateRole_Post_RedirectsToRoles()
    {
        var response = ApiResponse<RoleDto>.Ok(TestDataFactory.CreateRole());
        _cabinetMock.Setup(x => x.CreateRoleAsync("Comptable", "Desc", It.IsAny<List<string>>())).ReturnsAsync(response);

        var result = await _controller.CreateRole("Comptable", "Desc", new List<string> { "client.view" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Roles", redirect.ActionName);
    }

    [Fact]
    public async Task UpdateRolePermissions_RedirectsToRoles()
    {
        var response = ApiResponse<RoleDto>.Ok(TestDataFactory.CreateRole());
        _cabinetMock.Setup(x => x.UpdateRolePermissionsAsync("role-1", It.IsAny<List<string>>())).ReturnsAsync(response);

        var result = await _controller.UpdateRolePermissions("role-1", new List<string> { "client.view", "client.create" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Roles", redirect.ActionName);
    }

    // ═══════════════════════════════════════════════════════════
    // CABINET INFO
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task EditCabinet_Get_ReturnsViewWithDto()
    {
        var detail = TestDataFactory.CreateCabinetDetail();
        _cabinetMock.Setup(x => x.GetCabinetAsync()).ReturnsAsync(detail);

        var result = await _controller.EditCabinet();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CabinetEditDto>(view.Model);
        Assert.Equal("Cabinet Demo SARL", model.Name);
    }

    [Fact]
    public async Task EditCabinet_Get_NotFound_Returns404()
    {
        _cabinetMock.Setup(x => x.GetCabinetAsync()).ReturnsAsync((CabinetDetailDto?)null);

        var result = await _controller.EditCabinet();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditCabinet_Post_Success_Redirects()
    {
        var dto = new CabinetEditDto { Id = 1, Name = "Updated", Email = "a@b.ma" };
        var response = ApiResponse<CabinetDetailDto>.Ok(TestDataFactory.CreateCabinetDetail());
        _cabinetMock.Setup(x => x.UpdateCabinetAsync(dto)).ReturnsAsync(response);

        var result = await _controller.EditCabinet(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EditCabinet", redirect.ActionName);
    }

    // ═══════════════════════════════════════════════════════════
    // PROFILE
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task Profile_ReturnsViewWithProfile()
    {
        var profile = TestDataFactory.CreateUserProfile();
        _cabinetMock.Setup(x => x.GetProfileAsync()).ReturnsAsync(profile);

        var result = await _controller.Profile();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(profile, view.Model);
    }

    [Fact]
    public async Task Profile_NotFound_Returns404()
    {
        _cabinetMock.Setup(x => x.GetProfileAsync()).ReturnsAsync((UserProfileDto?)null);

        var result = await _controller.Profile();

        Assert.IsType<NotFoundResult>(result);
    }

    // ═══════════════════════════════════════════════════════════
    // AUDIT LOGS
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task AuditLogs_ReturnsViewWithLogs()
    {
        var page = new PagedResult<AuditLogDto>
        {
            Items = new List<AuditLogDto>
            {
                new() { Id = 1, Action = "Creation", UserName = "Test", CreatedAt = DateTime.Now }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 50
        };
        _cabinetMock.Setup(x => x.GetAuditLogsAsync(null, null, null, null, 1)).ReturnsAsync(page);

        var result = await _controller.AuditLogs();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(page, view.Model);
    }

    // ═══════════════════════════════════════════════════════════
    // EQUIPES
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task Equipes_ReturnsViewWithEmployees()
    {
        var page = TestDataFactory.CreateCabinetEmployeePage();
        _cabinetMock.Setup(x => x.GetEmployeesAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(page);

        var result = await _controller.Equipes();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(page, view.Model);
    }
}
