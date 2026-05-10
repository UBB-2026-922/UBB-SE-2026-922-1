namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Domain.Extensions;
using Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            User? user = _databaseContext.Users.Find(userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            NotificationPreference notification = new()
            {
                User = user,
                Category = NotificationTypeExtensions.FromString(category),
                PushEnabled = false,
                EmailEnabled = false,
                SmsEnabled = false
            };
            _databaseContext.NotificationPreferences.Add(notification);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: $"Failed to create notification preference: {exception.Message}");
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<NotificationPreference>> FindByUserId(int userId)
    {
        var preferences = _databaseContext.NotificationPreferences.Where(preference => EF.Property<int>(preference, "UserId") == userId)
            .ToList();
        return preferences;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences)
    {
        try
        {
            foreach (NotificationPreference preference in preferences)
            {
                NotificationPreference? existing = _databaseContext.NotificationPreferences
                    .FirstOrDefault(existingPreference => EF.Property<int>(existingPreference, "UserId") == userId && existingPreference.Category == preference.Category);
                if (existing is not null)
                {
                    existing.PushEnabled = preference.PushEnabled;
                    existing.EmailEnabled = preference.EmailEnabled;
                    existing.SmsEnabled = preference.SmsEnabled;
                    existing.MinAmountThreshold = preference.MinAmountThreshold;
                }
            }

            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }
}
