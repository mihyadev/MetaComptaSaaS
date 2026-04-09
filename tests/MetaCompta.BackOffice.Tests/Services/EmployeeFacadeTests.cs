using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for EmployeeFacade — verifies URL construction and response unwrapping.
/// </summary>
public class EmployeeFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly EmployeeFacade _facade;

    public EmployeeFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new EmployeeFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetAllAsync_DefaultParams_CallsApiCorrectly()
    {
        var page = TestDataFactory.CreateEmployeePage();
        _mockApi.Setup(x => x.GetAsync<PagedResult<EmployeeDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<EmployeeDto>>.Ok(page));

        var result = await _facade.GetAllAsync();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WithClientIdFilter_IncludesInUrl()
    {
        _mockApi.Setup(x => x.GetAsync<PagedResult<EmployeeDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<EmployeeDto>>.Ok(new PagedResult<EmployeeDto>()));

        await _facade.GetAllAsync(clientId: 7, page: 2, search: "alami");

        _mockApi.Verify(x => x.GetAsync<PagedResult<EmployeeDto>>(
            It.Is<string>(u => u.Contains("clientId=7") && u.Contains("page=2") && u.Contains("search=alami")),
            default), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployee()
    {
        var emp = TestDataFactory.CreateEmployee(10);
        _mockApi.Setup(x => x.GetAsync<EmployeeDto>("api/employees/10", default))
            .ReturnsAsync(ApiResponse<EmployeeDto>.Ok(emp));

        var result = await _facade.GetByIdAsync(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.FullName.Should().Contain("Mohamed");
    }

    [Fact]
    public async Task CreateAsync_PostsEmployeeData()
    {
        var dto = TestDataFactory.CreateEmployeeDto();
        var created = TestDataFactory.CreateEmployee(1);
        _mockApi.Setup(x => x.PostAsync<EmployeeDto>("api/employees", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<EmployeeDto>.Ok(created, "Employé créé."));

        var result = await _facade.CreateAsync(dto);

        result!.Success.Should().BeTrue();
        result.Message.Should().Be("Employé créé.");
    }

    [Fact]
    public async Task UpdateAsync_PutsToCorrectUrl()
    {
        var dto = TestDataFactory.CreateEmployeeDto();
        _mockApi.Setup(x => x.PutAsync<EmployeeDto>("api/employees/5", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<EmployeeDto>.Ok(TestDataFactory.CreateEmployee(5)));

        var result = await _facade.UpdateAsync(5, dto);

        result!.Success.Should().BeTrue();
        _mockApi.Verify(x => x.PutAsync<EmployeeDto>("api/employees/5", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteEndpoint()
    {
        _mockApi.Setup(x => x.DeleteAsync<bool>("api/employees/8", default))
            .ReturnsAsync(ApiResponse<bool>.Ok(true));

        var result = await _facade.DeleteAsync(8);

        result!.Success.Should().BeTrue();
    }
}
