// <copyright file="TransactionDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

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

    /// <inheritdoc />
    /// <param name="accountId">The accountId value.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transaction>> FindRecentByAccountId(int accountId, int limit = DefaultTransactionLimit)
    {
        const string databaseCommandText = """
                                           SELECT TOP (@Limit)
                                               Id, AccountId, CardId, TransactionRef, [Type], Direction, Amount,
                                               Currency, BalanceAfter, CounterpartyName, CounterpartyIBAN, MerchantName,
                                               CategoryId, Description, Fee, ExchangeRate, [Status], RelatedEntityType,
                                               RelatedEntityId, CreatedAt
                                           FROM [Transaction]
                                           WHERE AccountId = @AccountId
                                           ORDER BY CreatedAt DESC
                                           """;
        return _databaseContext.Query(connection =>
            connection.Query<Transaction>(databaseCommandText, new { AccountId = accountId, Limit = limit }).AsList());
    }
}