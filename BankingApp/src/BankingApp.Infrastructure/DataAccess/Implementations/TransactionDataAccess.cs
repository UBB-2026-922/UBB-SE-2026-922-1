using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for financial transaction records.
/// </summary>
public class TransactionDataAccess : ITransactionDataAccess
{
    private const int DefaultTransactionLimit = 10;
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public TransactionDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <summary>
    ///     Inserts a transaction record into the database.
    /// </summary>
    /// <param name="transaction">The transaction entity to insert.</param>
    /// <returns>
    ///     An <see cref="ErrorOr{Transaction}"/> containing the created <see cref="Transaction"/> on success,
    ///     or an error describing the failure.
    /// </returns>
    public ErrorOr<Transaction> Add(Transaction transaction)
    {
        try
        {
            _databaseContext.Transactions.Add(transaction);
            _databaseContext.SaveChanges();
            return transaction;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="accountId">The accountId value.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transaction>> FindRecentByAccountId(int accountId, int limit = DefaultTransactionLimit)
    {
        var transactions = _databaseContext.Transactions
            .Where(transaction => transaction.AccountId == accountId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Take(limit)
            .ToList();
        return transactions;
    }
}
