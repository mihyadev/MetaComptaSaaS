using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for DeclarationsController — verifies all action results including TVA workflow.
/// </summary>
public class DeclarationsControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IDeclarationFacade> _mockDeclarations;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly DeclarationsController _controller;

    public DeclarationsControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("TYPE_DECLARATION", "STATUT_DECLARATION");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockDeclarations = new Mock<IDeclarationFacade>();
        _mockClients = new Mock<IClientFacade>();
        _controller = new DeclarationsController(_mockRefData.Object, _mockAccessor.Object, _mockDeclarations.Object, _mockClients.Object);
        ControllerTestHelper.SetupController(_controller);
    }

    [Fact]
    public async Task Index_ReturnsViewWithDeclarations()
    {
        var page = TestDataFactory.CreateDeclarationPage();
        _mockDeclarations.Setup(x => x.GetAllAsync(null, null, null, 1)).ReturnsAsync(page);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PagedResult<DeclarationDto>>();
    }

    [Fact]
    public async Task Index_SetsFilterViewBags()
    {
        _mockDeclarations.Setup(x => x.GetAllAsync(3, "TVA", "EnAttente", 2))
            .ReturnsAsync(TestDataFactory.CreateDeclarationPage());

        await _controller.Index(clientId: 3, type: "TVA", statut: "EnAttente", page: 2);

        Assert.Equal(3, (int?)_controller.ViewBag.ClientId);
        Assert.Equal("TVA", (string?)_controller.ViewBag.Type);
        Assert.Equal("EnAttente", (string?)_controller.ViewBag.Statut);
    }

    [Fact]
    public async Task Details_ReturnsViewWithDeclaration()
    {
        var decl = TestDataFactory.CreateDeclaration(5, "IS", "Validée");
        _mockDeclarations.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(decl);

        var result = await _controller.Details(5);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<DeclarationDto>().Subject;
        model.Type.Should().Be("IS");
    }

    [Fact]
    public async Task CreateTVA_Success_RedirectsToEditTVA()
    {
        var created = TestDataFactory.CreateDeclaration(10, "TVA");
        _mockDeclarations.Setup(x => x.CreateTVAAsync(1))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Ok(created, "Déclaration TVA créée."));

        var result = await _controller.CreateTVA(1);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("EditTVA");
        redirect.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task CreateTVA_Failure_RedirectsToIndexWithError()
    {
        _mockDeclarations.Setup(x => x.CreateTVAAsync(1))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Fail("Période déjà existante."));

        var result = await _controller.CreateTVA(1);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Error"].Should().Be("Période déjà existante.");
    }

    [Fact]
    public async Task EditTVA_Get_ReturnsViewWithDto()
    {
        var decl = TestDataFactory.CreateDeclaration(1, "TVA");
        _mockDeclarations.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(decl);

        var result = await _controller.EditTVA(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<DeclarationTVADto>().Subject;
        model.DeclarationId.Should().Be(1);
    }

    [Fact]
    public async Task EditTVA_Post_Success_RedirectsToDetails()
    {
        var tvaData = TestDataFactory.CreateTVAData();
        _mockDeclarations.Setup(x => x.SaveTVADataAsync(1, tvaData))
            .ReturnsAsync(ApiResponse<DeclarationTVADto>.Ok(tvaData));

        var result = await _controller.EditTVA(1, tvaData);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(1);
    }

    [Fact]
    public async Task EditTVA_Post_Failure_ReturnsView()
    {
        var tvaData = TestDataFactory.CreateTVAData();
        _mockDeclarations.Setup(x => x.SaveTVADataAsync(1, tvaData))
            .ReturnsAsync(ApiResponse<DeclarationTVADto>.Fail("Erreur de calcul."));

        var result = await _controller.EditTVA(1, tvaData);

        result.Should().BeOfType<ViewResult>();
        _controller.TempData["Error"].Should().Be("Erreur de calcul.");
    }

    [Fact]
    public async Task ChangeStatus_Success_RedirectsToDetails()
    {
        _mockDeclarations.Setup(x => x.ChangeStatusAsync(5, "Validée", "OK"))
            .ReturnsAsync(ApiResponse<bool>.Ok(true, "Statut mis à jour."));

        var result = await _controller.ChangeStatus(5, "Validée", "OK");

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        _controller.TempData["Success"].Should().Be("Statut mis à jour.");
    }

    [Fact]
    public async Task ChangeStatus_Failure_RedirectsWithError()
    {
        _mockDeclarations.Setup(x => x.ChangeStatusAsync(5, "Validée", null))
            .ReturnsAsync(ApiResponse<bool>.Fail("Transition non autorisée."));

        var result = await _controller.ChangeStatus(5, "Validée");

        _controller.TempData["Error"].Should().Be("Transition non autorisée.");
    }
}
