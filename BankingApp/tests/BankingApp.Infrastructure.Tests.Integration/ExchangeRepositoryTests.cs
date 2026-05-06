// Copyright (c) UBB-922. All rights reserved.
// Licensed under the MIT license.

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using ErrorOr;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="ExchangeRepository" /> verifying exchange transaction
///     persistence and lifecycle are handled correctly against the real database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class ExchangeRepositoryTests : IAsyncLifetime
{
    private const decimal InitialAccountBalance = 10000m;
    private const decimal SourceAmount = 100m;
    private const decimal TargetAmount = 114.50m;
    private const decimal ExchangeRate = 1.15m;
    private const decimal Commission = 0.50m;

    private readonly DatabaseFixture _fixture;
    private readonly Faker<User> _userFaker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeRepositoryTests" /> class.
    /// </summary>
    public ExchangeRepositoryTests(DatabaseFixture fixture)
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
    ///     Verifies the Create_WhenAccountsHaveSufficientFunds_PersistsExchangeAndUpdatesBalances scenario.
    /// </summary>
    [Fact]
    public void Create_WhenAccountsHaveSufficientFunds_PersistsExchangeAndUpdatesBalances()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", InitialAccountBalance);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction exchange = MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.Create(exchange);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Status.Should().Be(ExchangeTransactionStatus.Completed);
        Account updatedSource = databaseContext.Accounts.Find(sourceAccount.Id) !;
        Account updatedTarget = databaseContext.Accounts.Find(targetAccount.Id) !;
        updatedSource.Balance.Should().Be(InitialAccountBalance - SourceAmount);
        updatedTarget.Balance.Should().Be(InitialAccountBalance + TargetAmount);
    }

    /// <summary>
    ///     Verifies the Create_WhenSourceAccountHasInsufficientFunds_ReturnsForbiddenError scenario.
    /// </summary>
    [Fact]
    public void Create_WhenSourceAccountHasInsufficientFunds_ReturnsForbiddenError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", 10m);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction exchange = MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.Create(exchange);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    /// <summary>
    ///     Verifies the Create_WhenSourceAccountDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void Create_WhenSourceAccountDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction exchange = MakeExchange(user.Id, 99999, targetAccount.Id);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.Create(exchange);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the Create_WhenTargetAccountDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void Create_WhenTargetAccountDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction exchange = MakeExchange(user.Id, sourceAccount.Id, 99999);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.Create(exchange);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetById_WhenExchangeExists_ReturnsMatchingExchange scenario.
    /// </summary>
    [Fact]
    public void GetById_WhenExchangeExists_ReturnsMatchingExchange()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", InitialAccountBalance);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction created = repository.Create(
            MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id)).Value;

        // Act
        ErrorOr<ExchangeTransaction> result = repository.GetById(created.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().Be(created.Id);
        result.Value.UserId.Should().Be(user.Id);
    }

    /// <summary>
    ///     Verifies the GetById_WhenExchangeDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetById_WhenExchangeDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        ExchangeRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.GetById(99999);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetByUserId_WhenUserHasExchanges_ReturnsAllExchangesForThatUser scenario.
    /// </summary>
    [Fact]
    public void GetByUserId_WhenUserHasExchanges_ReturnsAllExchangesForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        User otherUser = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", InitialAccountBalance);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        Account otherSourceAccount = SeedAccount(databaseContext, otherUser.Id, "EUR", InitialAccountBalance);
        Account otherTargetAccount = SeedAccount(databaseContext, otherUser.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        repository.Create(MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id));
        repository.Create(MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id));
        repository.Create(MakeExchange(otherUser.Id, otherSourceAccount.Id, otherTargetAccount.Id));

        // Act
        ErrorOr<List<ExchangeTransaction>> result = repository.GetByUserId(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(
            exchange => exchange.UserId.Should().Be(user.Id));
    }

    /// <summary>
    ///     Verifies the GetByUserId_WhenUserHasNoExchanges_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public void GetByUserId_WhenUserHasNoExchanges_ReturnsEmptyList()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        ExchangeRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<List<ExchangeTransaction>> result = repository.GetByUserId(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the UpdateStatus_WhenExchangeExists_PersistsNewStatus scenario.
    /// </summary>
    [Fact]
    public void UpdateStatus_WhenExchangeExists_PersistsNewStatus()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account sourceAccount = SeedAccount(databaseContext, user.Id, "EUR", InitialAccountBalance);
        Account targetAccount = SeedAccount(databaseContext, user.Id, "USD", InitialAccountBalance);
        ExchangeRepository repository = MakeRepository(databaseContext);
        ExchangeTransaction created = repository.Create(
            MakeExchange(user.Id, sourceAccount.Id, targetAccount.Id)).Value;

        // Act
        ErrorOr<ExchangeTransaction> result = repository.UpdateStatus(created.Id, ExchangeTransactionStatus.Failed);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Status.Should().Be(ExchangeTransactionStatus.Failed);
    }

    /// <summary>
    ///     Verifies the UpdateStatus_WhenExchangeDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void UpdateStatus_WhenExchangeDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        ExchangeRepository repository = MakeRepository(databaseContext);

        // Act
        ErrorOr<ExchangeTransaction> result = repository.UpdateStatus(99999, ExchangeTransactionStatus.Failed);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    private static ExchangeRepository MakeRepository(AppDatabaseContext databaseContext)
    {
        return new ExchangeRepository(databaseContext);
    }

    private static ExchangeTransaction MakeExchange(int userId, int sourceAccountId, int targetAccountId)
    {
        return new ExchangeTransaction
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            SourceCurrency = "EUR",
            TargetCurrency = "USD",
            SourceAmount = SourceAmount,
            TargetAmount = TargetAmount,
            ExchangeRate = ExchangeRate,
            Commission = Commission,
            RateLockedAt = DateTime.UtcNow,
            Status = ExchangeTransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static Account SeedAccount(
        AppDatabaseContext databaseContext,
        int userId,
        string currency,
        decimal balance)
    {
        Account account = new Account
        {
            UserId = userId,
            AccountName = $"{currency} Account",
            Iban = $"RO{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            Currency = currency,
            Balance = balance,
            AccountType = AccountType.Checking,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };
        databaseContext.Accounts.Add(account);
        databaseContext.SaveChanges();
        return account;
    }

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        User user = _userFaker.Generate();
        databaseContext.Users.Add(user);
        databaseContext.SaveChanges();
        return user;
    }
}