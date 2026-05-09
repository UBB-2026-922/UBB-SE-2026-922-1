namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.UserRegistration.Commands;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private RegisterCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _hashService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<RegisterCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsEmailRegisteredError()
    {
        var existing = User.Register(Email.Create("taken@test.com").Value, "Existing", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var command = new RegisterCommand("taken@test.com", "ValidPass1!", "New User");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenHashFails_ReturnsError()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _hashService.Setup(s => s.GetHash(It.IsAny<string>()))
            .Returns(Error.Failure("hash.error"));
        var command = new RegisterCommand("new@test.com", "ValidPass1!", "New User");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash.error");
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesUserAndIdentity()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new RegisterCommand("new@test.com", "ValidPass1!", "New User");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.FullName == "New User"), It.IsAny<CancellationToken>()), Times.Once);
        _identityRepo.Verify(r => r.AddAsync(It.IsAny<IdentityAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_SavesChanges()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new RegisterCommand("new@test.com", "ValidPass1!", "New User");

        await CreateHandler().Handle(command, CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenValid_TrimsFullName()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new RegisterCommand("new@test.com", "ValidPass1!", "  Spaced Name  ");

        await CreateHandler().Handle(command, CancellationToken.None);

        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.FullName == "Spaced Name"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
