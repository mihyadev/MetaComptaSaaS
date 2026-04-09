using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for EmployeesController — validates CRUD operations, client loading, and redirects.
/// </summary>
public class EmployeesControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IEmployeeFacade> _mockEmployees;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("TYPE_CONTRAT", "SITUATION_FAMILIALE", "SEXE", "MODE_PAIEMENT", "STATUT_EMPLOYE", "VILLE");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockEmployees = new Mock<IEmployeeFacade>();
        _mockClients = new Mock<IClientFacade>();
        _mockClients.Setup(x => x.GetAllForDropdownAsync()).ReturnsAsync(new List<ClientDto>
        {
            TestDataFactory.CreateClient(1),
            TestDataFactory.CreateClient(2)
        });

        _controller = new EmployeesController(_mockRefData.Object, _mockAccessor.Object, _mockEmployees.Object, _mockClients.Object);
        ControllerTestHelper.SetupController(_controller);
    }

    [Fact]
    public async Task Index_ReturnsViewWithPagedResult()
    {
        var page = TestDataFactory.CreateEmployeePage();
        _mockEmployees.Setup(x => x.GetAllAsync(null, 1, null)).ReturnsAsync(page);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PagedResult<EmployeeDto>>();
    }

    [Fact]
    public async Task Index_LoadsClientsForDropdown()
    {
        _mockEmployees.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(TestDataFactory.CreateEmployeePage());

        await _controller.Index();

        _mockClients.Verify(x => x.GetAllForDropdownAsync(), Times.Once);
    }

    [Fact]
    public async Task Details_ReturnsViewWithEmployee()
    {
        var emp = TestDataFactory.CreateEmployee(7);
        _mockEmployees.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(emp);

        var result = await _controller.Details(7);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<EmployeeDto>().Subject;
        model.Id.Should().Be(7);
    }

    [Fact]
    public async Task Details_NotFound_Returns404()
    {
        _mockEmployees.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EmployeeDto?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Get_ReturnsViewWithEmptyDto()
    {
        var result = await _controller.Create();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<EmployeeCreateDto>();
    }

    [Fact]
    public async Task Create_Get_WithClientId_SetsClientIdOnDto()
    {
        var result = await _controller.Create(clientId: 5);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<EmployeeCreateDto>().Subject;
        model.ClientId.Should().Be(5);
    }

    [Fact]
    public async Task Create_Post_ValidDto_RedirectsToIndex()
    {
        var dto = TestDataFactory.CreateEmployeeDto();
        _mockEmployees.Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(ApiResponse<EmployeeDto>.Ok(TestDataFactory.CreateEmployee(), "Employé ajouté."));

        var result = await _controller.Create(dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Nom", "Required");

        var result = await _controller.Create(new EmployeeCreateDto());

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_MapsEmployeeToCreateDto()
    {
        var emp = TestDataFactory.CreateEmployee(3);
        _mockEmployees.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(emp);

        var result = await _controller.Edit(3);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<EmployeeCreateDto>().Subject;
        model.CIN.Should().Be(emp.CIN);
        model.Nom.Should().Be(emp.Nom);
        model.SalaireBase.Should().Be(emp.SalaireBase);
    }

    [Fact]
    public async Task Edit_Get_NotFound_Returns404()
    {
        _mockEmployees.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EmployeeDto?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_ValidDto_RedirectsToDetails()
    {
        var dto = TestDataFactory.CreateEmployeeDto();
        _mockEmployees.Setup(x => x.UpdateAsync(5, dto))
            .ReturnsAsync(ApiResponse<EmployeeDto>.Ok(TestDataFactory.CreateEmployee(5)));

        var result = await _controller.Edit(5, dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(5);
    }

    [Fact]
    public async Task Delete_Success_RedirectsToIndex()
    {
        _mockEmployees.Setup(x => x.DeleteAsync(3))
            .ReturnsAsync(ApiResponse<bool>.Ok(true, "Employé supprimé."));

        var result = await _controller.Delete(3);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Success"].Should().Be("Employé supprimé.");
    }
}
