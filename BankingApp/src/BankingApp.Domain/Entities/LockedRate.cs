namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a temporarily locked exchange rate for a user,
///     valid for 30 seconds from the moment it was locked.
/// </summary>
public class LockedRate
{
    private const int LockDurationSeconds = 30;

    /// <summary>Gets or sets the identifier of the user who locked this rate.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the user who locked this rate.</summary>
    public User? User { get; set; }

    /// <summary>Gets or sets the currency pair string (e.g., "EUR/USD").</summary>
    /// <value>Gets or sets the current value.</value>
    public string CurrencyPair { get; set; } = string.Empty;

    /// <summary>Gets or sets the locked exchange rate value.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Rate { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this rate was locked.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime LockedAt { get; set; }

    /// <summary>Returns whether the 30-second lock window has elapsed.</summary>
    /// <returns>True if the lock has expired.</returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow - LockedAt > TimeSpan.FromSeconds(LockDurationSeconds);
    }
}
