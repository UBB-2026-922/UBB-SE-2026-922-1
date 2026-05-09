namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.UserProfile.Commands;
using BankingApp.Application.Features.UserProfile.Dtos;
using BankingApp.Application.Features.UserProfile.Queries;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class GetProfileQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();

    private GetProfileQueryHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        NullLogger<GetProfileQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        var query = new GetProfileQuery(1);

        ErrorOr<ProfileDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserFound_ReturnsProfile()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test User", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var query = new GetProfileQuery(1);

        ErrorOr<ProfileDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.FullName.Should().Be("Test User");
    }
}

public sealed class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private UpdateProfileCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<UpdateProfileCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsError()
    {
        var command = new UpdateProfileCommand(1, "New Name", null, null, null, null, null);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserFound_UpdatesAndSaves()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Old Name", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var command = new UpdateProfileCommand(1, "New Name", "+40721000000", null, null, null, "en");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private ChangePasswordCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _hashService.Object,
        _unitOfWork.Object,
        NullLogger<ChangePasswordCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsError()
    {
        var command = new ChangePasswordCommand(1, "OldPass1!", "NewPass1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordWrong_ReturnsError()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap("correct-hash"));
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)false);
        var command = new ChangePasswordCommand(1, "WrongOldPass1!", "NewPass1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesPassword()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap("old-hash"));
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        _hashService.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        _hashService.Setup(s => s.GetHash(It.IsAny<string>()))
            .Returns((ErrorOr<string>)"new-hash");
        var command = new ChangePasswordCommand(1, "OldPass1!", "NewPass1!");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class Enable2FaCommandHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private Enable2FaCommandHandler CreateHandler() => new(
        _identityRepo.Object,
        _unitOfWork.Object,
        NullLogger<Enable2FaCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsError()
    {
        var command = new Enable2FaCommand(1, TwoFactorMethod.Email);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_Enables2Fa()
    {
        var identity = IdentityAccount.Create(1, null);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new Enable2FaCommand(1, TwoFactorMethod.Email);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        identity.Is2FaEnabled.Should().BeTrue();
    }
}

public sealed class RevokeSessionCommandHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private RevokeSessionCommandHandler CreateHandler() => new(
        _identityRepo.Object,
        _unitOfWork.Object,
        NullLogger<RevokeSessionCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsError()
    {
        var command = new RevokeSessionCommand(1, 42);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ReturnsError()
    {
        var identity = IdentityAccount.Create(1, null);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var command = new RevokeSessionCommand(1, 999);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSessionExists_RevokesSession()
    {
        var identity = IdentityAccount.Create(1, null);
        identity.OpenSession("token", DateTime.UtcNow.AddHours(24), DateTime.UtcNow);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var sessionId = identity.Sessions.First().Id;
        var command = new RevokeSessionCommand(1, sessionId);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class GetActiveSessionsQueryHandlerTests
{
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();

    private GetActiveSessionsQueryHandler CreateHandler() => new(
        _identityRepo.Object,
        NullLogger<GetActiveSessionsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ReturnsError()
    {
        var query = new GetActiveSessionsQuery(1);

        ErrorOr<List<SessionDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIdentityFound_ReturnsActiveSessions()
    {
        var identity = IdentityAccount.Create(1, null);
        identity.OpenSession("token1", DateTime.UtcNow.AddHours(24), DateTime.UtcNow, "PC");
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var query = new GetActiveSessionsQuery(1);

        ErrorOr<List<SessionDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
    }
}
