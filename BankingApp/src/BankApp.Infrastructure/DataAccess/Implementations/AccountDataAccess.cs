// <copyright file="AccountDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AccountDataAccess class.
// </summary>

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for bank account records.
/// </summary>
public class AccountDataAccess : IAccountDataAccess
{
    private const string SelectAllColumns = """
                                            SELECT Id, UserId, AccountName, IBAN, Currency, Balance,
                                                   AccountType, Status, CreatedAt
                                            FROM Account
                                            """;

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public AccountDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Account> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<Account>(query, new { Id = id }))
            .Then(account => account ?? (ErrorOr<Account>)Error.NotFound(description: "Account not found."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Account>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return _databaseContext.Query(connection => connection.Query<Account>(query, new { UserId = userId }).AsList());
    }
}