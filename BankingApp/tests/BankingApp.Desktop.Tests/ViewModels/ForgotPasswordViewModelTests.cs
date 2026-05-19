namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Shared.Enums;

/// <summary>
///     Tests for <see cref="ForgotPasswordViewModel" />.
/// </summary>
public class ForgotPasswordViewModelTests
{
    private readonly Mock<IPasswordRecoveryService> _passwordRecoveryService = new();

    /// <summary>
    ///     When the email is empty, state transitions to <see cref="ForgotPasswordState.Error" />
    ///     and a validation message is set without calling the recovery manager.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPassword_WhenEmailEmpty_SetsErrorState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);
        _passwordRecoveryService
            .Setup(requestsCodeAsync => requestsCodeAsync.RequestCodeAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForgotPasswordState.EmailSent);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);
        _passwordRecoveryService
            .Setup(checksIsPasswordValid => checksIsPasswordValid.IsPasswordValid(It.IsAny<string>()))
            .Returns(false);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);
        _passwordRecoveryService
            .Setup(checksIsPasswordValid => checksIsPasswordValid.IsPasswordValid(It.IsAny<string>()))
            .Returns(true);
        _passwordRecoveryService
            .Setup(resetsPasswordAsync =>
                resetsPasswordAsync.ResetPasswordAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForgotPasswordState.PasswordResetSuccess);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);

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
        var viewModel = new ForgotPasswordViewModel(_passwordRecoveryService.Object);
        _passwordRecoveryService
            .Setup(verifiesTokenAsync => verifiesTokenAsync.VerifyTokenAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForgotPasswordState.TokenValid);

        // Act
        await viewModel.VerifyToken("123456");

        // Assert
        viewModel.State.Should().Be(ForgotPasswordState.TokenValid);
        viewModel.ValidationError.Should().BeEmpty();
    }
}
