using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using ErrorOr;

namespace BankingApp.Api.Tests.Integration;

/// <summary>
///     Integration tests that verify route contracts, middleware auth enforcement,
///     and the public-versus-protected distinction by sending real HTTP requests
///     through the full ASP.NET Core pipeline.
/// </summary>
public class EndpointRoutingTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-test-token";
    private const int TestUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EndpointRoutingTests" /> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public EndpointRoutingTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // By default, configure the substitutes so that a valid token is accepted.
        factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(ValidToken))
            .Returns(TestUserId);

        factory.AuthRepositoryMock
            .Setup(checksSession => checksSession.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Theory]
    [InlineData("POST", "/api/auth/login")]
    [InlineData("POST", "/api/auth/register")]
    [InlineData("POST", "/api/auth/verify-otp")]
    [InlineData("POST", "/api/auth/forgot-password")]
    [InlineData("POST", "/api/auth/reset-password")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("POST", "/api/auth/resend-otp")]
    [InlineData("POST", "/api/auth/oauth-login")]
    [InlineData("POST", "/api/auth/verify-reset-token")]
    public async Task SendAsync_WhenAuthEndpointIsPublicAndTokenIsMissing_ShouldNotReturnUnauthorized(
        string method,
        string path)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { })
        };

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        // The endpoint is reachable (middleware did not reject). We accept any
        // status other than 401, because the empty body may cause a 400 or 500.
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
    [InlineData("GET", "/api/profile/oauth-links")]
    [InlineData("GET", "/api/profile/notifications/preferences")]
    [InlineData("PUT", "/api/profile/notifications/preferences")]
    [InlineData("POST", "/api/profile/verify-password")]
    [InlineData("PUT", "/api/profile/2fa/enable")]
    [InlineData("PUT", "/api/profile/2fa/disable")]
    [InlineData("GET", "/api/profile/sessions")]
    [InlineData("DELETE", "/api/profile/sessions/1")]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsMissing_ShouldReturnUnauthorized(
        string method,
        string path)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), path);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
    [InlineData("GET", "/api/profile/oauth-links")]
    [InlineData("GET", "/api/profile/notifications/preferences")]
    [InlineData("PUT", "/api/profile/notifications/preferences")]
    [InlineData("POST", "/api/profile/verify-password")]
    [InlineData("PUT", "/api/profile/2fa/enable")]
    [InlineData("PUT", "/api/profile/2fa/disable")]
    [InlineData("GET", "/api/profile/sessions")]
    [InlineData("DELETE", "/api/profile/sessions/1")]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsValid_ShouldNotReturnUnauthorized(
        string method,
        string path)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        // Provide minimal JSON body for endpoints that expect one.
        if (method is "POST" or "PUT") request.Content = JsonContent.Create(new { });

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        const string invalidToken = "bad-token";

        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(invalidToken))
            .Returns(Error.Unauthorized("Token.Invalid", "Token is invalid."));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndSessionIsExpired_ShouldReturnUnauthorized()
    {
        // Arrange
        const string orphanToken = "orphan-token";

        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(orphanToken))
            .Returns(TestUserId);

        _factory.AuthRepositoryMock
            .Setup(checksSession => checksSession.IsSessionActive(orphanToken))
            .Returns(Error.NotFound("Session.NotFound", "Session not found."));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", orphanToken);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsync_WhenRouteDoesNotExistAndTokenIsMissing_ShouldReturnUnauthorized()
    {
        // Arrange
        const string nonExistentProtectedRoute = "/api/does-not-exist";

        // Act
        HttpResponseMessage response = await _client.GetAsync(nonExistentProtectedRoute, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenRouteDoesNotExistAndTokenIsValid_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/does-not-exist");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
