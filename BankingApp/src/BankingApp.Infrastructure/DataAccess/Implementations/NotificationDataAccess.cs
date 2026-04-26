// <copyright file="NotificationDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for notification records.
/// </summary>
public class NotificationDataAccess : INotificationDataAccess
{

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
        List<Notification> notifications = _databaseContext.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
        return notifications.Count;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        List<Notification> notifications = _databaseContext.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToList();
        return notifications;
    }
}