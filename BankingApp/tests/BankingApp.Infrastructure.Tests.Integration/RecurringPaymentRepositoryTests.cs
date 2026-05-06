// <copyright file="RecurringPaymentRepositoryTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains integration tests for RecurringPaymentRepository.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using ErrorOr;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="RecurringPaymentRepository" /> against a real database.
/// </summary>
[Collection("Integration")]
public class RecurringPaymentRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentRepositoryTests" /> class.
    /// </summary>
    /// <param name="fixture">The shared database fixture.</param>
    public RecurringPaymentRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public async Task InitializeAsync() => await _fixture.ResetAsync();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Create_WhenPaymentIsValid_ReturnsPersistedPaymentWithId()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment payment = BuildPayment(userId, billerId, accountId);

        ErrorOr<RecurringPayment> result = repository.Create(payment);

        Assert.False(result.IsError);
        Assert.True(result.Value.Id > 0);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(billerId, result.Value.BillerId);
        Assert.Equal(accountId, result.Value.SourceAccountId);
        Assert.Equal(payment.Amount, result.Value.Amount);
        Assert.Equal(RecurringPaymentStatus.Active, result.Value.Status);
    }

    [Fact]
    public void Create_WhenIsPayInFullIsTrue_PersistsFlag()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment payment = BuildPayment(userId, billerId, accountId, isPayInFull: true);

        ErrorOr<RecurringPayment> result = repository.Create(payment);

        Assert.False(result.IsError);
        Assert.True(result.Value.IsPayInFull);
    }

    [Fact]
    public void Create_WhenEndDateIsProvided_PersistsEndDate()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        DateTime expectedEndDate = DateTime.UtcNow.AddYears(1).Date;
        RecurringPayment payment = BuildPayment(userId, billerId, accountId, endDate: expectedEndDate);

        ErrorOr<RecurringPayment> result = repository.Create(payment);

        Assert.False(result.IsError);
        Assert.NotNull(result.Value.EndDate);
        Assert.Equal(expectedEndDate, result.Value.EndDate!.Value.Date);
    }

    [Fact]
    public void GetById_WhenPaymentExists_ReturnsCorrectPayment()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment created = repository.Create(BuildPayment(userId, billerId, accountId)).Value;

        ErrorOr<RecurringPayment> result = repository.GetById(created.Id);

        Assert.False(result.IsError);
        Assert.Equal(created.Id, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
    }

    [Fact]
    public void GetById_WhenPaymentDoesNotExist_ReturnsError()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);

        ErrorOr<RecurringPayment> result = repository.GetById(int.MaxValue);

        Assert.True(result.IsError);
    }

    [Fact]
    public void GetByUserId_WhenUserHasPayments_ReturnsAllUserPayments()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        repository.Create(BuildPayment(userId, billerId, accountId, amount: 50m));
        repository.Create(BuildPayment(userId, billerId, accountId, amount: 100m));

        ErrorOr<List<RecurringPayment>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.True(result.Value.Count >= 2);
        Assert.All(result.Value, p => Assert.Equal(userId, p.UserId));
    }

    [Fact]
    public void GetByUserId_WhenOtherUsersHavePayments_ReturnsOnlyRequestedUsersPayments()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        (int otherUserId, int otherBillerId, int otherAccountId) = SeedDependencies(context);
        repository.Create(BuildPayment(userId, billerId, accountId));
        repository.Create(BuildPayment(otherUserId, otherBillerId, otherAccountId));

        ErrorOr<List<RecurringPayment>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.DoesNotContain(result.Value, p => p.UserId == otherUserId);
    }

    [Fact]
    public void GetByUserId_WhenUserHasNoPayments_ReturnsEmptyList()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, _, _) = SeedDependencies(context);

        ErrorOr<List<RecurringPayment>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void GetDuePayments_WhenPaymentIsOverdue_ReturnsPayment()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        repository.Create(BuildPayment(userId, billerId, accountId, nextExecutionDate: DateTime.UtcNow.AddDays(-1)));

        ErrorOr<List<RecurringPayment>> result = repository.GetDuePayments(DateTime.UtcNow);

        Assert.False(result.IsError);
        Assert.Contains(result.Value, p => p.UserId == userId && p.BillerId == billerId);
    }

    [Fact]
    public void GetDuePayments_WhenPaymentIsScheduledInFuture_DoesNotReturnPayment()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        ErrorOr<RecurringPayment> created = repository.Create(
            BuildPayment(userId, billerId, accountId, nextExecutionDate: DateTime.UtcNow.AddDays(30)));

        ErrorOr<List<RecurringPayment>> result = repository.GetDuePayments(DateTime.UtcNow);

        Assert.False(result.IsError);
        Assert.DoesNotContain(result.Value, p => p.Id == created.Value.Id);
    }

    [Fact]
    public void GetDuePayments_WhenPaymentIsPaused_DoesNotReturnPayment()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        ErrorOr<RecurringPayment> created = repository.Create(
            BuildPayment(
                userId,
                billerId,
                accountId,
                nextExecutionDate: DateTime.UtcNow.AddDays(-1),
                status: RecurringPaymentStatus.Paused));

        ErrorOr<List<RecurringPayment>> result = repository.GetDuePayments(DateTime.UtcNow);

        Assert.False(result.IsError);
        Assert.DoesNotContain(result.Value, p => p.Id == created.Value.Id);
    }

    [Fact]
    public void Update_WhenPaymentExists_PersistsChanges()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment created = repository.Create(BuildPayment(userId, billerId, accountId)).Value;
        created.Status = RecurringPaymentStatus.Paused;
        created.NextExecutionDate = DateTime.UtcNow.AddMonths(1);

        ErrorOr<Success> result = repository.Update(created);

        Assert.False(result.IsError);
        ErrorOr<RecurringPayment> fetched = repository.GetById(created.Id);
        Assert.False(fetched.IsError);
        Assert.Equal(RecurringPaymentStatus.Paused, fetched.Value.Status);
    }

    [Fact]
    public void Update_WhenStatusChangedToCancelled_ReflectsOnSubsequentRead()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment created = repository.Create(BuildPayment(userId, billerId, accountId)).Value;
        created.Status = RecurringPaymentStatus.Cancelled;

        ErrorOr<Success> result = repository.Update(created);

        Assert.False(result.IsError);
        ErrorOr<RecurringPayment> fetched = repository.GetById(created.Id);
        Assert.False(fetched.IsError);
        Assert.Equal(RecurringPaymentStatus.Cancelled, fetched.Value.Status);
    }

    [Fact]
    public void Update_WhenCancelledPaymentWasDue_IsExcludedFromDuePayments()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new RecurringPaymentRepository(context);
        (int userId, int billerId, int accountId) = SeedDependencies(context);
        RecurringPayment created = repository
            .Create(BuildPayment(userId, billerId, accountId, nextExecutionDate: DateTime.UtcNow.AddDays(-1)))
            .Value;
        created.Status = RecurringPaymentStatus.Cancelled;
        repository.Update(created);

        ErrorOr<List<RecurringPayment>> result = repository.GetDuePayments(DateTime.UtcNow);

        Assert.False(result.IsError);
        Assert.DoesNotContain(result.Value, p => p.Id == created.Id);
    }

    private static (int UserId, int BillerId, int AccountId) SeedDependencies(AppDatabaseContext context)
    {
        var user = new User
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            PasswordHash = "hash",
            Is2FaEnabled = false,
            IsLocked = false,
            FailedLoginAttempts = 0,
            PreferredLanguage = "en",
        };
        context.Users.Add(user);

        var biller = new Biller
        {
            Name = $"Biller-{Guid.NewGuid()}",
            Category = BillerCategory.Utilities.ToString(),
            IsActive = true,
        };
        context.Billers.Add(biller);
        context.SaveChanges();

        var account = new Account
        {
            UserId = user.Id,
            Iban = $"RO{Guid.NewGuid():N}"[..24],
            Currency = "RON",
            Balance = 5000m,
            Status = AccountStatus.Active,
        };
        context.Accounts.Add(account);
        context.SaveChanges();

        return (user.Id, biller.Id, account.Id);
    }

    private static RecurringPayment BuildPayment(
        int userId,
        int billerId,
        int accountId,
        decimal amount = 200m,
        bool isPayInFull = false,
        RecurringFrequency frequency = RecurringFrequency.Monthly,
        DateTime? nextExecutionDate = null,
        DateTime? endDate = null,
        RecurringPaymentStatus status = RecurringPaymentStatus.Active)
    {
        DateTime start = DateTime.UtcNow.Date;
        return new RecurringPayment
        {
            UserId = userId,
            BillerId = billerId,
            SourceAccountId = accountId,
            Amount = amount,
            IsPayInFull = isPayInFull,
            Frequency = frequency,
            StartDate = start,
            EndDate = endDate,
            NextExecutionDate = nextExecutionDate ?? start.AddMonths(1),
            Status = status,
            CreatedAt = DateTime.UtcNow,
        };
    }
}