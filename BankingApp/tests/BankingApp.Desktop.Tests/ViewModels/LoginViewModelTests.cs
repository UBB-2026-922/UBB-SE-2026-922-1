namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Application.Features.Authentication.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class LoginViewModelTests
{
    private readonly Mock<IAuthClientService> _authClientService = new();

    public LoginViewModelTests()
    {
        _authClientService.Setup(authClientService => authClientService.EnsureConfigured()).Returns(Result.Success);
        _authClientService.SetupProperty(authClientService => authClientService.CurrentUserId);
    }

    [Fact]
    public void CanLogin_WhenValid_ReturnsTrue()
    {
        LoginViewModel.CanLogin("test@test.com", "password").Should().BeTrue();
    }

    [Fact]
    public void CanLogin_WhenInvalid_ReturnsFalse()
    {
        LoginViewModel.CanLogin(string.Empty, "password").Should().BeFalse();
        LoginViewModel.CanLogin("test@test.com", string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(string.Empty, string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(" ", " ").Should().BeFalse();
    }

    [Fact]
    public async Task Login_WhenSuccess_SetsStateToSuccessAndSetsUserId()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authClientService.Object, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { Token = "test-token", UserId = 1, Requires2Fa = false };

        _authClientService
            .Setup(authClientService => authClientService.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(response);
        _authClientService.Setup(authClientService => authClientService.SetToken(It.IsAny<string>()));

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Success);
        _authClientService.Object.CurrentUserId.Should().Be(1);
    }

    [Fact]
    public async Task Login_WhenRequires2FA_SetsStateToRequire2Fa()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authClientService.Object, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { UserId = 1, Requires2Fa = true };

        _authClientService
            .Setup(authClientService => authClientService.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Require2Fa);
        _authClientService.Object.CurrentUserId.Should().Be(1);
    }

    [Fact]
    public async Task Login_WhenUnauthorized_SetsStateToInvalidCredentials()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authClientService.Object, NullLogger<LoginViewModel>.Instance);

        _authClientService
            .Setup(authClientService => authClientService.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.InvalidCredentials);
    }

    [Fact]
    public async Task Login_WhenServerError_SetsErrorState()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authClientService.Object, NullLogger<LoginViewModel>.Instance);

        _authClientService
            .Setup(authClientService => authClientService.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Value.Should().Be(LoginState.Error);
    }

    [Fact]
    public async Task Login_WhenForbidden_SetsStateToAccountLocked()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authClientService.Object, NullLogger<LoginViewModel>.Instance);

        _authClientService
            .Setup(authClientService => authClientService.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Error.Forbidden());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.AccountLocked);
    }
}
