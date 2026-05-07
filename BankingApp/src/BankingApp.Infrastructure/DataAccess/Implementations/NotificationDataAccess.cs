namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Interfaces;
using ErrorOr;

/// <summary>
///     Provides SQL Server data access for notification records.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="NotificationDataAccess" /> class.
/// </remarks>
/// <param name="databaseContext">The database context used for executing queries.</param>
/// <returns>The result of the operation.</returns>
public class NotificationDataAccess(AppDatabaseContext databaseContext) : INotificationDataAccess
{
    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<int> CountUnreadByUserId(int userId)
    {
        var notifications = databaseContext.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead).ToList();
        return notifications.Count;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        var notifications = databaseContext.Notifications.Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt).ToList();
        return notifications;
    }
}
