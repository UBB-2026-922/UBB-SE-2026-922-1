namespace BankingApp.Infrastructure.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class RecurringPaymentRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenRecurringPaymentExists_ShouldReturnRecurringPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenRecurringPaymentDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListDueAsync_WhenDueRecurringPaymentsExist_ShouldReturnRecurringPayments()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListDueAsync_WhenNoDueRecurringPaymentsExist_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasRecurringPayments_ShouldReturnRecurringPayments()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasNoRecurringPayments_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenRecurringPaymentIsValid_ShouldPersistRecurringPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task UpdateAsync_WhenRecurringPaymentExists_ShouldPersistChanges()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task DeleteAsync_WhenRecurringPaymentExists_ShouldRemoveRecurringPayment()
    {
        throw new NotImplementedException();
    }
}
