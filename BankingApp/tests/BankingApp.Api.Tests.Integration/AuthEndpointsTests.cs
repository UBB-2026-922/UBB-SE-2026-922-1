// <copyright file="AuthEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure;
using Application.DTOs.Auth;
using Application.Services.Login;
using ErrorOr;
using FluentAssertions;

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

        // Reset mocks before each test to ensure isolated state
        _factory.LoginServiceMock.Invocations.Clear();
        _factory.RegistrationServiceMock.Invocations.Clear();
        _factory.PasswordRecoveryServiceMock.Invocations.Clear();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValidAndNo2Fa_ShouldReturnOkWithToken()
    {
        // Arrange
        var request = new { Email = "test@example.com", Password = "Password1!" };
        _factory.LoginServiceMock
            .Setup(service => service.Login(It.IsAny<LoginRequest>(), It.IsAny<SessionMetadata>()))
            .Returns(new FullLogin(1, "fake-jwt-token"));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request, _cancellationToken);

        // Assert
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
        // Arrange
        var request = new { Email = "test@example.com", Password = "Password1!" };
        _factory.LoginServiceMock
            .Setup(loginService => loginService.Login(It.IsAny<LoginRequest>(), It.IsAny<SessionMetadata>()))
            .Returns(new RequiresTwoFactor(1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Requires2Fa.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new { Email = "test@example.com", Password = "WrongPassword!" };
        _factory.LoginServiceMock
            .Setup(service => service.Login(It.IsAny<LoginRequest>(), It.IsAny<SessionMetadata>()))
            .Returns(Error.Unauthorized("invalid_credentials", "Invalid credentials."));

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WhenValid_ShouldReturnNoContent()
    {
        // Arrange
        var request = new { Email = "new@example.com", Password = "Password1!", FirstName = "Test", LastName = "User" };
        _factory.RegistrationServiceMock
            .Setup(registrationService => registrationService.Register(It.IsAny<RegisterRequest>()))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var request = new { Email = "existing@example.com", Password = "Password1!", FirstName = "Test", LastName = "User" };
        _factory.RegistrationServiceMock
            .Setup(registrationService => registrationService.Register(It.IsAny<RegisterRequest>()))
            .Returns(Error.Conflict("email_taken", "Email is already registered."));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task VerifyOtp_WhenValid_ShouldReturnOkWithToken()
    {
        // Arrange
        var request = new { UserId = 1, OtpCode = "123456" };
        _factory.LoginServiceMock
            .Setup(service => service.VerifyOtp(It.IsAny<VerifyOtpRequest>(), It.IsAny<SessionMetadata>()))
            .Returns(new FullLogin(1, "fake-jwt-token"));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/verify-otp", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task VerifyOtp_WhenInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new { UserId = 1, OtpCode = "wrong" };
        _factory.LoginServiceMock
            .Setup(loginService => loginService.VerifyOtp(It.IsAny<VerifyOtpRequest>(), It.IsAny<SessionMetadata>()))
            .Returns(Error.Unauthorized("invalid_otp", "Invalid or expired code."));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/verify-otp", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailProvided_ShouldReturnOk()
    {
        // Arrange
        var request = new { Email = "test@example.com" };
        _factory.PasswordRecoveryServiceMock
            .Setup(passwordRecoveryService => passwordRecoveryService.RequestPasswordReset(request.Email))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailMissing_ShouldReturnBadRequest()
    {
        var request = new { Email = string.Empty };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WhenValid_ShouldReturnNoContent()
    {
        // Arrange
        var request = new { Token = "valid-token", NewPassword = "NewStrongPassword1!" };
        _factory.PasswordRecoveryServiceMock
            .Setup(passwordRecoveryService => passwordRecoveryService.ResetPassword(request.Token, request.NewPassword))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/reset-password", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResetPassword_WhenPasswordIsWeak_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new { Token = "valid-token", NewPassword = "weak" };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/reset-password", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WhenTokenIsProvided_ShouldReturnNoContent()
    {
        // Arrange
        _factory.LoginServiceMock
            .Setup(loginService => loginService.Logout("valid-token"))
            .Returns(Result.Success);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "valid-token");

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WhenTokenIsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyResetToken_WhenValid_ShouldReturnNoContent()
    {
        // Arrange
        var request = new { Token = "valid-token" };
        _factory.PasswordRecoveryServiceMock
            .Setup(passwordRecoveryService => passwordRecoveryService.VerifyResetToken(request.Token))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/verify-reset-token", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(_cancellationToken));
    }

    [Fact]
    public async Task VerifyResetToken_WhenInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new { Token = "invalid-token" };
        _factory.PasswordRecoveryServiceMock
            .Setup(passwordRecoveryService => passwordRecoveryService.VerifyResetToken(request.Token))
            .Returns(Error.Validation("invalid_token", "Token is invalid or expired."));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/verify-reset-token", request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
