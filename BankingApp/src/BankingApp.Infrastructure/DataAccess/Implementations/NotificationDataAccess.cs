// <copyright file="NotificationDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for notification records.
/// </summary>
public class NotificationDataAccess : INotificationDataAccess
{
    private const string SelectAllColumns = """
                                            SELECT Id, UserId, Title, [Message], [Type], Channel,
                                                   IsRead, RelatedEntityType, RelatedEntityId, CreatedAt
                                            FROM Notification
                                            """;

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public NotificationDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<int> CountUnreadByUserId(int userId)
    {
        const string query = "SELECT COUNT(*) FROM Notification WHERE UserId = @UserId AND IsRead = 0";
        return _databaseContext.Query(connection => connection.QueryFirst<int>(query, new { UserId = userId }));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return _databaseContext.Query(connection =>
            connection.Query<Notification>(query, new { UserId = userId }).AsList());
    }
}