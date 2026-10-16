using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for ClientsController — validates view results, TempData, and redirects.
/// </summary>
public class ClientsControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly Mock<ICommercialFacade> _mockCommercial;
    private readonly Mock<IWorkflowFacade> _mockWorkflow;
    private readonly Mock<ICabinetFacade> _mockCabinet;
    private readonly Mock<IComptabiliteFacade> _mockCompta;
    private readonly Mock<IDepartmentFacade> _mockDepartments;
    private readonly ClientsController _controller;

    public ClientsControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("FORME_JURIDIQUE", "STATUT_CLIENT", "TYPE_TVA", "VILLE", "SECTEUR_ACTIVITE");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockClients = new Mock<IClientFacade>();
        _mockCommercial = new Mock<ICommercialFacade>();
        _mockWorkflow = new Mock<IWorkflowFacade>();
        _mockCabinet = new Mock<ICabinetFacade>();
        _mockCompta = new Mock<IComptabiliteFacade>();
        _mockDepartments = new Mock<IDepartmentFacade>();
        _controller = new ClientsController(
            _mockRefData.Object,
            _mockAccessor.Object,
            _mockClients.Object,
            _mockCommercial.Object,
            _mockWorkflow.Object,
            _mockCabinet.Object,
            _mockCompta.Object,
            _mockDepartments.Object);
        ControllerTestHelper.SetupController(_controller, sharedContext: _mockAccessor.Object.HttpContext);
    }

    // ── Index ────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewWithPagedResult()
    {
        var page = TestDataFactory.CreateClientPage();
        _mockClients.Setup(x => x.GetAllAsync(1, null, null)).ReturnsAsync(page);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PagedResult<ClientDto>>();
        var model = (PagedResult<ClientDto>)viewResult.Model!;
        model.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Index_PassesSearchAndFilterToFacade()
    {
        _mockClients.Setup(x => x.GetAllAsync(2, "test", "Active"))
            .ReturnsAsync(TestDataFactory.CreateClientPage(2, 2));

        var result = await _controller.Index(page: 2, search: "test", statut: "Active");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        Assert.Equal("test", (string)_controller.ViewBag.Search);
        Assert.Equal("Active", (string)_controller.ViewBag.Statut);
    }

    [Fact]
    public async Task Index_LoadsReferenceData()
    {
        _mockClients.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(TestDataFactory.CreateClientPage());

        await _controller.Index();

        _mockRefData.Verify(x => x.GetMultipleAsync(It.IsAny<string[]>()), Times.Once);
    }

    // ── Details ──────────────────────────────────────────────────

    [Fact]
    public async Task Details_ReturnsViewWithClient()
    {
        var client = TestDataFactory.CreateClient(5);
        _mockClients.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(client);

        var result = await _controller.Details(5);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ClientDto>().Subject;
        model.Id.Should().Be(5);
    }

    // ── Create GET ───────────────────────────────────────────────

    [Fact]
    public async Task Create_Get_ReturnsViewWithEmptyDto()
    {
        var result = await _controller.Create();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ClientCreateDto>();
    }

    // ── Create POST ──────────────────────────────────────────────

    [Fact]
    public async Task Create_Post_ValidDto_RedirectsToIndex()
    {
        var dto = TestDataFactory.CreateClientDto();
        _mockClients.Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(ApiResponse<ClientDto>.Ok(TestDataFactory.CreateClient(), "Client créé avec succès."));

        var result = await _controller.Create(dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Success"].Should().Be("Client créé avec succès.");
    }

    [Fact]
    public async Task Create_Post_ApiFails_ReturnsViewWithError()
    {
        var dto = TestDataFactory.CreateClientDto();
        _mockClients.Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(ApiResponse<ClientDto>.Fail("ICE déjà existant."));

        var result = await _controller.Create(dto);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(dto);
        _controller.TempData["Error"].Should().Be("ICE déjà existant.");
    }

    [Fact]
    public async Task Create_Post_InvalidModelState_ReturnsView()
    {
        _controller.ModelState.AddModelError("RaisonSociale", "Required");
        var dto = new ClientCreateDto();

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(dto);
    }

    // ── Edit GET ─────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_ReturnsViewWithMappedDto()
    {
        var client = TestDataFactory.CreateClient(3);
        _mockClients.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(client);

        var result = await _controller.Edit(3);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ClientCreateDto>().Subject;
        model.RaisonSociale.Should().Be(client.RaisonSociale);
        model.ICE.Should().Be(client.ICE);
    }

    [Fact]
    public async Task Edit_Get_NotFound_Returns404()
    {
        _mockClients.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((ClientDto?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Edit POST ────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Post_ValidDto_RedirectsToDetails()
    {
        var dto = TestDataFactory.CreateClientDto();
        _mockClients.Setup(x => x.UpdateAsync(5, dto))
            .ReturnsAsync(ApiResponse<ClientDto>.Ok(TestDataFactory.CreateClient(5), "Client mis à jour."));

        var result = await _controller.Edit(5, dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(5);
    }

    [Fact]
    public async Task Edit_Post_ApiFails_ReturnsViewWithError()
    {
        var dto = TestDataFactory.CreateClientDto();
        _mockClients.Setup(x => x.UpdateAsync(5, dto))
            .ReturnsAsync(ApiResponse<ClientDto>.Fail("Erreur de mise à jour."));

        var result = await _controller.Edit(5, dto);

        result.Should().BeOfType<ViewResult>();
        _controller.TempData["Error"].Should().Be("Erreur de mise à jour.");
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Success_RedirectsToIndexWithMessage()
    {
        _mockClients.Setup(x => x.DeleteAsync(3))
            .ReturnsAsync(ApiResponse<bool>.Ok(true, "Client supprimé."));

        var result = await _controller.Delete(3);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Success"].Should().Be("Client supprimé.");
    }

    [Fact]
    public async Task Delete_Failure_RedirectsToIndexWithError()
    {
        _mockClients.Setup(x => x.DeleteAsync(3))
            .ReturnsAsync(ApiResponse<bool>.Fail("Impossible de supprimer."));

        var result = await _controller.Delete(3);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _controller.TempData["Error"].Should().Be("Impossible de supprimer.");
    }
}
