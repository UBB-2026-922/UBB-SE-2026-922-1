namespace BankingApp.Application.Tests.Features.PasswordReset.Queries;

using System.Security.Cryptography;
using System.Text;
using BankingApp.Application.Features.PasswordReset.Queries;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Shared.Clock;

public sealed class VerifyResetTokenQueryTests
{
    private const string RawToken = "reset-token";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
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

        VerifyResetTokenQueryHandler handler = CreateHandler();
        var query = new VerifyResetTokenQuery(RawToken);

        // Act
        ErrorOr<Success> result = await handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenInvalid);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
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

        VerifyResetTokenQueryHandler handler = CreateHandler();
        var query = new VerifyResetTokenQuery(RawToken);

        // Act
        ErrorOr<Success> result = await handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenAlreadyUsed);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
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

        VerifyResetTokenQueryHandler handler = CreateHandler();
        var query = new VerifyResetTokenQuery(RawToken);

        // Act
        ErrorOr<Success> result = await handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PasswordResetErrors.TokenExpired);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTokenIsValid_ShouldReturnSuccess()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        string tokenHash = ComputeSha256Hash(RawToken);
        IdentityAccount identity = CreateIdentityWithToken(tokenHash, _testNow.AddMinutes(30));

        _identityRepositoryMock
            .Setup(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken))
            .ReturnsAsync(identity);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        VerifyResetTokenQueryHandler handler = CreateHandler();
        var query = new VerifyResetTokenQuery(RawToken);

        // Act
        ErrorOr<Success> result = await handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _identityRepositoryMock.Verify(repository => repository.GetByResetTokenHashAsync(tokenHash, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private VerifyResetTokenQueryHandler CreateHandler()
    {
        return new VerifyResetTokenQueryHandler(_identityRepositoryMock.Object, _clockMock.Object);
    }

    private static IdentityAccount CreateIdentityWithToken(string tokenHash, DateTime expiresAt)
    {
        var identity = IdentityAccount.Create(1, HashedPassword.Wrap("old-hash"));
        identity.IssuePasswordResetToken(tokenHash, expiresAt, _testNow);
        return identity;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
