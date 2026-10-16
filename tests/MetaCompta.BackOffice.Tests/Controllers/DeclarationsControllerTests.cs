using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Tests.Helpers;
using MetaCompta.Domain.Common;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for DeclarationsController — verifies action results including TVA workflow.
/// </summary>
public class DeclarationsControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IDeclarationFacade> _mockDeclarations;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly Mock<IWorkflowFacade> _mockWorkflow;
    private readonly Mock<IDocumentFacade> _mockDocuments;
    private readonly Mock<IMessageFacade> _mockMessages;
    private readonly DeclarationsController _controller;

    public DeclarationsControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("TYPE_DECLARATION", "STATUT_DECLARATION");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockDeclarations = new Mock<IDeclarationFacade>();
        _mockClients = new Mock<IClientFacade>();
        _mockWorkflow = new Mock<IWorkflowFacade>();
        _mockDocuments = new Mock<IDocumentFacade>();
        _mockMessages = new Mock<IMessageFacade>();
        _controller = new DeclarationsController(
            _mockRefData.Object,
            _mockAccessor.Object,
            _mockDeclarations.Object,
            _mockClients.Object,
            _mockWorkflow.Object,
            _mockDocuments.Object,
            _mockMessages.Object);
        ControllerTestHelper.SetupController(_controller, sharedContext: _mockAccessor.Object.HttpContext);
        _controller.HttpContext.Session.SetString("SelectedClientId", "1");
        _controller.HttpContext.Session.SetString("ActiveServices", "[\"TVA\"]");
        PermissionHelper.SetPermissions(_controller.HttpContext.Session, new[]
        {
            ClaimDefinitions.DeclarationCreate,
            ClaimDefinitions.DeclarationEdit,
            ClaimDefinitions.TvaEdit
        });
    }

    [Fact]
    public async Task Index_ReturnsViewWithDeclarations()
    {
        var page = TestDataFactory.CreateDeclarationPage();
        _mockDeclarations.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(page);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PagedResult<DeclarationDto>>();
    }

    [Fact]
    public async Task Index_SetsFilterViewBags()
    {
        _mockDeclarations.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(TestDataFactory.CreateDeclarationPage());

        await _controller.Index(type: null, statut: "EnAttente", page: 2);

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
        _mockDeclarations.Setup(x => x.CreateAsync(It.IsAny<CreateDeclarationDto>()))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Ok(created, "Déclaration TVA créée."));

        var result = await _controller.CreateTVA();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("EditTVA");
        redirect.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task CreateTVA_Failure_RedirectsToIndexWithError()
    {
        _mockDeclarations.Setup(x => x.CreateAsync(It.IsAny<CreateDeclarationDto>()))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Fail("Période déjà existante."));

        var result = await _controller.CreateTVA();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("TVA");
        _controller.TempData["Error"].Should().Be("Période déjà existante.");
    }

    [Fact]
    public async Task EditTVA_Get_ReturnsViewWithDto()
    {
        var decl = TestDataFactory.CreateDeclaration(1, "TVA");
        _mockDeclarations.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(decl);
        _mockDeclarations.Setup(x => x.GetTVADataAsync(1))
            .ReturnsAsync(new DeclarationTVADto { DeclarationId = 1 });
        _mockClients.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(TestDataFactory.CreateClient(1));
        _mockWorkflow.Setup(x => x.GetInstancesAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(new PagedResult<WorkflowInstanceDto> { Items = new List<WorkflowInstanceDto>() });

        var result = await _controller.EditTVA(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<DeclarationTVADto>().Subject;
        model.DeclarationId.Should().Be(1);
    }
}
