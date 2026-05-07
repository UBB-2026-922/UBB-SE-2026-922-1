namespace BankingApp.Infrastructure.Tests.Integration;

using Domain.Entities;
using Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;

using Infrastructure;
using Bogus;
using ErrorOr;
using Features.AccountOverview;
using Persistence;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class AccountOverviewRepositoryTests : IAsyncLifetime
{
    private const int SeedTransactionCount = 5;
    private const int TransactionQueryLimit = 3;
    private const int SeedNotificationCount = 4;

    private readonly DatabaseFixture _fixture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountOverviewRepositoryTests" /> class.
    /// </summary>
    /// <param name="fixture">Database fixture.</param>
    public AccountOverviewRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public ValueTask InitializeAsync()
    {
        return new ValueTask(_fixture.ResetAsync());
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void GetAccountsByUser_WhenUserHasAccounts_ReturnsAllAccounts()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        SeedAccount(databaseContext, user.Id);
        AccountOverviewRepository repository = MakeAccountOverviewRepository(databaseContext);

        // Act
        ErrorOr<List<Account>> result = repository.GetAccountsByUser(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value.First().UserId.Should().Be(user.Id);
        result.Value.First().Currency.Should().Be("RON");
    }

    [Fact]
    public void GetRecentTransactions_WhenInsertedMoreThanLimit_ShouldReturnAtMostLimitItems()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        SeedTransactions(databaseContext, account.Id, SeedTransactionCount);
        AccountOverviewRepository repository = MakeAccountOverviewRepository(databaseContext);

        // Act
        ErrorOr<List<Transaction>> result = repository.GetRecentTransactions(account.Id, TransactionQueryLimit);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(TransactionQueryLimit);
    }

    [Fact]
    public void GetUnreadNotificationCount_WhenNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        SeedNotifications(databaseContext, user.Id, SeedNotificationCount);
        AccountOverviewRepository repository = MakeAccountOverviewRepository(databaseContext);

        // Act
        ErrorOr<int> result = repository.GetUnreadNotificationCount(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().Be(SeedNotificationCount);
    }

    [Fact]
    public void GetCardsByUser_WhenUserHasCards_ReturnsCardsForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = MakeDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        SeedCard(databaseContext, account.Id, user.Id);
        AccountOverviewRepository repository = MakeAccountOverviewRepository(databaseContext);

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

    private static User SeedUser(AppDatabaseContext databaseContext)
    {
        var faker = new Faker();
        var dataAccess = new UserDataAccess(databaseContext);
        var user = new User
        {
            Email = faker.Internet.Email(),
            PasswordHash = faker.Internet.Password(),
            FullName = faker.Person.FullName,
            PreferredLanguage = "en"
        };

        dataAccess.Create(user).IsError.Should().BeFalse();

        ErrorOr<User> findResult = dataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    private static Account SeedAccount(AppDatabaseContext databaseContext, int userId, string? iban = null)
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
            Status = AccountStatus.Active
        };
        databaseContext.Accounts.Add(account);
        databaseContext.SaveChanges();
        return account;
    }

    private static void SeedCard(AppDatabaseContext databaseContext, int accountId, int userId)
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
            Status = CardStatus.Active
        };
        databaseContext.Cards.Add(card);
        databaseContext.SaveChanges();
    }

    private static void SeedTransactions(AppDatabaseContext databaseContext, int accountId, int count)
    {
        for (int index = 0; index < count; index++)
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
                Status = TransactionStatus.Completed
            };
            databaseContext.Transactions.Add(transaction);
        }

        databaseContext.SaveChanges();
    }

    private static void SeedNotifications(AppDatabaseContext databaseContext, int userId, int count)
    {
        for (int index = 0; index < count; index++)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Info",
                Message = "You have a new notification.",
                Type = "Alert",
                Channel = "Push",
                IsRead = false
            };
            databaseContext.Notifications.Add(notification);
        }

        databaseContext.SaveChanges();
    }

    private static AccountOverviewRepository MakeAccountOverviewRepository(AppDatabaseContext databaseContext)
    {
        return new AccountOverviewRepository(
            new AccountDataAccess(databaseContext),
            new CardDataAccess(databaseContext),
            new TransactionDataAccess(databaseContext),
            new NotificationDataAccess(databaseContext),
            new TransferDataAccess(databaseContext));
    }
}
