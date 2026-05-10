namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a password reset token issued to a user.
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    ///     Gets or sets the unique identifier for the token.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the user this token belongs to.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    ///     Gets or sets the hashed value of the reset token.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the token expires.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the token was used, if applicable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the token was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}
