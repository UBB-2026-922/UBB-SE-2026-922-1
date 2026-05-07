namespace BankingApp.Infrastructure.Tests.Integration;

using Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;

using Infrastructure;
using Bogus;
using ErrorOr;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class AuthenticationRepositoryTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private const int PasswordResetTokenExpiryHours = 1;

    private readonly Faker<User> _userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");

    /// <summary>
    ///     Initializes the test fixture.
    /// </summary>
    public ValueTask InitializeAsync()
    {
        return new ValueTask(fixture.ResetAsync());
    }

    /// <summary>
    ///     Disposes the test fixture.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void CreateUser_WhenCalled_ShouldAutomaticallyCreatesNotificationPreferences()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);
        User? newUser = _userFaker.Generate();

        // Act
        ErrorOr<Success> result = repository.CreateUser(newUser);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        User user = userDataAccess.FindByEmail(newUser.Email).Value;
        int count = databaseContext.NotificationPreferences.Count(preference => preference.UserId == user.Id);
        count.Should().BeGreaterThan(0, "Expected at least one notification preference to be created.");
    }

    [Fact]
    public void CreateSession_WhenUserExists_ShouldReturnSessionWithPositiveId()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);

        // Act
        ErrorOr<Session> result = repository.CreateSession(user.Id, "token-abc", "Chrome", "Chrome 120", "127.0.0.1");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Token.Should().Be("token-abc");
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void FindSessionByToken_WhenTokenIsActive_ShouldReturnSession()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);
        repository.CreateSession(user.Id, "valid-token-123", null, null, null);

        // Act
        ErrorOr<Session> result = repository.FindSessionByToken("valid-token-123");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Token.Should().Be("valid-token-123");
    }

    [Fact]
    public void FindSessionByToken_WhenTokenIsRevoked_ShouldReturnNotFound()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);
        ErrorOr<Session> sessionResult = repository.CreateSession(user.Id, "revoke-me", null, null, null);
        sessionResult.IsError.Should().BeFalse();
        repository.UpdateSessionToken(sessionResult.Value.Id);

        // Act
        ErrorOr<Session> result = repository.FindSessionByToken("revoke-me");

        // Assert
        result.IsError.Should().BeTrue("A revoked session should not be retrievable.");
    }

    [Fact]
    public void FindPasswordResetToken_WhenSavingToken_ShouldReturnPersistedToken()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "sha256-hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenExpiryHours)
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

    [Fact]
    public void MarkPasswordResetTokenAsUsed_WhenTokenExists_ShouldSetUsedAtTimestamp()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        AuthenticationRepository repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "mark-used-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenExpiryHours)
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
        return fixture.CreateDatabaseContext();
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

    private static AuthenticationRepository MakeAuthenticationRepository(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        var sessionDataAccess = new SessionDataAccess(databaseContext);
        var passwordResetTokenDataAccess = new PasswordResetTokenDataAccess(databaseContext);
        var notificationPreferenceDataAccess = new NotificationPreferenceDataAccess(databaseContext);
        return new AuthenticationRepository(
            userDataAccess,
            sessionDataAccess,
            passwordResetTokenDataAccess,
            notificationPreferenceDataAccess);
    }
}
