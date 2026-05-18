namespace BankingApp.Desktop.Tests.ViewModels;

using Enums;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using Contracts.Features.Authentication.Dtos;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class TwoFactorViewModelTests
{
    private const int ExpectedResendCooldownSeconds = 30;

    private readonly Mock<IAuthService> _authClientService = new();
    private readonly Mock<ICountdownTimer> _countdownTimer = new();

    [Fact]
    public async Task VerifyOtp_WhenCodeTooShort_SetsErrorAndDoesNotCallApi()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance)
        {
            OtpCode = "123"
        };

        // Act
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Should().Be(TwoFactorState.Idle);
        viewModel.HasError.Should().BeTrue();
        _authClientService.Verify(
            s => s.VerifyOtpAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task VerifyOtp_WhenUserIdIsNull_SetsInvalidOtpState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authClientService.Setup(authClientService => authClientService.CurrentUserId).Returns((int?)null);

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Should().Be(TwoFactorState.InvalidOtp);
        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_WhenApiSucceeds_SetsSuccessState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authClientService.Setup(authClientService => authClientService.CurrentUserId).Returns(1);

        var successResponse = new LoginSuccessResponse { Token = "token", UserId = 1 };
        _authClientService
            .Setup(authClientService => authClientService.VerifyOtpAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(successResponse);
        _authClientService.Setup(authClientService => authClientService.SetToken(It.IsAny<string>()));

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Should().Be(TwoFactorState.Success);
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtp_WhenApiFails_SetsInvalidOtpState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authClientService.Setup(authClientService => authClientService.CurrentUserId).Returns(1);

        _authClientService
            .Setup(authClientService => authClientService.VerifyOtpAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(Error.Validation("invalid_otp"));

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Should().Be(TwoFactorState.InvalidOtp);
        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task ResendOtp_WhenInCooldown_DoesNotMakeSecondApiCall()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authClientService.Setup(authClientService => authClientService.CurrentUserId).Returns(1);
        _authClientService
            .Setup(authClientService => authClientService.ResendOtpAsync(It.IsAny<int>()))
            .ReturnsAsync(new object());

        await viewModel.ResendOtp();

        // Act
        await viewModel.ResendOtp();

        // Assert
        _authClientService.Verify(
            s => s.ResendOtpAsync(It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendOtp_WhenCanResend_CallsApiAndStartsTimer()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authClientService.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authClientService.Setup(authClientService => authClientService.CurrentUserId).Returns(1);
        _authClientService
            .Setup(authClientService => authClientService.ResendOtpAsync(It.IsAny<int>()))
            .ReturnsAsync(new object());

        // Act
        await viewModel.ResendOtp();

        // Assert
        _countdownTimer.Verify(t => t.Start(), Times.Once);
        viewModel.SecondsRemaining.Should().Be(ExpectedResendCooldownSeconds);
    }
}
