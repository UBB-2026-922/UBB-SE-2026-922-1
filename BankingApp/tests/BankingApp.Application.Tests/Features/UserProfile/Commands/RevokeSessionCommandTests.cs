namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using BankingApp.Application.Features.UserProfile.Commands;
using BankingApp.Domain.Aggregates.IdentityAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Persistence;

public sealed class RevokeSessionCommandTests
{
    private const int TestUserId = 7;

    private static readonly DateTime _testNow = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = MockFactory.CreateIdentityRepositoryMock();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        RevokeSessionCommandHandler handler = CreateHandler();
        RevokeSessionCommand command = new(TestUserId, 1);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldReturnSessionNotFoundError()
    {
        // Arrange
        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        RevokeSessionCommandHandler handler = CreateHandler();
        RevokeSessionCommand command = new(TestUserId, 999);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.SessionNotFound);
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldMarkSessionRevokedAndSaveChanges()
    {
        // Arrange
        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        Session session = identity.OpenSession("test-token", _testNow.AddHours(24), _testNow);
        int sessionId = session.Id;

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        RevokeSessionCommandHandler handler = CreateHandler();
        RevokeSessionCommand command = new(TestUserId, sessionId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        session.IsRevoked.Should().BeTrue();
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.Verify(
            repository => repository.UpdateAsync(identity, CancellationToken.None),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private RevokeSessionCommandHandler CreateHandler()
    {
        return new RevokeSessionCommandHandler(
            _identityRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<RevokeSessionCommandHandler>.Instance);
    }
}
