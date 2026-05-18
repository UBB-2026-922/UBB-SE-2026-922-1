namespace BankingApp.Desktop.Tests.ViewModels;

using Application.Features.Authentication.Dtos;
using Enums;
using Services;
using Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Configuration;
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
        LoginViewModel viewModel = CreateViewModel();
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1, Requires2Fa = false };

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
        LoginViewModel viewModel = CreateViewModel();
        LoginSuccessResponse response = new() { UserId = 1, Requires2Fa = true };

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
        LoginViewModel viewModel = CreateViewModel();

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
        LoginViewModel viewModel = CreateViewModel();

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
        LoginViewModel viewModel = CreateViewModel();

        _authServiceMock
            .Setup(mock => mock.LoginAsync("test@test.com", "password"))
            .ReturnsAsync(Error.Forbidden());

        // Act
        await viewModel.Login("test@test.com", "password");

        // Assert
        viewModel.State.Should().Be(LoginState.AccountLocked);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenConfiguredAndSuccessful_ShouldSetLoginStateToSuccessAndSetUserId()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1, Requires2Fa = false };

        _authServiceMock
            .Setup(mock => mock.LoginAsync("dev@test.com", "password"))
            .ReturnsAsync(response);
        _authServiceMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeFalse();
        viewModel.State.Should().Be(LoginState.Success);
        _authServiceMock.Object.CurrentUserId.Should().Be(1);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenMissingConfiguration_ShouldReturnErrorAndNotCallApi()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DevLogin.NotConfigured");
        _authServiceMock.Verify(mock => mock.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DevLogin_WhenRequires2Fa_ShouldReturnErrorAndResetState()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        LoginSuccessResponse response = new() { UserId = 1, Requires2Fa = true };

        _authServiceMock
            .Setup(mock => mock.LoginAsync("dev@test.com", "password"))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DevLogin.Requires2Fa");
        viewModel.State.Should().Be(LoginState.Idle);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenApiReturnsError_ShouldReturnErrorAndResetState()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        var apiError = Error.Unauthorized();

        _authServiceMock
            .Setup(mock => mock.LoginAsync("dev@test.com", "password"))
            .ReturnsAsync(apiError);

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(apiError);
        viewModel.State.Should().Be(LoginState.Idle);
        _authServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenTokenMissing_ShouldReturnErrorAndResetState()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        LoginSuccessResponse response = new() { UserId = 1, Requires2Fa = false, Token = null };

        _authServiceMock
            .Setup(mock => mock.LoginAsync("dev@test.com", "password"))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DevLogin.MissingToken");
        viewModel.State.Should().Be(LoginState.Idle);
        _authServiceMock.VerifyAll();
    }

    private LoginViewModel CreateViewModel(Dictionary<string, string?>? values = null)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();

        return new LoginViewModel(
            _authServiceMock.Object,
            configuration,
            NullLogger<LoginViewModel>.Instance);
    }
}
