namespace BankingApp.Desktop.Utilities;

using System;

/// <summary>
///     Abstracts the system clock to allow deterministic time-based testing.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    ///     Gets the current UTC time.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime UtcNow { get; }
}
