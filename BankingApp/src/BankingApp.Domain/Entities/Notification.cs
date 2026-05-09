namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a notification sent to a user.
/// </summary>
public class Notification
{
    /// <summary>
    ///     Gets or sets the unique identifier for the notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the user this notification belongs to.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    ///     Gets or sets the title of the notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the message body of the notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of the notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the delivery channel used for the notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsRead { get; set; }

    /// <summary>
    ///     Gets or sets the type of entity related to this notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the entity related to this notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the notification was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
