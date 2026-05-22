namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using BankingApp.Application.Features.UserProfile.Commands;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Persistence;

public sealed class Enable2FaCommandTests
{
    private const int TestUserId = 1;

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync((IdentityAccount?)null);
        Enable2FaCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new Enable2FaCommand(TestUserId, TwoFactorMethod.Authenticator), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldEnable2FaWithChosenMethod()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap("hash"));
        SetupValid(identity, cancellationToken);
        Enable2FaCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new Enable2FaCommand(TestUserId, TwoFactorMethod.Email), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        identity.Is2FaEnabled.Should().BeTrue();
        identity.Preferred2FaMethod.Should().Be(TwoFactorMethod.Email);
        VerifyValid(identity, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap("hash"));
        SetupValid(identity, cancellationToken);
        Enable2FaCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<Success> result = await handler.Handle(new Enable2FaCommand(TestUserId, TwoFactorMethod.Authenticator), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        VerifyValid(identity, cancellationToken);
    }

    private Enable2FaCommandHandler CreateHandler() =>
        new(_identityRepositoryMock.Object, _unitOfWorkMock.Object, NullLogger<Enable2FaCommandHandler>.Instance);

    private void SetupValid(IdentityAccount identity, CancellationToken cancellationToken)
    {
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _identityRepositoryMock.Setup(repository => repository.UpdateAsync(identity, cancellationToken)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);
    }

    private void VerifyValid(IdentityAccount identity, CancellationToken cancellationToken)
    {
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }
}
