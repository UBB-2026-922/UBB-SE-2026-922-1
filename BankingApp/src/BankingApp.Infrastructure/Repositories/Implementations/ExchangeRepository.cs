namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using DataAccess;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     EF Core implementation of <see cref="IExchangeRepository" />.
/// </summary>
public class ExchangeRepository : IExchangeRepository
{
    private const string ExchangeRelatedEntityType = "Exchange";
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The application database context.</param>
    public ExchangeRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> GetById(int id)
    {
        ExchangeTransaction? exchange =
            _databaseContext.ExchangeTransactions
                .Include(transaction => transaction.User)
                .Include(transaction => transaction.SourceAccount)
                .Include(transaction => transaction.TargetAccount)
                .Include(transaction => transaction.Transaction)
                .FirstOrDefault(transaction => transaction.Id == id);
        if (exchange is null)
        {
            return Error.NotFound(description: "Exchange transaction not found.");
        }

        return exchange;
    }

    /// <inheritdoc />
    public ErrorOr<List<ExchangeTransaction>> GetByUserId(int userId)
    {
        try
        {
            return _databaseContext.ExchangeTransactions
                .Include(transaction => transaction.User)
                .Include(transaction => transaction.SourceAccount)
                .Include(transaction => transaction.TargetAccount)
                .Include(transaction => transaction.Transaction)
                .Where(transaction => EF.Property<int>(transaction, "UserId") == userId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ToList();
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> Create(ExchangeTransaction exchange)
    {
        using IDbContextTransaction databaseTransaction = _databaseContext.Database.BeginTransaction();

        try
        {
            Account? sourceAccount =
                _databaseContext.Accounts.FirstOrDefault(account => account.Id == exchange.SourceAccount!.Id);
            if (sourceAccount is null)
            {
                return Error.NotFound(description: "Source account not found.");
            }

            Account? targetAccount =
                _databaseContext.Accounts.FirstOrDefault(account => account.Id == exchange.TargetAccount!.Id);
            if (targetAccount is null)
            {
                return Error.NotFound(description: "Target account not found.");
            }

            decimal totalDebit = exchange.SourceAmount;
            if (sourceAccount.Balance < totalDebit)
            {
                return Error.Forbidden(description: "Insufficient funds for exchange.");
            }

            sourceAccount.Balance -= totalDebit;
            targetAccount.Balance += exchange.TargetAmount;

            var ledgerTransaction = new Transaction
            {
                Account = sourceAccount,
                TransactionRef = $"FX-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                Type = ExchangeRelatedEntityType,
                Direction = TransactionDirection.Out,
                Amount = exchange.SourceAmount,
                Currency = exchange.SourceCurrency,
                BalanceAfter = sourceAccount.Balance,
                CounterpartyName = $"Exchange to {exchange.TargetCurrency}",
                Fee = exchange.Commission,
                ExchangeRate = exchange.ExchangeRate,
                Status = TransactionStatus.Completed,
                RelatedEntityType = ExchangeRelatedEntityType,
                CreatedAt = exchange.CreatedAt
            };

            _databaseContext.Transactions.Add(ledgerTransaction);
            _databaseContext.SaveChanges();

            exchange.Transaction = ledgerTransaction;
            _databaseContext.ExchangeTransactions.Add(exchange);
            _databaseContext.SaveChanges();
            databaseTransaction.Commit();
            return exchange;
        }
        catch (Exception exception)
        {
            databaseTransaction.Rollback();
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> UpdateStatus(int exchangeId, ExchangeTransactionStatus status)
    {
        try
        {
            ExchangeTransaction? exchange =
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
