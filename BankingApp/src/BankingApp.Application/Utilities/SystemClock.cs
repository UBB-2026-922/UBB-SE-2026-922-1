namespace BankingApp.Application.Utilities;

/// <summary>
///     Production implementation of <see cref="ISystemClock" /> that delegates to <see cref="DateTime.UtcNow" />.
/// </summary>
public class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
