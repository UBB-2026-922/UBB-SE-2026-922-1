// <copyright file="LoginServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="LoginService" />.
/// </summary>
public class LoginServiceTests
{
    private const int AttemptsBeforeLockout = 4;
    private const int LockoutLowerBoundMinutes = 14;
    private const int LockoutUpperBoundMinutes = 16;
    private readonly Mock<IAuthRepository> _authRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IEmailService> _emailService = MockFactory.CreateEmailService();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly Mock<IJsonWebTokenService> _jwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpAttemptTracker> _otpAttemptTracker = new();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly LoginService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginServiceTests" /> class.
    /// </summary>
    public LoginServiceTests()
    {
        _service = new LoginService(
            _authRepository.Object,
            _hashService.Object,
            _jwtService.Object,
            _otpService.Object,
            _emailService.Object,
            _otpAttemptTracker.Object,
            NullLogger<LoginService>.Instance);
    }

    /// <summary>
    ///     Verifies the Login_WhenEmailIsInvalid_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenEmailIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new LoginRequest { Email = "not-an-email", Password = "ValidPass1!" };

        // Act
        ErrorOr<LoginSuccess> result = _service.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    /// <summary>
    ///     Verifies the Login_WhenValid_CreatesSessionWithMetadata scenario.
    /// </summary>
    [Fact]
    public void Login_WhenValid_CreatesSessionWithMetadata()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "ValidPass1!" };
        var metadata = new SessionMetadata
        {
            DeviceInfo = "Windows",
            Browser = "Edge",
            IpAddress = "127.0.0.1",
        };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
        };

        _authRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _hashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash))
            .Returns(true);
        _jwtService.Setup(generatesToken => generatesToken.GenerateToken(user.Id))
            .Returns((ErrorOr<string>)"jwt-token");
        _authRepository.Setup(createsSession => createsSession.CreateSession(
                user.Id,
                "jwt-token",
                metadata.DeviceInfo,
                metadata.Browser,
                metadata.IpAddress))
            .Returns(new Session());

        // Act
        ErrorOr<LoginSuccess> result = _service.Login(request, metadata);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<FullLogin>();
        _authRepository.Verify(
            createsSession => createsSession.CreateSession(
                user.Id,
                "jwt-token",
                metadata.DeviceInfo,
                metadata.Browser,
                metadata.IpAddress),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the Login_WhenAuthenticatorTwoFactorIsEnabled_GeneratesTotp scenario.
    /// </summary>
    [Fact]
    public void Login_WhenAuthenticatorTwoFactorIsEnabled_GeneratesTotp()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "ValidPass1!" };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
            Is2FaEnabled = true,
            Preferred2FaMethod = TwoFactorMethod.Authenticator,
        };

        _authRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _hashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash))
            .Returns(true);
        _otpService.Setup(generatesTotp => generatesTotp.GenerateTotp(user.Id))
            .Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<LoginSuccess> result = _service.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
        _emailService.Verify(
            sendsOtpCode => sendsOtpCode.SendOtpCode(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies the Login_WhenMaxFailedAttemptsReached_LocksForFifteenMinutes scenario.
    /// </summary>
    [Fact]
    public void Login_WhenMaxFailedAttemptsReached_LocksForFifteenMinutes()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "wrong" };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
            FailedLoginAttempts = AttemptsBeforeLockout,
        };
        DateTime before = DateTime.UtcNow.AddMinutes(LockoutLowerBoundMinutes);

        _authRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _hashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        ErrorOr<LoginSuccess> result = _service.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        _authRepository.Verify(
            locksAccount => locksAccount.LockAccount(
                user.Id,
                It.Is<DateTime>(lockoutEnd =>
                    lockoutEnd >= before && lockoutEnd <= DateTime.UtcNow.AddMinutes(LockoutUpperBoundMinutes))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the Login_WhenEmailTwoFactorIsEnabled_GeneratesEmailOtp scenario.
    /// </summary>
    [Fact]
    public void Login_WhenEmailTwoFactorIsEnabled_GeneratesEmailOtp()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "ValidPass1!" };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
            Is2FaEnabled = true,
            Preferred2FaMethod = TwoFactorMethod.Email,
        };

        _authRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _hashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash))
            .Returns(true);
        _otpService.Setup(generatesSmsOtp => generatesSmsOtp.GenerateSmsOtp(user.Id))
            .Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<LoginSuccess> result = _service.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
        _emailService.Verify(sendsOtpCode => sendsOtpCode.SendOtpCode(user.Email, "123456"), Times.Once);
    }

    /// <summary>
    ///     Verifies the VerifyOtp_WhenThreeInvalidAttempts_InvalidatesOtpAndRequiresRestart scenario.
    /// </summary>
    [Fact]
    public void VerifyOtp_WhenThreeInvalidAttempts_InvalidatesOtpAndRequiresRestart()
    {
        // Arrange
        var request = new VerifyOtpRequest { UserId = 1, OtpCode = "000000" };
        var user = new User { Id = request.UserId, Preferred2FaMethod = TwoFactorMethod.Email };

        _authRepository.Setup(findsUserById => findsUserById.FindUserById(user.Id))
            .Returns((ErrorOr<User>)user);
        _otpService.Setup(verifiesSmsOtp => verifiesSmsOtp.VerifySmsOtp(user.Id, request.OtpCode))
            .Returns(false);
        _otpAttemptTracker.SetupSequence(recordsFailure => recordsFailure.RecordFailure(user.Id))
            .Returns(1)
            .Returns(2)
            .Returns(3);
        _otpAttemptTracker.Setup(resets => resets.Reset(user.Id));

        // Act
        _ = _service.VerifyOtp(request);
        _ = _service.VerifyOtp(request);
        ErrorOr<LoginSuccess> result = _service.VerifyOtp(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("otp_attempts_exceeded");
        _otpService.Verify(invalidatesOtp => invalidatesOtp.InvalidateOtp(user.Id), Times.Once);
    }
}
