namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Application.Features.Authentication.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class LoginViewModelTests
{
    private readonly Mock<IAuthClientService> _authServiceMock = new();

    public LoginViewModelTests()
    {
        _authServiceMock.Setup(mock => mock.EnsureConfigured()).Returns(Result.Success);
        _authServiceMock.SetupProperty(mock => mock.CurrentUserId);
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
    public async Task Login_WhenSuccess_ShouldSetLoginStateToSuccessAndSetUserId()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authServiceMock.Object, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { Token = "test-token", UserId = 1, Requires2Fa = false };

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(response);
        _authServiceMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.Success);
        _authServiceMock.Object.CurrentUserId.Should().Be(1);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenRequires2FA_ShouldSetLoginStateToRequire2Fa()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authServiceMock.Object, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { UserId = 1, Requires2Fa = true };

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(response);

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.Require2Fa);
        _authServiceMock.Object.CurrentUserId.Should().Be(1);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsUnauthorized_ShouldSetLoginStateToInvalidCredentials()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authServiceMock.Object, NullLogger<LoginViewModel>.Instance);

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.InvalidCredentials);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsFailure_ShouldSetLoginStateToError()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authServiceMock.Object, NullLogger<LoginViewModel>.Instance);

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(Error.Failure());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.Error);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsForbidden_ShouldSetLoginStateToAccountLocked()
    {
        // Arrange
        var viewModel = new LoginViewModel(_authServiceMock.Object, NullLogger<LoginViewModel>.Instance);

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(Error.Forbidden());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.AccountLocked);
        _authServiceMock.VerifyAll();
    }
}