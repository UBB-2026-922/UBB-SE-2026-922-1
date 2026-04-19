// <copyright file="CardDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the CardDataAccess class.
// </summary>

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for payment card records.
/// </summary>
public class CardDataAccess : ICardDataAccess
{
    private const string SelectAllColumns = """
                                            SELECT Id, AccountId, UserId, CardNumber, CardholderName,
                                                   ExpiryDate, CVV, CardType, CardBrand, Status,
                                                   DailyTransactionLimit, MonthlySpendingCap, AtmWithdrawalLimit,
                                                   ContactlessLimit, IsContactlessEnabled, IsOnlineEnabled,
                                                   SortOrder, CancelledAt, CreatedAt
                                            FROM Card
                                            """;

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CardDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public CardDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Card> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<Card>(query, new { Id = id }))
            .Then(card => card ?? (ErrorOr<Card>)Error.NotFound(description: "Card not found."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Card>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return _databaseContext.Query(connection => connection.Query<Card>(query, new { UserId = userId }).AsList());
    }
}