namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Domain.Aggregates.IdentityAggregate;
using ErrorOr;

public class EndpointRoutingTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-test-token";
    private const int TestUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public EndpointRoutingTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.SenderMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();

        factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(ValidToken))
            .Returns(TestUserId);

        factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(ValidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActiveIdentity(TestUserId, ValidToken));
    }

    [Theory]
    [InlineData("POST", "/api/auth/login")]
    [InlineData("POST", "/api/auth/register")]
    [InlineData("POST", "/api/auth/verify-otp")]
    [InlineData("POST", "/api/auth/forgot-password")]
    [InlineData("POST", "/api/auth/reset-password")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("POST", "/api/auth/resend-otp")]
    [InlineData("POST", "/api/auth/verify-reset-token")]
    public async Task SendAsync_WhenAuthEndpointIsPublicAndTokenIsMissing_ShouldNotReturnUnauthorized(
        string method,
        string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { })
        };

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
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
        HttpResponseMessage response = await _client.SendAsync(
            new HttpRequestMessage(new HttpMethod(method), path),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
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
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(new { });
        }

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsInvalid_ShouldReturnUnauthorized()
    {
        const string invalidToken = "bad-token";

        _factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(invalidToken))
            .Returns(Error.Unauthorized("Token.Invalid", "Token is invalid."));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndSessionIsExpired_ShouldReturnUnauthorized()
    {
        const string orphanToken = "orphan-token";

        _factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(orphanToken))
            .Returns(TestUserId);

        _factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(orphanToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", orphanToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsync_WhenRouteDoesNotExistAndTokenIsMissing_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync(
            "/api/does-not-exist",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenRouteDoesNotExistAndTokenIsValid_ShouldReturnNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/does-not-exist");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        IdentityAccount identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
