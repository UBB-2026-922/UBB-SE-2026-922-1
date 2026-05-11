namespace BankingApp.Infrastructure.Persistence.Repositories;

using BankingApp.Domain.Aggregates.AccountAggregate.Entities;
using BankingApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions.FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> ListByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .Where(transaction => transaction.AccountId == accountId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
