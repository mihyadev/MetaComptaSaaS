using MetaCompta.BackOffice.Tests.Helpers;

namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for ClientFacade — verifies correct URL construction and response handling.
/// </summary>
public class ClientFacadeTests
{
    private readonly Mock<IApiClient> _mockApi;
    private readonly ClientFacade _facade;

    public ClientFacadeTests()
    {
        _mockApi = new Mock<IApiClient>();
        _facade = new ClientFacade(_mockApi.Object);
    }

    [Fact]
    public async Task GetAllAsync_DefaultParams_CallsApiWithPage1()
    {
        var page = TestDataFactory.CreateClientPage();
        _mockApi.Setup(x => x.GetAsync<PagedResult<ClientDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<ClientDto>>.Ok(page));

        var result = await _facade.GetAllAsync();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(5);
        _mockApi.Verify(x => x.GetAsync<PagedResult<ClientDto>>(
            It.Is<string>(u => u.Contains("api/clients") && u.Contains("page=1")), default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchAndStatut_PassesParamsInUrl()
    {
        _mockApi.Setup(x => x.GetAsync<PagedResult<ClientDto>>(It.IsAny<string>(), default))
            .ReturnsAsync(ApiResponse<PagedResult<ClientDto>>.Ok(new PagedResult<ClientDto>()));

        await _facade.GetAllAsync(page: 2, search: "test", statut: "Active");

        _mockApi.Verify(x => x.GetAsync<PagedResult<ClientDto>>(
            It.Is<string>(u => u.Contains("page=2") && u.Contains("search=test") && u.Contains("statut=Active")),
            default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenApiReturnsNull_ReturnsNull()
    {
        _mockApi.Setup(x => x.GetAsync<PagedResult<ClientDto>>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<PagedResult<ClientDto>>?)null);

        var result = await _facade.GetAllAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CallsCorrectUrl()
    {
        var client = TestDataFactory.CreateClient(42);
        _mockApi.Setup(x => x.GetAsync<ClientDto>("api/clients/42", default))
            .ReturnsAsync(ApiResponse<ClientDto>.Ok(client));

        var result = await _facade.GetByIdAsync(42);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
    }

    [Fact]
    public async Task CreateAsync_PostsToCorrectEndpoint()
    {
        var dto = TestDataFactory.CreateClientDto();
        var created = TestDataFactory.CreateClient(1);
        _mockApi.Setup(x => x.PostAsync<ClientDto>("api/clients", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<ClientDto>.Ok(created, "Client créé avec succès."));

        var result = await _facade.CreateAsync(dto);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.RaisonSociale.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_PutsToCorrectEndpoint()
    {
        var dto = TestDataFactory.CreateClientDto();
        var updated = TestDataFactory.CreateClient(5);
        _mockApi.Setup(x => x.PutAsync<ClientDto>("api/clients/5", It.IsAny<object>(), default))
            .ReturnsAsync(ApiResponse<ClientDto>.Ok(updated));

        var result = await _facade.UpdateAsync(5, dto);

        result!.Success.Should().BeTrue();
        _mockApi.Verify(x => x.PutAsync<ClientDto>("api/clients/5", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsCorrectEndpoint()
    {
        _mockApi.Setup(x => x.DeleteAsync<bool>("api/clients/3", default))
            .ReturnsAsync(ApiResponse<bool>.Ok(true, "Client supprimé."));

        var result = await _facade.DeleteAsync(3);

        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllForDropdownAsync_ReturnsListFromPagedResult()
    {
        var page = TestDataFactory.CreateClientPage(10);
        _mockApi.Setup(x => x.GetAsync<PagedResult<ClientDto>>(It.Is<string>(u => u.Contains("pageSize=500")), default))
            .ReturnsAsync(ApiResponse<PagedResult<ClientDto>>.Ok(page));

        var result = await _facade.GetAllForDropdownAsync();

        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetAllForDropdownAsync_WhenNull_ReturnsEmptyList()
    {
        _mockApi.Setup(x => x.GetAsync<PagedResult<ClientDto>>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<PagedResult<ClientDto>>?)null);

        var result = await _facade.GetAllForDropdownAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetForContratPickerAsync_CallsCorrectEndpoint_AndReturnsGroups()
    {
        var payload = new ClientContratPickerResultDto
        {
            SansContratActif =
            {
                new ClientContratPickerItemDto { Id = 1, RaisonSociale = "Sans SARL", HasActiveContract = false }
            },
            AvecContratActif =
            {
                new ClientContratPickerItemDto
                {
                    Id = 2,
                    RaisonSociale = "Avec SA",
                    HasActiveContract = true,
                    ActiveContractReference = "CTR-2026-0001",
                    ActiveContractStatut = "Actif"
                }
            }
        };
        _mockApi.Setup(x => x.GetAsync<ClientContratPickerResultDto>("api/clients/for-contrat-picker", default))
            .ReturnsAsync(ApiResponse<ClientContratPickerResultDto>.Ok(payload));

        var result = await _facade.GetForContratPickerAsync();

        result.SansContratActif.Should().HaveCount(1);
        result.AvecContratActif.Should().HaveCount(1);
        result.AvecContratActif[0].ActiveContractReference.Should().Be("CTR-2026-0001");
        _mockApi.Verify(x => x.GetAsync<ClientContratPickerResultDto>("api/clients/for-contrat-picker", default), Times.Once);
    }

    [Fact]
    public async Task GetForContratPickerAsync_WhenNull_ReturnsEmptyGroups()
    {
        _mockApi.Setup(x => x.GetAsync<ClientContratPickerResultDto>(It.IsAny<string>(), default))
            .ReturnsAsync((ApiResponse<ClientContratPickerResultDto>?)null);

        var result = await _facade.GetForContratPickerAsync();

        result.Should().NotBeNull();
        result.SansContratActif.Should().BeEmpty();
        result.AvecContratActif.Should().BeEmpty();
    }
}
