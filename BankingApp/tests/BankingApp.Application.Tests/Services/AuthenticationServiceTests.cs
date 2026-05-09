namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Authentication.Commands;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly Mock<IJsonWebTokenService> _jwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly Mock<IOtpAttemptTracker> _otpTracker = MockFactory.CreateOtpAttemptTracker();
    private readonly Mock<IEmailService> _emailService = MockFactory.CreateEmailService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private LoginCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _hashService.Object,
        _jwtService.Object,
        _otpService.Object,
        _otpTracker.Object,
        _emailService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<LoginCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenEmailIsInvalid_ReturnsError()
    {
        var command = new LoginCommand("not-an-email", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsError()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAccountCurrentlyLocked_ReturnsAccountLocked()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        identity.LockAccount(DateTime.UtcNow.AddHours(1));
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenLockExpired_ResetsAndContinues()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        identity.LockAccount(DateTime.UtcNow.AddHours(-1));
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenOAuthOnlyAccount_ReturnsInvalidCredentials()
    {
        var emailValue = Email.Create("oauth@test.com").Value;
        var user = User.Register(emailValue, "OAuth User", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new LoginCommand("oauth@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _hashService.Verify(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWrongPassword_ReturnsInvalidCredentials()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)false);
        var command = new LoginCommand("test@test.com", "wrong-password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenMaxFailedAttempts_LocksAccount()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        for (int i = 0; i < 4; i++)
        {
            identity.IncrementFailedAttempts();
        }

        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)false);
        var command = new LoginCommand("test@test.com", "wrong");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenHashVerifyFails_ReturnsError()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Error.Failure("hash.failed", "Hash verify error"));
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash.failed");
    }

    [Fact]
    public async Task Handle_When2FaEnabled_ReturnsRequiresTwoFactor()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair(is2FaEnabled: true, method: TwoFactorMethod.Email);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
    }

    [Fact]
    public async Task Handle_When2FaOtpGenerationFails_ReturnsError()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair(is2FaEnabled: true, method: TwoFactorMethod.Email);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        _otpService.Setup(s => s.GenerateSmsOtp(It.IsAny<int>()))
            .Returns(Error.Failure("otp.failed", "OTP generation failed"));
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenJwtGenerationFails_ReturnsError()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        _jwtService.Setup(s => s.GenerateToken(It.IsAny<int>()))
            .Returns(Error.Failure("jwt.failed", "JWT generation failed"));
        var command = new LoginCommand("test@test.com", "password");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("jwt.failed");
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_ReturnsFullLogin()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        _jwtService.Setup(s => s.GenerateToken(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"jwt-token");
        var command = new LoginCommand("test@test.com", "ValidPassword1!");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<FullLogin>();
        ((FullLogin)result.Value).Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_SavesSession()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        var command = new LoginCommand("test@test.com", "ValidPassword1!");

        await CreateHandler().Handle(command, CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_SendsLoginAlert()
    {
        var (user, identity) = MockFactory.CreateAuthenticatedPair();
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        var command = new LoginCommand("test@test.com", "ValidPassword1!");

        await CreateHandler().Handle(command, CancellationToken.None);

        _emailService.Verify(s => s.SendLoginAlertAsync(It.IsAny<string>()), Times.Once);
    }
}

public sealed class VerifyOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IJsonWebTokenService> _jwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly Mock<IOtpAttemptTracker> _otpTracker = MockFactory.CreateOtpAttemptTracker();
    private readonly Mock<IEmailService> _emailService = MockFactory.CreateEmailService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _otpService.Object,
        _otpTracker.Object,
        _jwtService.Object,
        _emailService.Object,
        _unitOfWork.Object,
        _clock.Object,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsError()
    {
        var command = new BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommand(1, "123456");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOtpInvalid_ReturnsUnauthorized()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _otpService.Setup(s => s.VerifySmsOtp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)false);
        var command = new BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommand(1, "000000");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenOtpValid_ReturnsFullLogin()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _jwtService.Setup(s => s.GenerateToken(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"otp-token");
        var command = new BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommand(1, "123456");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<FullLogin>();
    }

    [Fact]
    public async Task Handle_WhenMaxOtpAttemptsReached_ReturnsError()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _otpService.Setup(s => s.VerifySmsOtp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)false);
        _otpTracker.Setup(t => t.RecordFailure(It.IsAny<int>())).Returns(3);
        var command = new BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommand(1, "000000");

        ErrorOr<LoginSuccess> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOtpValid_InvalidatesOtp()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new BankingApp.Application.Features.Authentication.Commands.VerifyOtpCommand(1, "123456");

        await CreateHandler().Handle(command, CancellationToken.None);

        _otpService.Verify(s => s.InvalidateOtp(It.IsAny<int>()), Times.Once);
    }
}

public sealed class LogoutCommandHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private BankingApp.Application.Features.Authentication.Commands.LogoutCommandHandler CreateHandler() => new(
        _identityRepo.Object,
        _unitOfWork.Object,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<BankingApp.Application.Features.Authentication.Commands.LogoutCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSessionNotFound_ReturnsError()
    {
        _identityRepo.Setup(r => r.GetBySessionTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var command = new BankingApp.Application.Features.Authentication.Commands.LogoutCommand("invalid-token");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSessionFound_RevokesAndReturnsSuccess()
    {
        var identity = IdentityAccount.Create(1, null);
        identity.OpenSession("valid-token", DateTime.UtcNow.AddHours(24), DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetBySessionTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new BankingApp.Application.Features.Authentication.Commands.LogoutCommand("valid-token");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
