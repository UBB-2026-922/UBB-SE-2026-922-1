namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.PasswordReset.Dtos;
using ErrorOr;
using Shared.Enums;

/// <summary>
///     Tests for <see cref="ForgotPasswordViewModel" />.
/// </summary>
public class ForgotPasswordViewModelTests
{
    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly Mock<ISystemClock> _clock = new();

    public ForgotPasswordViewModelTests()
    {
        _clock.Setup(clock => clock.UtcNow).Returns(DateTime.UtcNow);
    }

    /// <summary>
    ///     When the email is empty, state transitions to <see cref="ForgotPasswordState.Error" />
    ///     and a validation message is set without calling the recovery manager.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPassword_WhenEmailEmpty_SetsErrorState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();

        // Act
        await viewModel.ForgotPassword(string.Empty);

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    ///     When a valid email is provided, the recovery manager is called and state transitions to
    ///     <see cref="ForgotPasswordState.EmailSent" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPassword_WhenValidEmail_SetsCodeSentState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();
        _authenticationService
            .Setup(authenticationService => authenticationService.ForgotPasswordAsync(
                It.IsAny<ForgotPasswordRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.ForgotPassword("test@bank.com");

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.EmailSent);
        viewModel.ValidationError.Should().BeEmpty();
    }

    /// <summary>
    ///     When both the token and the new password are empty, state transitions to
    ///     <see cref="ForgotPasswordState.Error" /> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenFieldsEmpty_SetsErrorState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();

        // Act
        await viewModel.ResetPassword(string.Empty, string.Empty);

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    ///     When the supplied password does not meet complexity requirements, state transitions to
    ///     <see cref="ForgotPasswordState.Error" /> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenPasswordTooWeak_SetsErrorState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();

        // Act
        await viewModel.ResetPassword("weak", "123456");

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    ///     When both the token and password are valid, state transitions to
    ///     <see cref="ForgotPasswordState.PasswordResetSuccess" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenValidFields_SetsSuccessState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();
        _authenticationService
            .Setup(authenticationService =>
                authenticationService.ResetPasswordAsync(
                    It.IsAny<ResetPasswordRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.ResetPassword("StrongP@ss1", "123456");

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.PasswordResetSuccess);
        viewModel.ValidationError.Should().BeEmpty();
    }

    /// <summary>
    ///     When an empty token is supplied for verification, state transitions to
    ///     <see cref="ForgotPasswordState.Error" /> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyToken_WhenEmpty_SetsErrorState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();

        // Act
        await viewModel.VerifyToken(string.Empty);

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    ///     When a non-empty token is supplied, the recovery manager validates it and state transitions to
    ///     <see cref="ForgotPasswordState.TokenValid" />.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyToken_WhenValid_SetsVerifiedState()
    {
        // Arrange
        ForgotPasswordViewModel viewModel = CreateViewModel();
        _authenticationService
            .Setup(authenticationService => authenticationService.VerifyResetTokenAsync(
                It.IsAny<VerifyResetTokenRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.VerifyToken("123456");

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.TokenValid);
        viewModel.ValidationError.Should().BeEmpty();
    }

    private ForgotPasswordViewModel CreateViewModel() =>
        new(_authenticationService.Object, _clock.Object);
}
