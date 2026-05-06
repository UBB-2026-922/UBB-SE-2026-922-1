// Copyright (c) UBB-922. All rights reserved.
// Licensed under the MIT license.

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="BillPaymentRepository" /> verifying bill payment
///     persistence and retrieval are handled correctly against the real database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class BillPaymentRepositoryTests : IAsyncLifetime
{
    private const decimal InitialAccountBalance = 5000m;
    private const decimal PaymentAmount = 150m;
    private const decimal PaymentFee = 0m;

    private readonly DatabaseFixture _fixture;
    private readonly Faker<User> _userFaker;
    private readonly Faker<Biller> _billerFaker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillPaymentRepositoryTests" /> class.
    /// </summary>
    public BillPaymentRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        _userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");

        _billerFaker = new Faker<Biller>()
            .RuleFor(biller => biller.Name, faker => faker.Company.CompanyName())
            .RuleFor(biller => biller.Category, _ => "Utilities")
            .RuleFor(biller => biller.IsActive, _ => true);
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
    ///     Verifies the AddPaymentAsync_WhenPaymentIsValid_PersistsPayment scenario.
    /// </summary>
    [Fact]
    public async Task AddPaymentAsync_WhenPaymentIsValid_PersistsPayment()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        Biller biller = SeedBiller(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        BillPayment payment = MakePayment(user.Id, account.Id, biller.Id);

        // Act
        await repository.AddPaymentAsync(payment);

        // Assert
        BillPayment? persisted = databaseContext.BillPayments.FirstOrDefault(
            billPayment => billPayment.ReceiptNumber == payment.ReceiptNumber);
        persisted.Should().NotBeNull();
        persisted!.Amount.Should().Be(PaymentAmount);
        persisted.UserId.Should().Be(user.Id);
    }

    /// <summary>
    ///     Verifies the GetUserPaymentHistoryAsync_WhenUserHasPayments_ReturnsAllPaymentsForThatUser scenario.
    /// </summary>
    [Fact]
    public async Task GetUserPaymentHistoryAsync_WhenUserHasPayments_ReturnsAllPaymentsForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        User otherUser = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        Account otherAccount = SeedAccount(databaseContext, otherUser.Id);
        Biller biller = SeedBiller(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        await repository.AddPaymentAsync(MakePayment(user.Id, account.Id, biller.Id));
        await repository.AddPaymentAsync(MakePayment(user.Id, account.Id, biller.Id));
        await repository.AddPaymentAsync(MakePayment(otherUser.Id, otherAccount.Id, biller.Id));

        // Act
        IEnumerable<BillPayment> result = await repository.GetUserPaymentHistoryAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(
            payment => payment.UserId.Should().Be(user.Id));
    }

    /// <summary>
    ///     Verifies the GetUserPaymentHistoryAsync_WhenUserHasNoPayments_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public async Task GetUserPaymentHistoryAsync_WhenUserHasNoPayments_ReturnsEmptyList()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        IEnumerable<BillPayment> result = await repository.GetUserPaymentHistoryAsync(user.Id);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetAccountByIdAsync_WhenAccountExists_ReturnsAccount scenario.
    /// </summary>
    [Fact]
    public async Task GetAccountByIdAsync_WhenAccountExists_ReturnsAccount()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        Account? result = await repository.GetAccountByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
        result.UserId.Should().Be(user.Id);
    }

    /// <summary>
    ///     Verifies the GetAccountByIdAsync_WhenAccountDoesNotExist_ReturnsNull scenario.
    /// </summary>
    [Fact]
    public async Task GetAccountByIdAsync_WhenAccountDoesNotExist_ReturnsNull()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        Account? result = await repository.GetAccountByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    ///     Verifies the GetAccountsByUserIdAsync_WhenUserHasAccounts_ReturnsAllAccounts scenario.
    /// </summary>
    [Fact]
    public async Task GetAccountsByUserIdAsync_WhenUserHasAccounts_ReturnsAllAccounts()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);
        SeedAccount(databaseContext, user.Id);
        SeedAccount(databaseContext, user.Id);

        // Act
        IEnumerable<Account> result = await repository.GetAccountsByUserIdAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(
            account => account.UserId.Should().Be(user.Id));
    }

    /// <summary>
    ///     Verifies the UpdateAccountAsync_WhenBalanceChanges_PersistsNewBalance scenario.
    /// </summary>
    [Fact]
    public async Task UpdateAccountAsync_WhenBalanceChanges_PersistsNewBalance()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Account account = SeedAccount(databaseContext, user.Id);
        BillPaymentRepository repository = MakeRepository(databaseContext);
        decimal newBalance = account.Balance - PaymentAmount;

        // Act
        account.Balance = newBalance;
        await repository.UpdateAccountAsync(account);

        // Assert
        Account? updated = databaseContext.Accounts.Find(account.Id);
        updated.Should().NotBeNull();
        updated!.Balance.Should().Be(newBalance);
    }

    /// <summary>
    ///     Verifies the AddSavedBillerAsync_WhenValid_PersistsSavedBiller scenario.
    /// </summary>
    [Fact]
    public async Task AddSavedBillerAsync_WhenValid_PersistsSavedBiller()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        Biller biller = SeedBiller(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        SavedBiller savedBiller = new SavedBiller
        {
            UserId = user.Id,
            BillerId = biller.Id,
            Nickname = "My electricity",
            DefaultReference = "REF-001",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        await repository.AddSavedBillerAsync(savedBiller);

        // Assert
        IEnumerable<SavedBiller> result = await repository.GetSavedBillersAsync(user.Id);
        result.Should().ContainSingle();
        result.First().Nickname.Should().Be("My electricity");
    }

    /// <summary>
    ///     Verifies the GetSavedBillersAsync_WhenUserHasNoSavedBillers_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public async Task GetSavedBillersAsync_WhenUserHasNoSavedBillers_ReturnsEmptyList()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        User user = SeedUser(databaseContext);
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        IEnumerable<SavedBiller> result = await repository.GetSavedBillersAsync(user.Id);

        // Assert
        result.Should().BeEmpty();
    }

    private static BillPaymentRepository MakeRepository(AppDatabaseContext databaseContext)
    {
        return new BillPaymentRepository(databaseContext);
    }

    private static BillPayment MakePayment(int userId, int accountId, int billerId)
    {
        return new BillPayment
        {
            UserId = userId,
            SourceAccountId = accountId,
            BillerId = billerId,
            BillerReference = $"REF-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            Amount = PaymentAmount,
            Fee = PaymentFee,
            ReceiptNumber = $"RCP-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            Status = BillPaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static Account SeedAccount(AppDatabaseContext databaseContext, int userId)
    {
        Account account = new Account
        {
            UserId = userId,
            AccountName = "Checking Account",
            Iban = $"RO{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            Currency = "RON",
            Balance = InitialAccountBalance,
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

    private Biller SeedBiller(AppDatabaseContext databaseContext)
    {
        Biller biller = _billerFaker.Generate();
        databaseContext.Billers.Add(biller);
        databaseContext.SaveChanges();
        return biller;
    }
}