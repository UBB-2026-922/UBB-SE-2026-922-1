namespace BankingApp.Infrastructure.Tests.Integration;

using Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;
using Infrastructure;
using Bogus;
using ErrorOr;
using Persistence;

/// <summary>
///     Initializes a new instance of the <see cref="UserRepositoryTests" /> class.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class UserRepositoryTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private const int NonExistentUserId = 99999;
    private const int ExpectedFailedAttemptCount = 2;
    private const int LockoutDurationMinutes = 30;

    private readonly DatabaseFixture _fixture = fixture;
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
        return new ValueTask(_fixture.ResetAsync());
    }

    /// <summary>
    ///     Disposes the test fixture.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void FindByEmail_AfterCreatingUser_ReturnsUserWithMatchingFields()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();

        // Act
        ErrorOr<User> result = userDataAccess.FindByEmail(user.Email);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Email.Should().Be(user.Email);
        result.Value.FullName.Should().Be(user.FullName);
        result.Value.PasswordHash.Should().Be(user.PasswordHash);
    }

    [Fact]
    public void FindById_WhenUserExists_ReturnsUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();
        ErrorOr<User> byEmail = userDataAccess.FindByEmail(user.Email);
        byEmail.IsError.Should().BeFalse(byEmail.IsError ? byEmail.FirstError.Description : string.Empty);

        // Act
        ErrorOr<User> result = userDataAccess.FindById(byEmail.Value.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().Be(byEmail.Value.Id);
        result.Value.Email.Should().Be(user.Email);
    }

    [Fact]
    public void FindById_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);

        // Act
        ErrorOr<User> result = userDataAccess.FindById(NonExistentUserId);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Update_WhenFieldsAreChanged_PersistsChanges()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();
        ErrorOr<User> savedUser = userDataAccess.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);
        User userToUpdate = savedUser.Value;
        userToUpdate.FullName = "Diana Updated";
        userToUpdate.PhoneNumber = "+40700000000";

        // Act
        ErrorOr<Success> updateResult = userDataAccess.Update(userToUpdate);

        // Assert
        updateResult.IsError.Should()
            .BeFalse(updateResult.IsError ? updateResult.FirstError.Description : string.Empty);
        ErrorOr<User> refreshed = userDataAccess.FindById(userToUpdate.Id);
        refreshed.IsError.Should().BeFalse(refreshed.IsError ? refreshed.FirstError.Description : string.Empty);
        refreshed.Value.FullName.Should().Be("Diana Updated");
        refreshed.Value.PhoneNumber.Should().Be("+40700000000");
    }

    [Fact]
    public void IncrementFailedAttempts_WhenCalledTwice_CounterIncreasesBy2()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();
        ErrorOr<User> savedUser = userDataAccess.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);

        // Act
        userDataAccess.IncrementFailedAttempts(savedUser.Value.Id);
        userDataAccess.IncrementFailedAttempts(savedUser.Value.Id);

        // Assert
        ErrorOr<User> updated = userDataAccess.FindById(savedUser.Value.Id);
        updated.IsError.Should().BeFalse(updated.IsError ? updated.FirstError.Description : string.Empty);
        updated.Value.FailedLoginAttempts.Should().Be(ExpectedFailedAttemptCount);
    }

    [Fact]
    public void LockAccount_WhenUserExists_SetsIsLockedTrue()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        User? user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();
        ErrorOr<User> savedUser = userDataAccess.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);
        DateTime lockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);

        // Act
        ErrorOr<Success> lockResult = userDataAccess.LockAccount(savedUser.Value.Id, lockoutEnd);

        // Assert
        lockResult.IsError.Should().BeFalse(lockResult.IsError ? lockResult.FirstError.Description : string.Empty);
        ErrorOr<User> locked = userDataAccess.FindById(savedUser.Value.Id);
        locked.IsError.Should().BeFalse(locked.IsError ? locked.FirstError.Description : string.Empty);
        locked.Value.IsLocked.Should().BeTrue();
    }
}