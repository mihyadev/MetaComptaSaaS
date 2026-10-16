using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for CommercialController ContratCreate client picker (sans contrat actif only).
/// </summary>
public class CommercialControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<ICommercialFacade> _mockCommercial;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly Mock<ICabinetFacade> _mockCabinet;
    private readonly Mock<IWorkflowFacade> _mockWorkflow;
    private readonly CommercialController _controller;

    public CommercialControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("TYPE_SERVICE");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockCommercial = new Mock<ICommercialFacade>();
        _mockClients = new Mock<IClientFacade>();
        _mockCabinet = new Mock<ICabinetFacade>();
        _mockWorkflow = new Mock<IWorkflowFacade>();
        _mockCabinet.Setup(x => x.GetEmployeesAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedResult<CabinetEmployeeDto> { Items = new List<CabinetEmployeeDto>() });
        _mockWorkflow.Setup(x => x.GetTemplatesAsync(
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new PagedResult<WorkflowTemplateDto> { Items = new List<WorkflowTemplateDto>() });

        _controller = new CommercialController(
            _mockRefData.Object,
            _mockAccessor.Object,
            _mockCommercial.Object,
            _mockClients.Object,
            _mockCabinet.Object,
            _mockWorkflow.Object);
        ControllerTestHelper.SetupController(_controller, sharedContext: _mockAccessor.Object.HttpContext);
    }

    [Fact]
    public async Task ContratCreate_Get_ExposesOnlyClientsSansContratActif()
    {
        _mockClients.Setup(x => x.GetForContratPickerAsync()).ReturnsAsync(new ClientContratPickerResultDto
        {
            SansContratActif =
            {
                new ClientContratPickerItemDto { Id = 10, RaisonSociale = "Libre SARL", HasActiveContract = false }
            },
            AvecContratActif =
            {
                new ClientContratPickerItemDto
                {
                    Id = 20,
                    RaisonSociale = "Sous contrat SA",
                    HasActiveContract = true,
                    ActiveContractReference = "CTR-1",
                    ActiveContractStatut = "Valide"
                }
            }
        });

        var result = await _controller.ContratCreate();

        result.Should().BeOfType<ViewResult>();
        var view = (ViewResult)result;
        var model = view.Model.Should().BeOfType<ContratCommercialCreateDto>().Subject;
        model.FrequenceFacturation.Should().Be("Mensuel");
        model.Devise.Should().Be("MAD");

        var sans = ((IEnumerable<ClientContratPickerItemDto>)_controller.ViewBag.ClientsSansContratActif).ToList();
        var avec = ((IEnumerable<ClientContratPickerItemDto>)_controller.ViewBag.ClientsAvecContratActif).ToList();
        sans.Should().ContainSingle(c => c.Id == 10);
        avec.Should().BeEmpty();
        _mockClients.Verify(x => x.GetForContratPickerAsync(), Times.Once);
        _mockClients.Verify(x => x.GetAllForDropdownAsync(), Times.Never);
    }

    [Fact]
    public async Task ContratCreate_Get_WhenPickerEmpty_SetsEmptySansList()
    {
        _mockClients.Setup(x => x.GetForContratPickerAsync())
            .ReturnsAsync(new ClientContratPickerResultDto());

        await _controller.ContratCreate();

        ((IEnumerable<ClientContratPickerItemDto>)_controller.ViewBag.ClientsSansContratActif).Should().BeEmpty();
        ((IEnumerable<ClientContratPickerItemDto>)_controller.ViewBag.ClientsAvecContratActif).Should().BeEmpty();
    }
}
