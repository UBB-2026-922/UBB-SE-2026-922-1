namespace BankingApp.Domain.Repositories;

using BankingApp.Domain.Aggregates.BillPaymentAggregate;

public interface IBillPaymentRepository
{
    public Task<BillPayment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<BillPayment>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(BillPayment billPayment, CancellationToken cancellationToken = default);
}
