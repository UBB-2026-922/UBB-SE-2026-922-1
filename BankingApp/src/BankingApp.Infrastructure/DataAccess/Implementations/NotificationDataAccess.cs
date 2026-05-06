// <copyright file="NotificationDataAccess.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
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
        List<Notification> notifications = _databaseContext.Notifications.Where(notification => notification.UserId == userId && !notification.IsRead).ToList();
        return notifications.Count;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        List<Notification> notifications = _databaseContext.Notifications.Where(notification => notification.UserId == userId).OrderByDescending(notification => notification.CreatedAt).ToList();
        return notifications;
    }
}