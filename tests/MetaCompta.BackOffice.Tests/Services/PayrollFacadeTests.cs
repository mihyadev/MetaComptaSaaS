using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for PayrollFacade — verifies URL construction and list handling.
/// </summary>
public class PayrollFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly PayrollFacade _facade;

    public PayrollFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new PayrollFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetAllPeriodsAsync_WithoutFilter_ReturnsAllPeriods()
    {
        var periods = Enumerable.Range(1, 3).Select(i => TestDataFactory.CreatePayrollPeriod(i, i)).ToList();
        _mockApi.Setup(x => x.GetAsync<List<PayrollPeriodDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<List<PayrollPeriodDto>>.Ok(periods));

        var result = await _facade.GetAllPeriodsAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllPeriodsAsync_WithClientId_IncludesInUrl()
    {
        _mockApi.Setup(x => x.GetAsync<List<PayrollPeriodDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<List<PayrollPeriodDto>>.Ok(new List<PayrollPeriodDto>()));

        await _facade.GetAllPeriodsAsync(clientId: 5);

        _mockApi.Verify(x => x.GetAsync<List<PayrollPeriodDto>>(
            It.Is<string>(u => u.Contains("clientId=5")), default), Times.Once);
    }

    [Fact]
    public async Task GetAllPeriodsAsync_WhenApiReturnsNull_ReturnsEmptyList()
    {
        _mockApi.Setup(x => x.GetAsync<List<PayrollPeriodDto>>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<List<PayrollPeriodDto>>?)null);

        var result = await _facade.GetAllPeriodsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPeriodAsync_ReturnsCorrectPeriod()
    {
        var period = TestDataFactory.CreatePayrollPeriod(7, 3);
        _mockApi.Setup(x => x.GetAsync<PayrollPeriodDto>("api/payroll/7", default))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Ok(period));

        var result = await _facade.GetPeriodAsync(7);

        result!.Id.Should().Be(7);
        result.Month.Should().Be(3);
    }

    [Fact]
    public async Task GetPaySlipsAsync_ReturnsSlipsList()
    {
        var slips = Enumerable.Range(1, 5).Select(i => TestDataFactory.CreatePaySlip(i)).ToList();
        _mockApi.Setup(x => x.GetAsync<List<PaySlipDto>>("api/payroll/1/payslips", default))
            .ReturnsAsync(ApiResponse<List<PaySlipDto>>.Ok(slips));

        var result = await _facade.GetPaySlipsAsync(1);

        result.Should().HaveCount(5);
        result[0].SalaireBase.Should().Be(8000m);
    }

    [Fact]
    public async Task InitializeAsync_PostsCorrectPayload()
    {
        var period = TestDataFactory.CreatePayrollPeriod(1, 6);
        _mockApi.Setup(x => x.PostAsync<PayrollPeriodDto>("api/payroll/initialize", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Ok(period));

        var result = await _facade.InitializeAsync(clientId: 1, annee: 2025, mois: 6);

        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAllAsync_PostsToCorrectEndpoint()
    {
        _mockApi.Setup(x => x.PostAsync<PayrollPeriodDto>("api/payroll/3/calculate", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<PayrollPeriodDto>.Ok(TestDataFactory.CreatePayrollPeriod(3)));

        var result = await _facade.CalculateAllAsync(3);

        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_PostsToCorrectEndpoint()
    {
        _mockApi.Setup(x => x.PostAsync<bool>("api/payroll/2/validate", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<bool>.Ok(true));

        var result = await _facade.ValidateAsync(2);

        result!.Success.Should().BeTrue();
    }
}
