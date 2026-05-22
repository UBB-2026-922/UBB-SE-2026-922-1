namespace BankingApp.Application.Tests.Features.PasswordReset.Commands;

using BankingApp.Application.Common.Notifications;
using BankingApp.Application.Features.PasswordReset.Commands;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class ForgotPasswordCommandTests
{
    private const int TestUserId = 1;
    private const string TestEmail = "test@example.com";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IEmailService> _emailServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnSuccessWithoutRevealingAbsence()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync((User?)null);

        ForgotPasswordCommandHandler handler = CreateHandler();
        var command = new ForgotPasswordCommand(TestEmail);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnSuccessWithoutRevealingAbsence()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(user);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(user.Id, cancellationToken))
            .ReturnsAsync((IdentityAccount?)null);

        ForgotPasswordCommandHandler handler = CreateHandler();
        var command = new ForgotPasswordCommand(TestEmail);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(user.Id, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldIssuePasswordResetToken()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap("old-hash"));
        SetupValidFlow(user, identity, cancellationToken);

        ForgotPasswordCommandHandler handler = CreateHandler();
        var command = new ForgotPasswordCommand(TestEmail);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        identity.PasswordResetTokens.Should().ContainSingle();
        PasswordResetToken resetToken = identity.PasswordResetTokens.Single();
        resetToken.TokenHash.Should().NotBeNullOrWhiteSpace();
        resetToken.ExpiresAt.Should().Be(_testNow.AddMinutes(30));
        resetToken.CreatedAt.Should().Be(_testNow);
        resetToken.UsedAt.Should().BeNull();

        VerifyValidFlow(user, identity, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSendPasswordResetEmail()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap("old-hash"));
        string? sentToken = null;
        SetupValidFlow(user, identity, cancellationToken, (_, token) => sentToken = token);

        ForgotPasswordCommandHandler handler = CreateHandler();
        var command = new ForgotPasswordCommand(TestEmail);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        sentToken.Should().NotBeNullOrWhiteSpace();

        VerifyValidFlow(user, identity, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap("old-hash"));
        SetupValidFlow(user, identity, cancellationToken);

        ForgotPasswordCommandHandler handler = CreateHandler();
        var command = new ForgotPasswordCommand(TestEmail);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        VerifyValidFlow(user, identity, cancellationToken);
    }

    private ForgotPasswordCommandHandler CreateHandler()
    {
        return new ForgotPasswordCommandHandler(
            _userRepositoryMock.Object,
            _identityRepositoryMock.Object,
            _emailServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<ForgotPasswordCommandHandler>.Instance);
    }

    private void SetupValidFlow(
        User user,
        IdentityAccount identity,
        CancellationToken cancellationToken,
        Action<string, string>? onEmail = null)
    {
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(user);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(identity);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _identityRepositoryMock
            .Setup(repository => repository.UpdateAsync(identity, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        if (onEmail is null)
        {
            _emailServiceMock
                .Setup(service => service.SendPasswordResetLinkAsync(TestEmail, It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }
        else
        {
            _emailServiceMock
                .Setup(service => service.SendPasswordResetLinkAsync(TestEmail, It.IsAny<string>()))
                .Callback<string, string>((email, token) => onEmail(email, token))
                .Returns(Task.CompletedTask);
        }
    }

    private void VerifyValidFlow(User user, IdentityAccount identity, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(user.Id, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _emailServiceMock.Verify(service => service.SendPasswordResetLinkAsync(TestEmail, It.IsAny<string>()), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _userRepositoryMock.VerifyNoOtherCalls();
        _identityRepositoryMock.VerifyNoOtherCalls();
        _emailServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static User CreateUser()
    {
        return User.Register(Email.Create(TestEmail).Value, "Test User", _testNow);
    }
}
