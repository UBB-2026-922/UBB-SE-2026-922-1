namespace BankingApp.Infrastructure.Repositories.Forex;


using BankingApp.Application.Features.Forex.Repositories;
using Domain.Entities;
using Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.ForexAggregate;
using Domain.Aggregates.TransactionAggregate;
using ErrorOr;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence;

/// <summary>
///     EF Core implementation of <see cref="IForexRepository" />.
/// </summary>
public class ForexRepository : IForexRepository
{
    private const string ExchangeRelatedEntityType = "Exchange";
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The application database context.</param>
    public ForexRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<ForexTransaction> GetById(int id)
    {
        ForexTransaction? exchange =
            _databaseContext.ExchangeTransactions.FirstOrDefault(transaction => transaction.Id == id);
        if (exchange is null)
        {
            return Error.NotFound(description: "Exchange transaction not found.");
        }

        return exchange;
    }

    /// <inheritdoc />
    public ErrorOr<List<ForexTransaction>> GetByUserId(int userId)
    {
        try
        {
            return _databaseContext.ExchangeTransactions
                .Where(transaction => transaction.UserId == userId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ToList();
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<ForexTransaction> Create(ForexTransaction forex)
    {
        using IDbContextTransaction databaseTransaction = _databaseContext.Database.BeginTransaction();

        try
        {
            Account? sourceAccount =
                _databaseContext.Accounts.FirstOrDefault(account => account.Id == forex.SourceAccountId);
            if (sourceAccount is null)
            {
                return Error.NotFound(description: "Source account not found.");
            }

            Account? targetAccount =
                _databaseContext.Accounts.FirstOrDefault(account => account.Id == forex.TargetAccountId);
            if (targetAccount is null)
            {
                return Error.NotFound(description: "Target account not found.");
            }

            decimal totalDebit = forex.SourceAmount;
            if (sourceAccount.Balance < totalDebit)
            {
                return Error.Forbidden(description: "Insufficient funds for exchange.");
            }

            sourceAccount.Balance -= totalDebit;
            targetAccount.Balance += forex.TargetAmount;

            var ledgerTransaction = new Transaction
            {
                AccountId = sourceAccount.Id,
                TransactionRef = $"FX-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                Type = ExchangeRelatedEntityType,
                Direction = TransactionDirection.Out,
                Amount = forex.SourceAmount,
                Currency = forex.SourceCurrency,
                BalanceAfter = sourceAccount.Balance,
                CounterpartyName = $"Exchange to {forex.TargetCurrency}",
                Fee = forex.Commission,
                ExchangeRate = forex.ExchangeRate,
                Status = TransactionStatus.Completed,
                RelatedEntityType = ExchangeRelatedEntityType,
                CreatedAt = forex.CreatedAt
            };

            _databaseContext.Transactions.Add(ledgerTransaction);
            _databaseContext.SaveChanges();

            forex.TransactionId = ledgerTransaction.Id;
            _databaseContext.ExchangeTransactions.Add(forex);
            _databaseContext.SaveChanges();
            databaseTransaction.Commit();
            return forex;
        }
        catch (Exception exception)
        {
            databaseTransaction.Rollback();
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<ForexTransaction> UpdateStatus(int exchangeId, ExchangeTransactionStatus status)
    {
        try
        {
            ForexTransaction? exchange =
                _databaseContext.ExchangeTransactions.FirstOrDefault(transaction => transaction.Id == exchangeId);
            if (exchange is null)
            {
                return Error.NotFound(description: "Exchange transaction not found.");
            }

            exchange.Status = status;
            _databaseContext.SaveChanges();
            return exchange;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }
}
