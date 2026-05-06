namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the lifecycle status of a recurring payment schedule.
/// </summary>
public enum RecurringPaymentStatus
{
    /// <summary>
    ///     The recurring payment is active and will execute on the next scheduled date.
    /// </summary>
    Active,

    /// <summary>
    ///     The recurring payment has been temporarily paused and will not execute until resumed.
    /// </summary>
    Paused,

    /// <summary>
    ///     The recurring payment has been permanently cancelled.
    /// </summary>
    Cancelled
}
