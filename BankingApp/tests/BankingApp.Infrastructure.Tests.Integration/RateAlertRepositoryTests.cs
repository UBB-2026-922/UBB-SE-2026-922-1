// Copyright (c) UBB-922. All rights reserved.
// Licensed under the MIT license.

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using ErrorOr;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="RateAlertRepository" /> verifying rate alert
///     persistence and lifecycle are handled correctly against the real database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class RateAlertRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly Faker<User> _userFaker;
    private readonly Faker<RateAlert> _alertFaker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertRepositoryTests" /> class.
    /// </summary>
    public RateAlertRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        _userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");

        _alertFaker = new Faker<RateAlert>()
            .RuleFor(alert => alert.BaseCurrency, _ => "EUR")
            .RuleFor(alert => alert.TargetCurrency, _ => "USD")
            .RuleFor(alert => alert.TargetRate, faker => Math.Round(faker.Random.Decimal(0.5m, 2.0m), 4))
            .RuleFor(alert => alert.IsBuyAlert, faker => faker.Random.Bool())
            .RuleFor(alert => alert.IsTriggered, _ => false)
            .RuleFor(alert => alert.CreatedAt, _ => DateTime.UtcNow);
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
    ///     Verifies the Create_WhenAlertIsValid_PersistsAndReturnsWithPositiveId scenario.
    /// </summary>
    [Fact]
    public void Create_WhenAlertIsValid_PersistsAndReturnsWithPositiveId()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);
        RateAlert alert = _alertFaker.Clone()
            .RuleFor(rateAlert => rateAlert.UserId, _ => user.Id)
            .Generate();

        // Act
        ErrorOr<RateAlert> result = repository.Create(alert);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.BaseCurrency.Should().Be(alert.BaseCurrency);
        result.Value.TargetCurrency.Should().Be(alert.TargetCurrency);
        result.Value.TargetRate.Should().Be(alert.TargetRate);
    }

    /// <summary>
    ///     Verifies the GetById_WhenAlertExists_ReturnsMatchingAlert scenario.
    /// </summary>
    [Fact]
    public void GetById_WhenAlertExists_ReturnsMatchingAlert()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);
        RateAlert alert = SeedAlert(databaseContext, user.Id);

        // Act
        ErrorOr<RateAlert> result = repository.GetById(alert.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().Be(alert.Id);
        result.Value.UserId.Should().Be(user.Id);
    }

    /// <summary>
    ///     Verifies the GetById_WhenAlertDoesNotExist_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void GetById_WhenAlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        RateAlertRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<RateAlert> result = repository.GetById(99999);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetByUserId_WhenUserHasAlerts_ReturnsAllAlertsForThatUser scenario.
    /// </summary>
    [Fact]
    public void GetByUserId_WhenUserHasAlerts_ReturnsAllAlertsForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        User otherUser = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);

        SeedAlert(databaseContext, user.Id);
        SeedAlert(databaseContext, user.Id);
        SeedAlert(databaseContext, otherUser.Id);

        // Act
        ErrorOr<List<RateAlert>> result = repository.GetByUserId(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(alert => alert.UserId.Should().Be(user.Id));
    }

    /// <summary>
    ///     Verifies the GetByUserId_WhenUserHasNoAlerts_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public void GetByUserId_WhenUserHasNoAlerts_ReturnsEmptyList()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<List<RateAlert>> result = repository.GetByUserId(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetUntriggeredAlerts_WhenMixedAlertsExist_ReturnsOnlyUntriggered scenario.
    /// </summary>
    [Fact]
    public void GetUntriggeredAlerts_WhenMixedAlertsExist_ReturnsOnlyUntriggered()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);

        RateAlert untriggeredAlert = SeedAlert(databaseContext, user.Id);
        RateAlert triggeredAlert = SeedAlert(databaseContext, user.Id);
        repository.MarkTriggered(triggeredAlert.Id);

        // Act
        ErrorOr<List<RateAlert>> result = repository.GetUntriggeredAlerts();

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().Contain(alert => alert.Id == untriggeredAlert.Id);
        result.Value.Should().NotContain(alert => alert.Id == triggeredAlert.Id);
    }

    /// <summary>
    ///     Verifies the MarkTriggered_WhenAlertExists_SetsIsTriggeredTrue scenario.
    /// </summary>
    [Fact]
    public void MarkTriggered_WhenAlertExists_SetsIsTriggeredTrue()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);
        RateAlert alert = SeedAlert(databaseContext, user.Id);

        // Act
        ErrorOr<RateAlert> result = repository.MarkTriggered(alert.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.IsTriggered.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the MarkTriggered_WhenAlertDoesNotExist_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void MarkTriggered_WhenAlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        RateAlertRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<RateAlert> result = repository.MarkTriggered(99999);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the Delete_WhenAlertExists_RemovesItFromDatabase scenario.
    /// </summary>
    [Fact]
    public void Delete_WhenAlertExists_RemovesItFromDatabase()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        RateAlertRepository repository = MakeRepository(databaseContext);
        RateAlert alert = SeedAlert(databaseContext, user.Id);

        // Act
        ErrorOr<Success> deleteResult = repository.Delete(alert.Id);

        // Assert
        deleteResult.IsError.Should().BeFalse(deleteResult.IsError ? deleteResult.FirstError.Description : string.Empty);
        repository.GetById(alert.Id).IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the Delete_WhenAlertDoesNotExist_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void Delete_WhenAlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        RateAlertRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<Success> result = repository.Delete(99999);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    private static RateAlertRepository MakeRepository(AppDatabaseContext databaseContext)
    {
        return new RateAlertRepository(databaseContext);
    }

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        User user = _userFaker.Generate();
        databaseContext.Users.Add(user);
        databaseContext.SaveChanges();
        return user;
    }

    private RateAlert SeedAlert(AppDatabaseContext databaseContext, int userId)
    {
        RateAlert alert = _alertFaker.Clone()
            .RuleFor(rateAlert => rateAlert.UserId, _ => userId)
            .Generate();
        databaseContext.RateAlerts.Add(alert);
        databaseContext.SaveChanges();
        return alert;
    }
}