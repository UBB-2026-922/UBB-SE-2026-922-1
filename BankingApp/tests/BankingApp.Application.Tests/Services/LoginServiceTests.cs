namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Authentication.Commands;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ResendOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly Mock<IOtpAttemptTracker> _otpTracker = MockFactory.CreateOtpAttemptTracker();
    private readonly Mock<IEmailService> _emailService = MockFactory.CreateEmailService();

    private ResendOtpCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _otpService.Object,
        _otpTracker.Object,
        _emailService.Object,
        NullLogger<ResendOtpCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsError()
    {
        var command = new ResendOtpCommand(1, "Email");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsError()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var command = new ResendOtpCommand(1, "Email");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOtpGenerationFails_ReturnsError()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _otpService.Setup(s => s.GenerateTotp(It.IsAny<int>()))
            .Returns(Error.Failure("otp.failed"));
        _otpService.Setup(s => s.GenerateSmsOtp(It.IsAny<int>()))
            .Returns(Error.Failure("otp.failed"));
        var command = new ResendOtpCommand(1, "Email");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenEmailMethod_SendsOtpEmail()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        identity.Enable2Fa(TwoFactorMethod.Email);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ResendOtpCommand(1, "Email");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _emailService.Verify(s => s.SendOtpCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatorMethod_DoesNotSendEmail()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        identity.Enable2Fa(TwoFactorMethod.Authenticator);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ResendOtpCommand(1, "Authenticator");

        await CreateHandler().Handle(command, CancellationToken.None);

        _emailService.Verify(s => s.SendOtpCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
