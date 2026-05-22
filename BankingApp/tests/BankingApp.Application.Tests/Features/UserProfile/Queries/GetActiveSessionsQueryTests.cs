namespace BankingApp.Application.Tests.Features.UserProfile.Queries;

using BankingApp.Application.Features.UserProfile.Queries;
using BankingApp.Contracts.Features.UserProfile.Dtos;
using BankingApp.Domain.Aggregates.IdentityAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class GetActiveSessionsQueryTests
{
    private const int TestUserId = 42;

    private static readonly DateTime TestNow = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = MockFactory.CreateIdentityRepositoryMock();

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        GetActiveSessionsQueryHandler handler = CreateHandler();
        GetActiveSessionsQuery query = new(TestUserId);

        // Act
        ErrorOr<List<SessionDto>> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityHasMixedSessions_ShouldReturnOnlyActiveSessions()
    {
        // Arrange
        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        identity.OpenSession("active-token", TestNow.AddHours(24), TestNow, "Windows", "Chrome", "1.2.3.4");
        Session revokedSession = identity.OpenSession("revoked-token", TestNow.AddHours(24), TestNow, "Mac", "Safari", "5.6.7.8");
        revokedSession.Revoke();

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        GetActiveSessionsQueryHandler handler = CreateHandler();
        GetActiveSessionsQuery query = new(TestUserId);

        // Act
        ErrorOr<List<SessionDto>> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(sessionDto => sessionDto.Browser == "Chrome");
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSessionExists_ShouldMapAllFieldsIncludingCreatedAt()
    {
        // Arrange
        DateTime sessionCreatedAt = new(2026, 4, 15, 9, 0, 0, DateTimeKind.Utc);
        const string deviceInfo = "Desktop";
        const string browser = "Firefox";
        const string ipAddress = "192.168.1.1";

        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        identity.OpenSession("token-abc", TestNow.AddHours(24), sessionCreatedAt, deviceInfo, browser, ipAddress);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        GetActiveSessionsQueryHandler handler = CreateHandler();
        GetActiveSessionsQuery query = new(TestUserId);

        // Act
        ErrorOr<List<SessionDto>> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        SessionDto sessionDto = result.Value.Should().ContainSingle().Subject;
        sessionDto.DeviceInfo.Should().Be(deviceInfo);
        sessionDto.Browser.Should().Be(browser);
        sessionDto.IpAddress.Should().Be(ipAddress);
        sessionDto.CreatedAt.Should().Be(sessionCreatedAt);
        sessionDto.LastActiveAt.Should().BeNull();
    }

    private GetActiveSessionsQueryHandler CreateHandler()
    {
        return new GetActiveSessionsQueryHandler(
            _identityRepositoryMock.Object,
            NullLogger<GetActiveSessionsQueryHandler>.Instance);
    }
}
