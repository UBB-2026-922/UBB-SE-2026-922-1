namespace BankingApp.Application.Tests.Features.PasswordReset.Commands;

using System.Security.Cryptography;
using System.Text;
using BankingApp.Application.Common.Security;
using BankingApp.Application.Features.PasswordReset.Commands;
using BankingApp.Domain.Common.Errors;
using Domain.Aggregates.IdentityAggregate.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class ResetPasswordCommandTests
{
    private const int TestUserId = 1;
    private const string RawToken = "reset-token";
    private const string NewPassword = "NewPassword123!";
    private const string NewPasswordHash = "new-password-hash";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHashService> _hashServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldReturnTokenInvalidError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);

        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync((IdentityAccount?)null);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenInvalid);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTokenAlreadyUsed_ShouldReturnTokenAlreadyUsedError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        identity.PasswordResetTokens.Single().MarkUsed(_testNow);

        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync(identity);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenAlreadyUsed);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ShouldReturnTokenExpiredError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(-1));

        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync(identity);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenExpired);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordHashingFails_ShouldReturnHashFailureError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        var hashError = Error.Failure("hash_failure", "Hash failed.");

        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync(identity);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _hashServiceMock
            .Setup(service => service.GetHash(NewPassword))
            .Returns((ErrorOr<string>)hashError);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(hashError);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(NewPassword), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdatePasswordHash()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        SetupValidFlow(identity, tokenHash, cancellationToken);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        identity.PasswordHash.Should().NotBeNull();
        identity.PasswordHash!.Value.Should().Be(NewPasswordHash);

        VerifyValidFlow(identity, tokenHash, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldMarkTokenAsUsed()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        PasswordResetToken resetToken = identity.PasswordResetTokens.Single();
        SetupValidFlow(identity, tokenHash, cancellationToken);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        resetToken.UsedAt.Should().Be(_testNow);

        VerifyValidFlow(identity, tokenHash, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldInvalidateAllSessions()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        Session session = identity.OpenSession("session-token", _testNow.AddDays(1), _testNow, "Desktop", "Edge", "127.0.0.1");
        SetupValidFlow(identity, tokenHash, cancellationToken);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        session.IsRevoked.Should().BeTrue();

        VerifyValidFlow(identity, tokenHash, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));
        SetupValidFlow(identity, tokenHash, cancellationToken);

        ResetPasswordCommandHandler handler = CreateHandler();
        var command = new ResetPasswordCommand(RawToken, NewPassword);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        VerifyValidFlow(identity, tokenHash, cancellationToken);
    }

    private ResetPasswordCommandHandler CreateHandler()
    {
        return new ResetPasswordCommandHandler(
            _identityRepositoryMock.Object,
            _hashServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<ResetPasswordCommandHandler>.Instance);
    }

    private void SetupValidFlow(IdentityAccount identity, string tokenHash, CancellationToken cancellationToken)
    {
        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync(identity);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _hashServiceMock
            .Setup(service => service.GetHash(NewPassword))
            .Returns((ErrorOr<string>)NewPasswordHash);

        _identityRepositoryMock
            .Setup(repository => repository.UpdateAsync(identity, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void VerifyValidFlow(IdentityAccount identity, string tokenHash, CancellationToken cancellationToken)
    {
        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(NewPassword), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static IdentityAccount CreateIdentityWithToken(string tokenHash, DateTime expiresAt)
    {
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap("old-hash"));
        identity.IssuePasswordResetToken(tokenHash, expiresAt, _testNow);
        return identity;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
