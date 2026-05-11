namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.PasswordReset.Commands;
using BankingApp.Application.Features.PasswordReset.Queries;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IEmailService> _emailService = MockFactory.CreateEmailService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ForgotPasswordCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _emailService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<ForgotPasswordCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenEmailInvalid_ReturnsSuccessWithoutEmail()
    {
        var command = new ForgotPasswordCommand("not-an-email");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _emailService.Verify(s => s.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsSuccessWithoutEmail()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new ForgotPasswordCommand("notfound@test.com");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _emailService.Verify(s => s.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsSuccessWithoutEmail()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var command = new ForgotPasswordCommand("test@test.com");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _emailService.Verify(s => s.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_IssuesTokenAndSendsEmail()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ForgotPasswordCommand("test@test.com");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _emailService.Verify(s => s.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ResetPasswordCommandHandler CreateHandler() => new(
        _identityRepo.Object,
        _hashService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<ResetPasswordCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenTokenNotFound_ReturnsTokenInvalidError()
    {
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var command = new ResetPasswordCommand("unknown-token", "NewPassword1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTokenAlreadyUsed_ReturnsTokenAlreadyUsedError()
    {
        var identity = IdentityAccount.Create(1, null);
        var rawToken = "valid-token";
        var tokenHash = ComputeHash(rawToken);
        identity.IssuePasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow);
        var resetToken = identity.PasswordResetTokens.First();
        resetToken.MarkUsed(DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ResetPasswordCommand(rawToken, "NewPassword1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ReturnsTokenExpiredError()
    {
        var identity = IdentityAccount.Create(1, null);
        var rawToken = "expired-token";
        var tokenHash = ComputeHash(rawToken);
        identity.IssuePasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(-35));
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ResetPasswordCommand(rawToken, "NewPassword1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenHashFails_ReturnsError()
    {
        var identity = IdentityAccount.Create(1, null);
        var rawToken = "valid-token";
        var tokenHash = ComputeHash(rawToken);
        identity.IssuePasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow);
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.GetHash(It.IsAny<string>()))
            .Returns(Error.Failure("hash.error"));
        var command = new ResetPasswordCommand(rawToken, "NewPassword1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash.error");
    }

    [Fact]
    public async Task Handle_WhenTokenValid_ResetsPasswordAndInvalidatesSessions()
    {
        var identity = IdentityAccount.Create(1, HashedPassword.Wrap("old-hash"));
        identity.OpenSession("session-token", DateTime.UtcNow.AddHours(24), DateTime.UtcNow);
        var rawToken = "valid-token";
        var tokenHash = ComputeHash(rawToken);
        identity.IssuePasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow);
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new ResetPasswordCommand(rawToken, "NewPassword1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string ComputeHash(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed class VerifyResetTokenQueryHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private VerifyResetTokenQueryHandler CreateHandler() => new(
        _identityRepo.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenTokenNotFound_ReturnsError()
    {
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        var query = new VerifyResetTokenQuery("unknown");

        ErrorOr<Success> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTokenValid_ReturnsSuccess()
    {
        var identity = IdentityAccount.Create(1, null);
        var rawToken = "valid-token";
        var tokenHash = ComputeHash(rawToken);
        identity.IssuePasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow);
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var query = new VerifyResetTokenQuery(rawToken);

        ErrorOr<Success> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
    }

    private static string ComputeHash(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
