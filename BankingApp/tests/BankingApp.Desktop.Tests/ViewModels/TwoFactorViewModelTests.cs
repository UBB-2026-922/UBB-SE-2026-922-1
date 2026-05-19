namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.Authentication.Dtos;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Enums;
using Shared.Timers;
using Xunit;

public class TwoFactorViewModelTests
{
    private const int ExpectedResendCooldownSeconds = 30;

    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly Mock<IAuthenticationSession> _authenticationSession = new();
    private readonly Mock<ICountdownTimer> _countdownTimer = new();

    [Fact]
    public async Task VerifyOtp_WhenCodeTooShort_SetsErrorAndDoesNotCallApi()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authenticationService.Object,
            _authenticationSession.Object,
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
        _authenticationService.Verify(
            s => s.VerifyOtpAsync(It.IsAny<VerifyOtpRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task VerifyOtp_WhenUserIdIsNull_SetsInvalidOtpState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authenticationService.Object,
            _authenticationSession.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authenticationSession.Setup(authenticationSession => authenticationSession.CurrentUserId).Returns((int?)null);

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
            _authenticationService.Object,
            _authenticationSession.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authenticationSession.Setup(authenticationSession => authenticationSession.CurrentUserId).Returns(1);

        var successResponse = new LoginSuccessResponse { Token = "token", UserId = 1 };
        _authenticationService
            .Setup(authenticationService => authenticationService.VerifyOtpAsync(
                It.IsAny<VerifyOtpRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);
        _authenticationSession.Setup(authenticationSession => authenticationSession.SetToken(It.IsAny<string>()));

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
            _authenticationService.Object,
            _authenticationSession.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authenticationSession.Setup(authenticationSession => authenticationSession.CurrentUserId).Returns(1);

        _authenticationService
            .Setup(authenticationService => authenticationService.VerifyOtpAsync(
                It.IsAny<VerifyOtpRequest>(),
                It.IsAny<CancellationToken>()))
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
            _authenticationService.Object,
            _authenticationSession.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authenticationSession.Setup(authenticationSession => authenticationSession.CurrentUserId).Returns(1);
        _authenticationService
            .Setup(authenticationService => authenticationService.ResendOtpAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        await viewModel.ResendOtp();

        // Act
        await viewModel.ResendOtp();

        // Assert
        _authenticationService.Verify(
            s => s.ResendOtpAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendOtp_WhenCanResend_CallsApiAndStartsTimer()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(
            _authenticationService.Object,
            _authenticationSession.Object,
            _countdownTimer.Object,
            NullLogger<TwoFactorViewModel>.Instance);
        _authenticationSession.Setup(authenticationSession => authenticationSession.CurrentUserId).Returns(1);
        _authenticationService
            .Setup(authenticationService => authenticationService.ResendOtpAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.ResendOtp();

        // Assert
        _countdownTimer.Verify(t => t.Start(), Times.Once);
        viewModel.SecondsRemaining.Should().Be(ExpectedResendCooldownSeconds);
    }
}
