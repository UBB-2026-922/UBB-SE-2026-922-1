// <copyright file="LoginViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for <see cref="LoginViewModel" />.
/// </summary>
public class LoginViewModelTests
{
    private readonly Mock<IApiClient> _apiClient = new();

    public LoginViewModelTests()
    {
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
        LoginViewModel.CanLogin("test@test.com", "password").Should().BeTrue();
    }

    /// <summary>
    ///     When either field is empty or whitespace, <see cref="LoginViewModel.CanLogin" /> returns false.
    /// </summary>
    [Fact]
    public void CanLogin_WhenInvalid_ReturnsFalse()
    {
        // Arrange
        LoginViewModel.CanLogin(string.Empty, "password").Should().BeFalse();
        LoginViewModel.CanLogin("test@test.com", string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(string.Empty, string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(" ", " ").Should().BeFalse();
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
        var viewModel = new LoginViewModel(_apiClient.Object, NullLogger<LoginViewModel>.Instance);
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
        var viewModel = new LoginViewModel(_apiClient.Object, NullLogger<LoginViewModel>.Instance);
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
        var viewModel = new LoginViewModel(_apiClient.Object, NullLogger<LoginViewModel>.Instance);

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
        var viewModel = new LoginViewModel(_apiClient.Object, NullLogger<LoginViewModel>.Instance);

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
