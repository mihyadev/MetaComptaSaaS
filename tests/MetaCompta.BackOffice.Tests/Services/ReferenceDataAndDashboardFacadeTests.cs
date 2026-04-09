using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for ReferenceDataFacade and DashboardFacade.
/// </summary>
public class ReferenceDataFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly ReferenceDataFacade _facade;

    public ReferenceDataFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new ReferenceDataFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetMultipleAsync_JoinsCategoriesInUrl()
    {
        var data = new Dictionary<string, List<ReferenceDataDto>>
        {
            ["FORME_JURIDIQUE"] = new() { new() { Code = "SARL", Label = "SARL" } },
            ["STATUT_CLIENT"] = new() { new() { Code = "Active", Label = "Active" } }
        };
        _mockApi.Setup(x => x.GetAsync<Dictionary<string, List<ReferenceDataDto>>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<Dictionary<string, List<ReferenceDataDto>>>.Ok(data));

        var result = await _facade.GetMultipleAsync("FORME_JURIDIQUE", "STATUT_CLIENT");

        result.Should().ContainKey("FORME_JURIDIQUE");
        result.Should().ContainKey("STATUT_CLIENT");
        _mockApi.Verify(x => x.GetAsync<Dictionary<string, List<ReferenceDataDto>>>(
            It.Is<string>(u => u.Contains("FORME_JURIDIQUE,STATUT_CLIENT")), default), Times.Once);
    }

    [Fact]
    public async Task GetMultipleAsync_WhenApiReturnsNull_ReturnsEmptyDictionary()
    {
        _mockApi.Setup(x => x.GetAsync<Dictionary<string, List<ReferenceDataDto>>>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<Dictionary<string, List<ReferenceDataDto>>>?)null);

        var result = await _facade.GetMultipleAsync("TEST");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

public class DashboardFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly DashboardFacade _facade;

    public DashboardFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new DashboardFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetManagerDashboardAsync_ReturnsDashboardData()
    {
        var dashboard = TestDataFactory.CreateDashboard();
        _mockApi.Setup(x => x.GetAsync<DashboardDto>("api/dashboard/manager", default))
            .ReturnsAsync(ApiResponse<DashboardDto>.Ok(dashboard));

        var result = await _facade.GetManagerDashboardAsync();

        result.Should().NotBeNull();
        result!.TotalClientsActifs.Should().Be(42);
        result.ProchainsDelais.Should().HaveCount(3);
        result.NotificationsRecentes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetManagerDashboardAsync_WhenApiReturnsNull_ReturnsNull()
    {
        _mockApi.Setup(x => x.GetAsync<DashboardDto>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<DashboardDto>?)null);

        var result = await _facade.GetManagerDashboardAsync();

        result.Should().BeNull();
    }
}
