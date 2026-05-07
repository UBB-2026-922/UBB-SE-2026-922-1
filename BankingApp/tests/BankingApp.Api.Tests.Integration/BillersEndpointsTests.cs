namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure;
using Application.DTOs.Billers;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

public class BillersEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public BillersEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.BillerServiceMock.Invocations.Clear();

        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(authRepository => authRepository.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetBillers_WhenNoFilters_ShouldReturnOkWithAllBillers()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers");

        var billers = new List<BillerDto>
        {
            new BillerDto
            {
                Id = 1,
                Name = "Water Company",
                Category = "Utilities",
                LogoUrl = "https://example.com/water.png",
                IsActive = true,
            },
            new BillerDto
            {
                Id = 2,
                Name = "Electric Corp",
                Category = "Utilities",
                LogoUrl = "https://example.com/electric.png",
                IsActive = true,
            },
        };

        _factory.BillerServiceMock
            .Setup(service => service.GetBillerDirectory())
            .Returns(billers);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BillerDto>? result = await response.Content.ReadFromJsonAsync<List<BillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Water Company");
        result[1].Name.Should().Be("Electric Corp");
    }

    [Fact]
    public async Task GetBillers_WhenSearchTermProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers?search=Water");

        var filteredBillers = new List<BillerDto>
        {
            new BillerDto
            {
                Id = 1,
                Name = "Water Company",
                Category = "Utilities",
                IsActive = true,
            },
        };

        _factory.BillerServiceMock
            .Setup(service => service.SearchBillers("Water", null))
            .Returns(filteredBillers);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BillerDto>? result = await response.Content.ReadFromJsonAsync<List<BillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("Water Company");
        _factory.BillerServiceMock.Verify(service => service.SearchBillers("Water", null), Times.Once);
    }

    [Fact]
    public async Task GetBillers_WhenCategoryFilterProvided_ShouldReturnCategoryFilteredResults()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers?category=Utilities");

        var categoryBillers = new List<BillerDto>
        {
            new BillerDto
            {
                Id = 1,
                Name = "Water Company",
                Category = "Utilities",
                IsActive = true,
            },
            new BillerDto
            {
                Id = 2,
                Name = "Electric Corp",
                Category = "Utilities",
                IsActive = true,
            },
        };

        _factory.BillerServiceMock
            .Setup(service => service.SearchBillers(string.Empty, "Utilities"))
            .Returns(categoryBillers);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BillerDto>? result = await response.Content.ReadFromJsonAsync<List<BillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result!.All(b => b.Category == "Utilities").Should().BeTrue();
    }

    [Fact]
    public async Task GetBillers_WhenSearchAndCategoryProvided_ShouldReturnBothFiltered()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers?search=Water&category=Utilities");

        var filteredBillers = new List<BillerDto>
        {
            new BillerDto
            {
                Id = 1,
                Name = "Water Company",
                Category = "Utilities",
                IsActive = true,
            },
        };

        _factory.BillerServiceMock
            .Setup(service => service.SearchBillers("Water", "Utilities"))
            .Returns(filteredBillers);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BillerDto>? result = await response.Content.ReadFromJsonAsync<List<BillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("Water Company");
        _factory.BillerServiceMock.Verify(service => service.SearchBillers("Water", "Utilities"), Times.Once);
    }

    [Fact]
    public async Task GetBillers_WhenNoBillersFound_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers?search=NonExistent");

        _factory.BillerServiceMock
            .Setup(service => service.SearchBillers("NonExistent", null))
            .Returns(new List<BillerDto>());

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BillerDto>? result = await response.Content.ReadFromJsonAsync<List<BillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSavedBillers_WhenAuthenticated_ShouldReturnOkWithSavedBillers()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var savedBillers = new List<SavedBillerDto>
        {
            new SavedBillerDto
            {
                Id = 1,
                UserId = ValidUserId,
                BillerId = 1,
                BillerName = "Water Company",
                BillerCategory = "Utilities",
                Nickname = "Home Water",
                CreatedAt = DateTime.UtcNow,
            },
            new SavedBillerDto
            {
                Id = 2,
                UserId = ValidUserId,
                BillerId = 2,
                BillerName = "Electric Corp",
                BillerCategory = "Utilities",
                Nickname = "Home Electric",
                CreatedAt = DateTime.UtcNow,
            },
        };

        _factory.BillerServiceMock
            .Setup(service => service.GetSavedBillers(ValidUserId))
            .Returns(savedBillers);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<SavedBillerDto>? result = await response.Content.ReadFromJsonAsync<List<SavedBillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Nickname.Should().Be("Home Water");
    }

    [Fact]
    public async Task GetSavedBillers_WhenUserHasNoSavedBillers_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.BillerServiceMock
            .Setup(service => service.GetSavedBillers(ValidUserId))
            .Returns(new List<SavedBillerDto>());

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<SavedBillerDto>? result = await response.Content.ReadFromJsonAsync<List<SavedBillerDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSavedBillers_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/billers/saved");

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveBiller_WhenDataIsValid_ShouldReturnCreatedWithSavedBiller()
    {
        // Arrange
        var requestData = new SaveBillerRequest
        {
            BillerId = 1,
            Nickname = "Home Water",
            DefaultReference = "ACCOUNT-123",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        var savedBillerDto = new SavedBillerDto
        {
            Id = 1,
            UserId = ValidUserId,
            BillerId = 1,
            BillerName = "Water Company",
            BillerCategory = "Utilities",
            Nickname = "Home Water",
            DefaultReference = "ACCOUNT-123",
            CreatedAt = DateTime.UtcNow,
        };

        _factory.BillerServiceMock
            .Setup(service => service.SaveBiller(ValidUserId, requestData))
            .Returns(savedBillerDto);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        SavedBillerDto? result = await response.Content.ReadFromJsonAsync<SavedBillerDto>(_cancellationToken);
        result.Should().NotBeNull();
        result!.BillerName.Should().Be("Water Company");
        result.Nickname.Should().Be("Home Water");
        result.DefaultReference.Should().Be("ACCOUNT-123");
    }

    [Fact]
    public async Task SaveBiller_WhenBillerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var requestData = new SaveBillerRequest
        {
            BillerId = 999,
            Nickname = "Non-existent",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        _factory.BillerServiceMock
            .Setup(service => service.SaveBiller(ValidUserId, requestData))
            .Returns(Error.NotFound("Biller.NotFound", "Biller not found."));

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SaveBiller_WhenAlreadySaved_ShouldReturnConflict()
    {
        // Arrange
        var requestData = new SaveBillerRequest
        {
            BillerId = 1,
            Nickname = "Home Water",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        _factory.BillerServiceMock
            .Setup(service => service.SaveBiller(ValidUserId, requestData))
            .Returns(Error.Conflict("Biller.AlreadySaved", "Biller already saved."));

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SaveBiller_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var requestData = new SaveBillerRequest
        {
            BillerId = 1,
            Nickname = "Home Water",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billers/saved");
        request.Content = JsonContent.Create(requestData);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveBiller_WhenDataIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new { BillerId = "invalid" };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billers/saved");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(invalidRequest);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveSavedBiller_WhenEntryExists_ShouldReturnNoContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/billers/saved/1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.BillerServiceMock
            .Setup(service => service.RemoveSavedBiller(ValidUserId, 1))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.BillerServiceMock.Verify(service => service.RemoveSavedBiller(ValidUserId, 1), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedBiller_WhenEntryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/billers/saved/999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.BillerServiceMock
            .Setup(service => service.RemoveSavedBiller(ValidUserId, 999))
            .Returns(Error.NotFound("SavedBiller.NotFound", "Saved biller not found."));

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSavedBiller_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/billers/saved/1");

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveSavedBiller_WhenIdIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/billers/saved/invalid");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
