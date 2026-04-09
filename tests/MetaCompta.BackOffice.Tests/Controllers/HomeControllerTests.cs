using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Controllers;

/// <summary>
/// Tests for HomeController — dashboard loading and error page.
/// </summary>
public class HomeControllerTests
{
    private readonly Mock<IReferenceDataFacade> _mockRefData;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IDashboardFacade> _mockDashboard;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockRefData = ControllerTestHelper.CreateMockReferenceData();
        _mockAccessor = ControllerTestHelper.CreateMockContextAccessor();
        _mockDashboard = new Mock<IDashboardFacade>();
        _controller = new HomeController(_mockRefData.Object, _mockAccessor.Object, _mockDashboard.Object);
        ControllerTestHelper.SetupController(_controller);
    }

    [Fact]
    public async Task Index_ReturnsViewWithDashboardData()
    {
        var dashboard = TestDataFactory.CreateDashboard();
        _mockDashboard.Setup(x => x.GetManagerDashboardAsync()).ReturnsAsync(dashboard);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<DashboardDto>().Subject;
        model.TotalClientsActifs.Should().Be(42);
    }

    [Fact]
    public async Task Index_SetsUserInfoInViewBag()
    {
        _mockDashboard.Setup(x => x.GetManagerDashboardAsync()).ReturnsAsync(TestDataFactory.CreateDashboard());

        await _controller.Index();

        Assert.Equal("Test User", (string?)_controller.ViewBag.UserName);
        Assert.Equal("Manager", (string?)_controller.ViewBag.UserRole);
        Assert.Equal("Test Cabinet", (string?)_controller.ViewBag.CabinetName);
    }

    [Fact]
    public async Task Index_WhenDashboardIsNull_ReturnsViewWithNull()
    {
        _mockDashboard.Setup(x => x.GetManagerDashboardAsync()).ReturnsAsync((DashboardDto?)null);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeNull();
    }

    [Fact]
    public void Error_ReturnsView()
    {
        var result = _controller.Error();

        result.Should().BeOfType<ViewResult>();
    }
}
