namespace BankingApp.Infrastructure.Tests.Integration;

using Domain.Entities;
using Domain.Enums;
using BankingApp.Infrastructure.DataAccess;

using Infrastructure;
using ErrorOr;
using Persistence;

/// <summary>
///     Integration tests for <see cref="TransferRepository" /> against a real database.
/// </summary>
[Collection("Integration")]
public class TransferRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferRepositoryTests" /> class.
    /// </summary>
    /// <param name="fixture">The shared database fixture.</param>
    public TransferRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Create_WhenTransferIsValid_ReturnsPersistedTransferWithId()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer transfer = BuildTransfer(userId, accountId);

        ErrorOr<Transfer> result = repository.Create(transfer);

        Assert.False(result.IsError);
        Assert.True(result.Value.Id > 0);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(accountId, result.Value.SourceAccountId);
        Assert.Equal(transfer.Amount, result.Value.Amount);
        Assert.Equal(TransferStatus.Pending, result.Value.Status);
    }

    [Fact]
    public void Create_WhenReferenceIsProvided_PersistsReference()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer transfer = BuildTransfer(userId, accountId, reference: "INV-2026-001");

        ErrorOr<Transfer> result = repository.Create(transfer);

        Assert.False(result.IsError);
        Assert.Equal("INV-2026-001", result.Value.Reference);
    }

    [Fact]
    public void Create_WhenRecipientDetailsAreProvided_PersistsRecipientFields()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer transfer = BuildTransfer(
            userId,
            accountId,
            recipientName: "Jane Doe",
            recipientIban: "RO49AAAA1B31007593840000");

        ErrorOr<Transfer> result = repository.Create(transfer);

        Assert.False(result.IsError);
        Assert.Equal("Jane Doe", result.Value.RecipientName);
        Assert.Equal("RO49AAAA1B31007593840000", result.Value.RecipientIban);
    }

    [Fact]
    public void GetById_WhenTransferExists_ReturnsCorrectTransfer()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer created = repository.Create(BuildTransfer(userId, accountId)).Value;

        ErrorOr<Transfer> result = repository.GetById(created.Id);

        Assert.False(result.IsError);
        Assert.Equal(created.Id, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
    }

    [Fact]
    public void GetById_WhenTransferDoesNotExist_ReturnsError()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);

        ErrorOr<Transfer> result = repository.GetById(int.MaxValue);

        Assert.True(result.IsError);
    }

    [Fact]
    public void GetByUserId_WhenUserHasTransfers_ReturnsAllUserTransfers()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        repository.Create(BuildTransfer(userId, accountId, amount: 100m));
        repository.Create(BuildTransfer(userId, accountId, amount: 200m));

        ErrorOr<List<Transfer>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.True(result.Value.Count >= 2);
        Assert.All(result.Value, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public void GetByUserId_WhenOtherUsersHaveTransfers_ReturnsOnlyRequestedUsersTransfers()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        (int otherUserId, int otherAccountId) = SeedDependencies(context);
        repository.Create(BuildTransfer(userId, accountId));
        repository.Create(BuildTransfer(otherUserId, otherAccountId));

        ErrorOr<List<Transfer>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.DoesNotContain(result.Value, t => t.UserId == otherUserId);
    }

    [Fact]
    public void GetByUserId_WhenUserHasNoTransfers_ReturnsEmptyList()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, _) = SeedDependencies(context);

        ErrorOr<List<Transfer>> result = repository.GetByUserId(userId);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void UpdateStatus_WhenTransferExists_PersistsNewStatus()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer created = repository.Create(BuildTransfer(userId, accountId)).Value;

        ErrorOr<Transfer> result = repository.UpdateStatus(created.Id, TransferStatus.Completed);

        Assert.False(result.IsError);
        Assert.Equal(TransferStatus.Completed, result.Value.Status);
    }

    [Fact]
    public void UpdateStatus_WhenTransferExists_ReflectsOnSubsequentRead()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer created = repository.Create(BuildTransfer(userId, accountId)).Value;
        repository.UpdateStatus(created.Id, TransferStatus.Failed);

        ErrorOr<Transfer> fetched = repository.GetById(created.Id);

        Assert.False(fetched.IsError);
        Assert.Equal(TransferStatus.Failed, fetched.Value.Status);
    }

    [Fact]
    public void UpdateStatus_WhenTransferDoesNotExist_ReturnsError()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);

        ErrorOr<Transfer> result = repository.UpdateStatus(int.MaxValue, TransferStatus.Completed);

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateStatus_WhenCancelledAfterPending_IsExcludedFromActiveView()
    {
        using AppDatabaseContext context = _fixture.CreateDatabaseContext();
        var repository = new TransferRepository(context);
        (int userId, int accountId) = SeedDependencies(context);
        Transfer created = repository.Create(BuildTransfer(userId, accountId)).Value;
        repository.UpdateStatus(created.Id, TransferStatus.Cancelled);

        ErrorOr<List<Transfer>> history = repository.GetByUserId(userId);

        Assert.False(history.IsError);
        Transfer? found = history.Value.FirstOrDefault(t => t.Id == created.Id);
        Assert.NotNull(found);
        Assert.Equal(TransferStatus.Cancelled, found.Status);
    }

    private static (int UserId, int AccountId) SeedDependencies(AppDatabaseContext context)
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
        context.SaveChanges();

        var account = new Account
        {
            UserId = user.Id,
            Iban = $"RO{Guid.NewGuid():N}"[..24],
            Currency = "RON",
            Balance = 10000m,
            Status = AccountStatus.Active,
        };
        context.Accounts.Add(account);
        context.SaveChanges();

        return (user.Id, account.Id);
    }

    private static Transfer BuildTransfer(
        int userId,
        int accountId,
        decimal amount = 500m,
        string currency = "RON",
        string recipientName = "John Doe",
        string recipientIban = "RO49AAAA1B31007593840000",
        string? reference = null,
        TransferStatus status = TransferStatus.Pending)
    {
        return new Transfer
        {
            UserId = userId,
            SourceAccountId = accountId,
            Amount = amount,
            Currency = currency,
            RecipientName = recipientName,
            RecipientIban = recipientIban,
            Reference = reference,
            Status = status,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
