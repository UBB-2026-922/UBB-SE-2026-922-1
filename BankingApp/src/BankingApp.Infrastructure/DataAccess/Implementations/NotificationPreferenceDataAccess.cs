// <copyright file="NotificationPreferenceDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationPreferenceDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Extensions;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for notification preference records.
/// </summary>
internal class NotificationPreferenceDataAccess : INotificationPreferenceDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationPreferenceDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public NotificationPreferenceDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="category">The category value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Create(int userId, string category)
    {
        const string databaseCommandText = """
                                           INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
                                           VALUES (@UserId, @Category, 0, 0, 0)
                                           """;
        return _databaseContext.Query(connection => connection.Execute(
                databaseCommandText,
                new { UserId = userId, Category = category }))
            .Then(rows =>
                rows > default(int)
                    ? Result.Success
                    : (ErrorOr<Success>)Error.Failure(description: "Failed to create notification preference."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<NotificationPreference>> FindByUserId(int userId)
    {
        const string query = """
                             SELECT Id, UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold
                             FROM NotificationPreference
                             WHERE UserId = @UserId
                             """;
        return _databaseContext.Query(connection =>
            connection.Query<NotificationPreference>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences)
    {
        const string databaseCommandText = """
                                           UPDATE NotificationPreference
                                           SET PushEnabled        = @PushEnabled,
                                               EmailEnabled       = @EmailEnabled,
                                               SmsEnabled         = @SmsEnabled,
                                               MinAmountThreshold = @MinAmountThreshold
                                           WHERE UserId   = @UserId
                                             AND Category = @Category
                                           """;
        return _databaseContext.Query(connection => connection.Execute(
                databaseCommandText,
                preferences.Select(preference => new
                {
                    preference.UserId,
                    Category = preference.Category.ToDisplayName(),
                    preference.PushEnabled,
                    preference.EmailEnabled,
                    preference.SmsEnabled,
                    preference.MinAmountThreshold,
                })))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}