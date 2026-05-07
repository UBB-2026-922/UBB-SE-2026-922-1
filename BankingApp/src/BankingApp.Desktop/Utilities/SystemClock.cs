namespace BankingApp.Desktop.Utilities;

using System;

/// <summary>
///     Production implementation of <see cref="ISystemClock" /> backed by <see cref="DateTime.UtcNow" />.
/// </summary>
public class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
