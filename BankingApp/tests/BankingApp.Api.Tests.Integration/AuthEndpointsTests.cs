namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Features.Authentication.Commands;
using BankingApp.Application.Features.PasswordReset.Commands;
using BankingApp.Application.Features.PasswordReset.Queries;
using BankingApp.Application.Features.UserRegistration.Commands;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using ErrorOr;

public class AuthEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public AuthEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.SenderMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValidAndNo2Fa_ShouldReturnOkWithToken()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(1, "fake-jwt-token", 42));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.LoginFull,
            new { Email = "test@example.com", Password = "Password1!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Token.Should().Be("fake-jwt-token");
        result.Requires2Fa.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValidAndRequires2Fa_ShouldReturnOkWithRequires2FaFlag()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new RequiresTwoFactor(1));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.LoginFull,
            new { Email = "test@example.com", Password = "Password1!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Requires2Fa.Should().BeTrue();
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized("invalid_credentials", "Invalid credentials."));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.LoginFull,
            new { Email = "test@example.com", Password = "WrongPassword!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WhenValid_ShouldReturnNoContent()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.RegisterFull,
            new { Email = "new@example.com", Password = "Password1!", FullName = "Test User" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict("email_taken", "Email is already registered."));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.RegisterFull,
            new { Email = "existing@example.com", Password = "Password1!", FullName = "Test User" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task VerifyOtp_WhenValid_ShouldReturnOkWithToken()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<VerifyOtpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(1, "fake-jwt-token", 42));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.VerifyOtpFull,
            new { UserId = 1, OtpCode = "123456" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailProvided_ShouldReturnOk()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.ForgotPasswordFull,
            new { Email = "test@example.com" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WhenValid_ShouldReturnNoContent()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.ResetPasswordFull,
            new { Token = "valid-token", NewPassword = "NewStrongPassword1!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WhenTokenIsProvided_ShouldReturnNoContent()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var request = new HttpRequestMessage(HttpMethod.Post, "/" + ApiEndpoints.Auth.LogoutFull);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "valid-token");

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WhenTokenIsMissing_ShouldReturnBadRequest()
    {
        HttpResponseMessage response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/" + ApiEndpoints.Auth.LogoutFull),
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyResetToken_WhenValid_ShouldReturnNoContent()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<VerifyResetTokenQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.VerifyResetTokenFull,
            new { Token = "valid-token" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task VerifyResetToken_WhenInvalid_ShouldReturnBadRequest()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<VerifyResetTokenQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("invalid_token", "Token is invalid or expired."));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.VerifyResetTokenFull,
            new { Token = "invalid-token" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
