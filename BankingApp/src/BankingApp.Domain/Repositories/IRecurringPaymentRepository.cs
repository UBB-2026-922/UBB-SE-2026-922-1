namespace BankingApp.Domain.Repositories;

using BankingApp.Domain.Aggregates.RecurringPaymentAggregate;

public interface IRecurringPaymentRepository
{
    public Task<RecurringPayment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<RecurringPayment>> ListDueAsync(DateTime asOfUtc, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<RecurringPayment>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default);
    public Task UpdateAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default);
    public Task DeleteAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default);
}
