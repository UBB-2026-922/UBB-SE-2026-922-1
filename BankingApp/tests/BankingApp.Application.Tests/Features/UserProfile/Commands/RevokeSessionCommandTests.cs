namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using BankingApp.Application.Features.UserProfile.Commands;
using BankingApp.Domain.Common.Errors;
using Domain.Aggregates.IdentityAggregate.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Persistence;

public sealed class RevokeSessionCommandTests
{
    private const int TestUserId = 1;
    private const int SessionId = 10;
    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync((IdentityAccount?)null);
        RevokeSessionCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new RevokeSessionCommand(TestUserId, SessionId), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldReturnSessionNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap("hash"));
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        RevokeSessionCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new RevokeSessionCommand(TestUserId, SessionId), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.SessionNotFound);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldRevokeSessionAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap("hash"));
        Session session = identity.OpenSession("token", _testNow.AddDays(1), _testNow, null, null, null);
        SetEntityId(session, SessionId);
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _identityRepositoryMock.Setup(repository => repository.UpdateAsync(identity, cancellationToken)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);
        RevokeSessionCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new RevokeSessionCommand(TestUserId, SessionId), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        session.IsRevoked.Should().BeTrue();
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private RevokeSessionCommandHandler CreateHandler() =>
        new(_identityRepositoryMock.Object, _unitOfWorkMock.Object, NullLogger<RevokeSessionCommandHandler>.Instance);

    private static void SetEntityId<T>(T entity, int id)
        where T : class
    {
        typeof(T).BaseType!.GetProperty("Id")!.SetValue(entity, id);
    }
}
