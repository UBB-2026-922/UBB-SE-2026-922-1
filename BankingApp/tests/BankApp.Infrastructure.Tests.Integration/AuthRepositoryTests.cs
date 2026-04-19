// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess;
using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.Repositories.Implementations;
using BankApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="AuthRepository" /> verifying session management
///     and password-reset token lifecycle are persisted correctly.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class AuthRepositoryTests : IAsyncLifetime
{
    private const int PasswordResetTokenExpiryHours = 1;

    private readonly DatabaseFixture _fixture;
    private readonly Faker<User> _userFaker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthRepositoryTests" /> class.
    /// </summary>
    public AuthRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        _userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");
    }

    /// <summary>
    ///     Initializes the test fixture.
    /// </summary>
    public Task InitializeAsync()
    {
        return _fixture.ResetAsync();
    }

    /// <summary>
    ///     Disposes the test fixture.
    /// </summary>
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Verifies the CreateUser_WhenCalled_AutomaticallyCreatesNotificationPreferences scenario.
    /// </summary>
    [Fact]
    public void CreateUser_WhenCalled_AutomaticallyCreatesNotificationPreferences()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);
        User? newUser = _userFaker.Generate();

        // Act
        ErrorOr<Success> result = repository.CreateUser(newUser);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        User user = userDataAccess.FindByEmail(newUser.Email).Value;
        ErrorOr<int> countResult = databaseContext.Query(connection =>
            connection.QueryFirst<int>(
                "SELECT COUNT(*) FROM NotificationPreference WHERE UserId = @UserId",
                new { UserId = user.Id }));
        countResult.IsError.Should().BeFalse();
        countResult.Value.Should().BeGreaterThan(0, "Expected at least one notification preference to be created.");
    }

    /// <summary>
    ///     Verifies the CreateSession_WhenUserExists_ReturnsSessionWithPositiveId scenario.
    /// </summary>
    [Fact]
    public void CreateSession_WhenUserExists_ReturnsSessionWithPositiveId()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);

        // Act
        ErrorOr<Session> result = repository.CreateSession(user.Id, "token-abc", "Chrome", "Chrome 120", "127.0.0.1");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Token.Should().Be("token-abc");
        result.Value.UserId.Should().Be(user.Id);
    }

    /// <summary>
    ///     Verifies the FindSessionByToken_WhenTokenIsActive_ReturnsSession scenario.
    /// </summary>
    [Fact]
    public void FindSessionByToken_WhenTokenIsActive_ReturnsSession()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);
        repository.CreateSession(user.Id, "valid-token-123", null, null, null);

        // Act
        ErrorOr<Session> result = repository.FindSessionByToken("valid-token-123");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Token.Should().Be("valid-token-123");
    }

    /// <summary>
    ///     Verifies the FindSessionByToken_WhenTokenIsRevoked_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void FindSessionByToken_WhenTokenIsRevoked_ReturnsNotFound()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);
        ErrorOr<Session> sessionResult = repository.CreateSession(user.Id, "revoke-me", null, null, null);
        sessionResult.IsError.Should().BeFalse();
        repository.UpdateSessionToken(sessionResult.Value.Id);

        // Act
        ErrorOr<Session> result = repository.FindSessionByToken("revoke-me");

        // Assert
        result.IsError.Should().BeTrue("A revoked session should not be retrievable.");
    }

    /// <summary>
    ///     Verifies the FindPasswordResetToken_AfterSavingToken_ReturnsPersistedToken scenario.
    /// </summary>
    [Fact]
    public void FindPasswordResetToken_AfterSavingToken_ReturnsPersistedToken()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "sha256-hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenExpiryHours),
        };
        repository.SavePasswordResetToken(token).IsError.Should().BeFalse();

        // Act
        ErrorOr<PasswordResetToken> result = repository.FindPasswordResetToken("sha256-hash-xyz");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TokenHash.Should().Be("sha256-hash-xyz");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.UsedAt.Should().BeNull();
    }

    /// <summary>
    ///     Verifies the MarkPasswordResetTokenAsUsed_WhenTokenExists_SetsUsedAtTimestamp scenario.
    /// </summary>
    [Fact]
    public void MarkPasswordResetTokenAsUsed_WhenTokenExists_SetsUsedAtTimestamp()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthRepository repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "mark-used-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenExpiryHours),
        };
        repository.SavePasswordResetToken(token).IsError.Should().BeFalse();
        ErrorOr<PasswordResetToken> created = repository.FindPasswordResetToken("mark-used-hash");
        created.IsError.Should().BeFalse();

        // Act
        ErrorOr<Success> markResult = repository.MarkPasswordResetTokenAsUsed(created.Value.Id);

        // Assert
        markResult.IsError.Should().BeFalse(markResult.IsError ? markResult.FirstError.Description : string.Empty);
        ErrorOr<PasswordResetToken> afterMark = repository.FindPasswordResetToken("mark-used-hash");
        afterMark.IsError.Should().BeFalse();
        afterMark.Value.UsedAt.Should().NotBeNull();
    }

    private AppDatabaseContext MakeDatabaseContext()
    {
        return _fixture.CreateDatabaseContext();
    }

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();

        ErrorOr<User> findResult = userDataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    private AuthRepository MakeAuthenticationRepository(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        var sessionDataAccess = new SessionDataAccess(databaseContext);
        var oauthLinkDataAccess = new OAuthLinkDataAccess(databaseContext);
        var passwordResetTokenDataAccess = new PasswordResetTokenDataAccess(databaseContext);
        var notificationPreferenceDataAccess = new NotificationPreferenceDataAccess(databaseContext);
        return new AuthRepository(
            userDataAccess,
            sessionDataAccess,
            oauthLinkDataAccess,
            passwordResetTokenDataAccess,
            notificationPreferenceDataAccess);
    }
}