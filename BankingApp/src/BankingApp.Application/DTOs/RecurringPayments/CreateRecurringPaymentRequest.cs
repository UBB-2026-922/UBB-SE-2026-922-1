namespace BankingApp.Application.DTOs.RecurringPayments;

using Domain.Enums;

/// <summary>
///     Request payload for creating a new recurring payment schedule.
/// </summary>
public class CreateRecurringPaymentRequest
{
    /// <summary>Gets or sets the identifier of the biller (payee).</summary>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the identifier of the account funds are drawn from.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the fixed payment amount per execution.</summary>
    public decimal Amount { get; set; }

    /// <summary>When <see langword="true" />, the full outstanding balance is paid instead of <see cref="Amount" />.</summary>
    public bool IsPayInFull { get; set; }

    /// <summary>Gets or sets how often the payment repeats.</summary>
    public RecurringFrequency Frequency { get; set; }

    /// <summary>Gets or sets the date on which the schedule begins.</summary>
    public DateTime StartDate { get; set; }

    /// <summary><see langword="null" /> means the schedule continues indefinitely.</summary>
    public DateTime? EndDate { get; set; }
}
