// <copyright file="DashboardRepositoryTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using ErrorOr;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="DashboardRepository" /> verifying that
///     aggregate and collection queries return correct, database-backed results.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class DashboardRepositoryTests : IAsyncLifetime
{
    private const int SeedTransactionCount = 5;
    private const int TransactionQueryLimit = 3;
    private const int SeedNotificationCount = 4;

    private readonly DatabaseFixture _fixture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardRepositoryTests" /> class.
    /// </summary>
    /// <param name="fixture">Database fixture.</param>
    public DashboardRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return _fixture.ResetAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Verifies the GetAccountsByUser_WhenUserHasAccounts_ReturnsAllAccounts scenario.
    /// </summary>
    [Fact]
    public void GetAccountsByUser_WhenUserHasAccounts_ReturnsAllAccounts()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        SeedAccount(databaseContext, user.Id);
        DashboardRepository repository = MakeDashboardRepository(databaseContext);

        // Act
        ErrorOr<List<Account>> result = repository.GetAccountsByUser(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value.First().UserId.Should().Be(user.Id);
        result.Value.First().Currency.Should().Be("RON");
    }

    /// <summary>
    ///     Verifies the GetRecentTransactions_WhenInsertedMoreThanLimit_ReturnsAtMostLimitItems scenario.
    /// </summary>
    [Fact]
    public void GetRecentTransactions_WhenInsertedMoreThanLimit_ReturnsAtMostLimitItems()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        SeedTransactions(databaseContext, account.Id, SeedTransactionCount);
        DashboardRepository repository = MakeDashboardRepository(databaseContext);

        // Act
        ErrorOr<List<Transaction>> result = repository.GetRecentTransactions(account.Id, TransactionQueryLimit);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(TransactionQueryLimit);
    }

    /// <summary>
    ///     Verifies the GetUnreadNotificationCount_WhenNotificationsExist_ReturnsCorrectCount scenario.
    /// </summary>
    [Fact]
    public void GetUnreadNotificationCount_WhenNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        SeedNotifications(databaseContext, user.Id, SeedNotificationCount);
        DashboardRepository repository = MakeDashboardRepository(databaseContext);

        // Act
        ErrorOr<int> result = repository.GetUnreadNotificationCount(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().Be(SeedNotificationCount);
    }

    /// <summary>
    ///     Verifies the GetCardsByUser_WhenUserHasCards_ReturnsCardsForThatUser scenario.
    /// </summary>
    [Fact]
    public void GetCardsByUser_WhenUserHasCards_ReturnsCardsForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        SeedCard(databaseContext, account.Id, user.Id);
        DashboardRepository repository = MakeDashboardRepository(databaseContext);

        // Act
        ErrorOr<List<Card>> result = repository.GetCardsByUser(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value.First().UserId.Should().Be(user.Id);
        result.Value.First().CardType.Should().Be(CardType.Debit);
    }

    private AppDatabaseContext MakeDatabaseContext()
    {
        return _fixture.CreateDatabaseContext();
    }

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        var faker = new Faker();
        var dataAccess = new UserDataAccess(databaseContext);
        var user = new User
        {
            Email = faker.Internet.Email(),
            PasswordHash = faker.Internet.Password(),
            FullName = faker.Person.FullName,
            PreferredLanguage = "en",
        };

        dataAccess.Create(user).IsError.Should().BeFalse();

        ErrorOr<User> findResult = dataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    private Account SeedAccount(AppDatabaseContext databaseContext, int userId, string? iban = null)
    {
        var faker = new Faker();
        iban ??= faker.Finance.Iban();
        var account = new Account
        {
            UserId = userId,
            AccountName = "Main Account",
            Iban = iban,
            Currency = "RON",
            Balance = 5000.00m,
            AccountType = AccountType.Checking,
            Status = AccountStatus.Active,
        };
        databaseContext.Accounts.Add(account);
        databaseContext.SaveChanges();
        return account;
    }

    private void SeedCard(AppDatabaseContext databaseContext, int accountId, int userId)
    {
        var card = new Card
        {
            AccountId = accountId,
            UserId = userId,
            CardNumber = "4111111111111111",
            CardholderName = "Test User",
            ExpiryDate = new DateTime(2027, 12, 31),
            Cvv = "123",
            CardType = CardType.Debit,
            Status = CardStatus.Active,
        };
        databaseContext.Cards.Add(card);
        databaseContext.SaveChanges();
    }

    private void SeedTransactions(AppDatabaseContext databaseContext, int accountId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            var transaction = new Transaction
            {
                AccountId = accountId,
                TransactionRef = $"REF-{index}-{Guid.NewGuid():N}",
                Type = "Transfer",
                RelatedEntityType = "Transfer",
                Direction = TransactionDirection.In,
                Amount = 100.00m,
                Currency = "RON",
                BalanceAfter = 5100.00m,
                Status = TransactionStatus.Completed,
            };
            databaseContext.Transactions.Add(transaction);
        }

        databaseContext.SaveChanges();
    }

    private void SeedNotifications(AppDatabaseContext databaseContext, int userId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Info",
                Message = "You have a new notification.",
                Type = "Alert",
                Channel = "Push",
                IsRead = false,
            };
            databaseContext.Notifications.Add(notification);
        }

        databaseContext.SaveChanges();
    }

    private DashboardRepository MakeDashboardRepository(AppDatabaseContext databaseContext)
    {
        return new DashboardRepository(
            new AccountDataAccess(databaseContext),
            new CardDataAccess(databaseContext),
            new TransactionDataAccess(databaseContext),
            new NotificationDataAccess(databaseContext));
    }
}
