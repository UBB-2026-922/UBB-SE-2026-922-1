namespace BankingApp.Application.Tests.Features.UserProfile.Queries;

using BankingApp.Application.Common.Security;
using BankingApp.Application.Features.UserProfile.Queries;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class VerifyPasswordQueryTests
{
    private const int TestUserId = 1;
    private const string Password = "Password123!";
    private const string PasswordHash = "hash";

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHashService> _hashServiceMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync((IdentityAccount?)null);
        VerifyPasswordQueryHandler handler = CreateHandler();

        // Act
        ErrorOr<bool> result = await handler.Handle(new VerifyPasswordQuery(TestUserId, Password), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        await Handle_WhenUserNotFound_ShouldReturnNotFoundError();
    }

    [Fact]
    public async Task Handle_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(PasswordHash));
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _hashServiceMock.Setup(service => service.Verify(Password, PasswordHash)).Returns((ErrorOr<bool>)false);
        VerifyPasswordQueryHandler handler = CreateHandler();

        // Act
        ErrorOr<bool> result = await handler.Handle(new VerifyPasswordQuery(TestUserId, Password), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(Password, PasswordHash), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(PasswordHash));
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _hashServiceMock.Setup(service => service.Verify(Password, PasswordHash)).Returns((ErrorOr<bool>)true);
        VerifyPasswordQueryHandler handler = CreateHandler();

        // Act
        ErrorOr<bool> result = await handler.Handle(new VerifyPasswordQuery(TestUserId, Password), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(Password, PasswordHash), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    private VerifyPasswordQueryHandler CreateHandler() =>
        new(_identityRepositoryMock.Object, _hashServiceMock.Object, NullLogger<VerifyPasswordQueryHandler>.Instance);
}
