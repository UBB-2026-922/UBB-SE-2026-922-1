// <copyright file="LoginViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for <see cref="LoginViewModel" />.
/// </summary>
public class LoginViewModelTests
{
    private readonly Mock<IApiClient> _apiClient = new();
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginViewModelTests" /> class.
    /// </summary>
    public LoginViewModelTests()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                { "ApiBaseUrl", "http://localhost" },
                { "OAuth:Google:Authority", "https://accounts.google.com" },
                { "OAuth:Google:ClientId", "client-id" },
                { "OAuth:Google:ClientSecret", "client-secret" },
                { "OAuth:Google:RedirectUri", "http://localhost:5000/callback" },
            });

        _configuration = configBuilder.Build();
        _apiClient.Setup(ensuresConfigured => ensuresConfigured.EnsureConfigured()).Returns(Result.Success);
        _apiClient.SetupProperty(getsCurrentUserId => getsCurrentUserId.CurrentUserId);
    }

    /// <summary>
    ///     When both fields are non-empty, <see cref="LoginViewModel.CanLogin" /> returns true.
    /// </summary>
    [Fact]
    public void CanLogin_WhenValid_ReturnsTrue()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);

        // Act & Assert
        viewModel.CanLogin("test@test.com", "password").Should().BeTrue();
    }

    /// <summary>
    ///     When either field is empty or whitespace, <see cref="LoginViewModel.CanLogin" /> returns false.
    /// </summary>
    [Fact]
    public void CanLogin_WhenInvalid_ReturnsFalse()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);

        // Act & Assert
        viewModel.CanLogin(string.Empty, "password").Should().BeFalse();
        viewModel.CanLogin("test@test.com", string.Empty).Should().BeFalse();
        viewModel.CanLogin(string.Empty, string.Empty).Should().BeFalse();
        viewModel.CanLogin(" ", " ").Should().BeFalse();
    }

    /// <summary>
    ///     On a successful login response, state transitions to <see cref="LoginState.Success" />
    ///     and <see cref="IApiClient.CurrentUserId" /> is populated.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Login_WhenSuccess_SetsStateToSuccessAndSetsUserId()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { Token = "test-token", UserId = 1, Requires2Fa = false };

        _apiClient
            .Setup(postsAsync =>
                postsAsync.PostAsync<LoginRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<LoginRequest>()))
            .ReturnsAsync(response);
        _apiClient.Setup(setsToken => setsToken.SetToken(It.IsAny<string>()));

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Success);
        _apiClient.Object.CurrentUserId.Should().Be(1);
    }

    /// <summary>
    ///     When the server indicates 2FA is required, state transitions to <see cref="LoginState.Require2Fa" />
    ///     and <see cref="IApiClient.CurrentUserId" /> is set so the 2FA screen knows which user to verify.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Login_WhenRequires2FA_SetsStateToRequire2Fa()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { UserId = 1, Requires2Fa = true };

        _apiClient
            .Setup(postsAsync =>
                postsAsync.PostAsync<LoginRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<LoginRequest>()))
            .ReturnsAsync(response);

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Require2Fa);
        _apiClient.Object.CurrentUserId.Should().Be(1);
    }

    /// <summary>
    ///     When the server rejects the credentials with an Unauthorized error, state transitions to
    ///     <see cref="LoginState.InvalidCredentials" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Login_WhenUnauthorized_SetsStateToInvalidCredentials()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);

        _apiClient
            .Setup(postsAsync =>
                postsAsync.PostAsync<LoginRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<LoginRequest>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.InvalidCredentials);
    }

    /// <summary>
    ///     When the server returns a generic error (not Unauthorized or Forbidden), state transitions to
    ///     <see cref="LoginState.Error" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Login_WhenServerError_SetsErrorState()
    {
        // Arrange
        var viewModel = new LoginViewModel(_apiClient.Object, _configuration, NullLogger<LoginViewModel>.Instance);

        _apiClient
            .Setup(postsAsync =>
                postsAsync.PostAsync<LoginRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<LoginRequest>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Error);
    }
}