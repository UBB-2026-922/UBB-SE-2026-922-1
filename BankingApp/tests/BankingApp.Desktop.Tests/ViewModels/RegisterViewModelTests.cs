// <copyright file="RegisterViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for <see cref="RegisterViewModel" />.
/// </summary>
public class RegisterViewModelTests
{
    private readonly Mock<IApiClient> _apiClient = new();
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterViewModelTests" /> class.
    /// </summary>
    public RegisterViewModelTests()
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
    }

    /// <summary>
    ///     When any required field is empty, state transitions to <see cref="RegisterState.Error" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenEmptyFields_SetsErrorState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _apiClient.Object,
            _configuration,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register(string.Empty, "pass", "pass", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.Error);
    }

    /// <summary>
    ///     When the password and confirmation do not match, state transitions to
    ///     <see cref="RegisterState.PasswordMismatch" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenPasswordMismatch_SetsPasswordMismatchState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _apiClient.Object,
            _configuration,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "Password123!", "Password123", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.PasswordMismatch);
    }

    /// <summary>
    ///     When the password does not meet complexity requirements, state transitions to
    ///     <see cref="RegisterState.WeakPassword" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenWeakPassword_SetsWeakPasswordState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _apiClient.Object,
            _configuration,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "weak", "weak", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.WeakPassword);
    }

    /// <summary>
    ///     When all inputs are valid and the API succeeds, state transitions to
    ///     <see cref="RegisterState.Success" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenValid_SetsSuccessState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _apiClient.Object,
            _configuration,
            NullLogger<RegisterViewModel>.Instance);

        _apiClient
            .Setup(postsAsync => postsAsync.PostAsync(It.IsAny<string>(), It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.Success);
    }

    /// <summary>
    ///     When the API returns a Conflict error, state transitions to
    ///     <see cref="RegisterState.EmailAlreadyExists" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenEmailConflicts_SetsEmailAlreadyExistsState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _apiClient.Object,
            _configuration,
            NullLogger<RegisterViewModel>.Instance);

        _apiClient
            .Setup(postsAsync => postsAsync.PostAsync(It.IsAny<string>(), It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Error.Conflict("Conflict", "Conflict"));

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.EmailAlreadyExists);
    }
}