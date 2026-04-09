using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for PayrollController — verifies initialization, calculation, and validation workflows.
/// </summary>
public class PayrollControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IPayrollFacade> _mockPayroll;
    private readonly Mock<IClientFacade> _mockClients;
    private readonly PayrollController _controller;

    public PayrollControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceDataWith("STATUT_PAIE");
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockPayroll = new Mock<IPayrollFacade>();
        _mockClients = new Mock<IClientFacade>();
        _mockClients.Setup(x => x.GetAllForDropdownAsync()).ReturnsAsync(new List<ClientDto>
        {
            TestDataFactory.CreateClient(1),
            TestDataFactory.CreateClient(2)
        });

        _controller = new PayrollController(_mockRefData.Object, _mockAccessor.Object, _mockPayroll.Object, _mockClients.Object);
        ControllerTestHelper.SetupController(_controller);
    }

    [Fact]
    public async Task Index_ReturnsViewWithPeriodsList()
    {
        var periods = new List<PayrollPeriodDto>
        {
            TestDataFactory.CreatePayrollPeriod(1, 1),
            TestDataFactory.CreatePayrollPeriod(2, 2)
        };
        _mockPayroll.Setup(x => x.GetAllPeriodsAsync(null)).ReturnsAsync(periods);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<PayrollPeriodDto>>().Subject;
        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_WithClientFilter_PassesClientIdToFacade()
    {
        _mockPayroll.Setup(x => x.GetAllPeriodsAsync(5))
            .ReturnsAsync(new List<PayrollPeriodDto>());

        await _controller.Index(clientId: 5);

        _mockPayroll.Verify(x => x.GetAllPeriodsAsync(5), Times.Once);
        Assert.Equal(5, (int?)_controller.ViewBag.ClientId);
    }

    [Fact]
    public async Task Initialize_Success_RedirectsToPeriod()
    {
        var period = TestDataFactory.CreatePayrollPeriod(10, 6);
        _mockPayroll.Setup(x => x.InitializeAsync(1, 2025, 6))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Ok(period, "Période initialisée."));

        var result = await _controller.Initialize(1, 2025, 6);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Period");
        redirect.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task Initialize_Failure_RedirectsToIndex()
    {
        _mockPayroll.Setup(x => x.InitializeAsync(1, 2025, 6))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Fail("Période déjà existante."));

        var result = await _controller.Initialize(1, 2025, 6);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Error"].Should().Be("Période déjà existante.");
    }

    [Fact]
    public async Task Period_ReturnsViewWithPeriodAndPaySlips()
    {
        var period = TestDataFactory.CreatePayrollPeriod(1, 1);
        var slips = Enumerable.Range(1, 3).Select(i => TestDataFactory.CreatePaySlip(i)).ToList();

        _mockPayroll.Setup(x => x.GetPeriodAsync(1)).ReturnsAsync(period);
        _mockPayroll.Setup(x => x.GetPaySlipsAsync(1)).ReturnsAsync(slips);

        var result = await _controller.Period(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PayrollPeriodDto>();
    }

    [Fact]
    public async Task Calculate_Success_RedirectsToPeriod()
    {
        _mockPayroll.Setup(x => x.CalculateAllAsync(5))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Ok(TestDataFactory.CreatePayrollPeriod(5), "Calcul effectué."));

        var result = await _controller.Calculate(5);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Period");
        redirect.RouteValues!["id"].Should().Be(5);
    }

    [Fact]
    public async Task Validate_Success_RedirectsToPeriodWithSuccess()
    {
        _mockPayroll.Setup(x => x.ValidateAsync(3))
            .ReturnsAsync(ApiResponse<bool>.Ok(true, "Période validée."));

        var result = await _controller.Validate(3);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Period");
        _controller.TempData["Success"].Should().Be("Période validée.");
    }

    [Fact]
    public async Task Validate_Failure_RedirectsWithError()
    {
        _mockPayroll.Setup(x => x.ValidateAsync(3))
            .ReturnsAsync(ApiResponse<bool>.Fail("Calcul non effectué."));

        var result = await _controller.Validate(3);

        _controller.TempData["Error"].Should().Be("Calcul non effectué.");
    }
}
