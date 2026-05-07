namespace BankingApp.Domain.Repositories;

using Aggregates.AccountAggregate.Entities;

public interface ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
