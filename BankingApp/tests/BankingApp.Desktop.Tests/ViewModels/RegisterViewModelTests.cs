namespace BankingApp.Desktop.Tests.ViewModels;

using Enums;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class RegisterViewModelTests
{
    private readonly Mock<IAuthService> _authClientService = new();

    [Fact]
    public async Task Register_WhenEmptyFields_SetsErrorState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authClientService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register(string.Empty, "pass", "pass", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.Error);
    }

    [Fact]
    public async Task Register_WhenPasswordMismatch_SetsPasswordMismatchState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authClientService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "Password123!", "Password123", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.PasswordMismatch);
    }

    [Fact]
    public async Task Register_WhenWeakPassword_SetsWeakPasswordState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authClientService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "weak", "weak", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.WeakPassword);
    }

    [Fact]
    public async Task Register_WhenValid_SetsSuccessState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authClientService.Object,
            NullLogger<RegisterViewModel>.Instance);

        _authClientService
            .Setup(authClientService => authClientService.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.Success);
    }

    [Fact]
    public async Task Register_WhenEmailConflicts_SetsEmailAlreadyExistsState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authClientService.Object,
            NullLogger<RegisterViewModel>.Instance);

        _authClientService
            .Setup(authClientService => authClientService.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Error.Conflict("Conflict", "Conflict"));

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.EmailAlreadyExists);
    }
}
