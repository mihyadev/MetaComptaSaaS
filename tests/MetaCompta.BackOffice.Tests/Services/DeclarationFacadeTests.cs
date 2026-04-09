using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for DeclarationFacade — URL construction and response handling.
/// </summary>
public class DeclarationFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly DeclarationFacade _facade;

    public DeclarationFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new DeclarationFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetAllAsync_DefaultParams_CallsApi()
    {
        var page = TestDataFactory.CreateDeclarationPage();
        _mockApi.Setup(x => x.GetAsync<PagedResult<DeclarationDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<DeclarationDto>>.Ok(page));

        var result = await _facade.GetAllAsync();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_IncludesAllParamsInUrl()
    {
        _mockApi.Setup(x => x.GetAsync<PagedResult<DeclarationDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<DeclarationDto>>.Ok(new PagedResult<DeclarationDto>()));

        await _facade.GetAllAsync(clientId: 3, type: "TVA", statut: "EnAttente", page: 2);

        _mockApi.Verify(x => x.GetAsync<PagedResult<DeclarationDto>>(
            It.Is<string>(u => u.Contains("clientId=3") && u.Contains("type=TVA")
                              && u.Contains("statut=EnAttente") && u.Contains("page=2")),
            default), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDeclaration()
    {
        var decl = TestDataFactory.CreateDeclaration(5, "IS", "Validée");
        _mockApi.Setup(x => x.GetAsync<DeclarationDto>("api/declarations/5", default))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Ok(decl));

        var result = await _facade.GetByIdAsync(5);

        result!.Type.Should().Be("IS");
        result.Statut.Should().Be("Validée");
    }

    [Fact]
    public async Task CreateTVAAsync_PostsToCorrectUrl()
    {
        var decl = TestDataFactory.CreateDeclaration(10, "TVA");
        _mockApi.Setup(x => x.PostAsync<DeclarationDto>("api/declarations/tva/7", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<DeclarationDto>.Ok(decl));

        var result = await _facade.CreateTVAAsync(7);

        result!.Success.Should().BeTrue();
        result.Data!.Type.Should().Be("TVA");
    }

    [Fact]
    public async Task SaveTVADataAsync_PostsDataToCorrectUrl()
    {
        var tvaData = TestDataFactory.CreateTVAData();
        _mockApi.Setup(x => x.PostAsync<DeclarationTVADto>("api/declarations/tva/1/data", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<DeclarationTVADto>.Ok(tvaData));

        var result = await _facade.SaveTVADataAsync(1, tvaData);

        result!.Success.Should().BeTrue();
        result.Data!.CAHT_20.Should().Be(500000m);
    }

    [Fact]
    public async Task ChangeStatusAsync_PutsStatusWithComment()
    {
        _mockApi.Setup(x => x.PutAsync<bool>("api/declarations/3/status", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<bool>.Ok(true));

        var result = await _facade.ChangeStatusAsync(3, "Validée", "Vérifié et conforme");

        result!.Success.Should().BeTrue();
        _mockApi.Verify(x => x.PutAsync<bool>("api/declarations/3/status", It.IsAny<object>(), default), Times.Once);
    }
}
