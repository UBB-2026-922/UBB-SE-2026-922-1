namespace BankingApp.Domain.Entities;

using Enums;
using Errors;
using ErrorOr;

/// <summary>
///     Represents a scheduled recurring payment from a user's account to a biller.
///     Maps to the SQL table <c>RecurringPayment</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One).
///     </para>
///     <para>
///         Reuses the base entity <see cref="Account" /> via <see cref="SourceAccountId" /> (Many-to-One).
///     </para>
///     <para>
///         References <see cref="Biller" /> via <see cref="BillerId" /> (Many-to-One).
///     </para>
/// </remarks>
public class RecurringPayment
{
    private const int DailyIntervalDays = 1;
    private const int WeeklyIntervalDays = 7;
    private const int BiWeeklyIntervalDays = 14;
    private const int MonthlyIntervalMonths = 1;
    private const int QuarterlyIntervalMonths = 3;
    private const int YearlyIntervalYears = 1;

    /// <summary>Gets or sets the unique identifier for this recurring payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the <see cref="User" /> who owns this schedule.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the user who owns this schedule.</summary>
    public User? User { get; set; }

    /// <summary>Gets or sets the identifier of the target <see cref="Biller" />.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the target biller.</summary>
    public Biller? Biller { get; set; }

    /// <summary>Gets or sets the identifier of the source <see cref="Account" /> to be debited.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the source account to be debited.</summary>
    public Account? SourceAccount { get; set; }

    /// <summary>Gets or sets the amount debited on each execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the full outstanding balance
    ///     is paid on each execution rather than a fixed amount.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsPayInFull { get; set; }

    /// <summary>Gets or sets the cadence at which this payment repeats.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringFrequency Frequency { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the first payment is scheduled.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) after which no further payments will be made,
    ///     or <see langword="null" /> for indefinite schedules.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the date and time (UTC) of the next scheduled execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>Gets or sets the current lifecycle status of this recurring payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringPaymentStatus Status { get; set; } = RecurringPaymentStatus.Active;

    /// <summary>Gets or sets the date and time (UTC) when this schedule was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Creates a validated recurring payment aggregate.
    /// </summary>
    /// <param name="userId">The owner user identifier.</param>
    /// <param name="billerId">The biller identifier.</param>
    /// <param name="sourceAccountId">The source account identifier.</param>
    /// <param name="amount">The amount to debit each cycle.</param>
    /// <param name="isPayInFull">Whether the payment pays the full balance.</param>
    /// <param name="frequency">The schedule frequency.</param>
    /// <param name="startDate">The first scheduled payment date.</param>
    /// <param name="endDate">The optional schedule end date.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The result of the operation.</returns>
    public static ErrorOr<RecurringPayment> Create(
        int userId,
        int billerId,
        int sourceAccountId,
        decimal amount,
        bool isPayInFull,
        RecurringFrequency frequency,
        DateTime startDate,
        DateTime? endDate,
        DateTime createdAt)
    {
        if (amount <= 0)
        {
            return RecurringPaymentErrors.InvalidAmount;
        }

        if (endDate.HasValue && endDate.Value <= startDate)
        {
            return RecurringPaymentErrors.InvalidEndDate;
        }

        return new RecurringPayment
        {
            UserId = userId,
            BillerId = billerId,
            SourceAccountId = sourceAccountId,
            Amount = amount,
            IsPayInFull = isPayInFull,
            Frequency = frequency,
            StartDate = startDate,
            EndDate = endDate,
            NextExecutionDate = ComputeNextRunDate(frequency, startDate),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = createdAt
        };
    }

    /// <summary>
    ///     Pauses the recurring payment when requested by its owner.
    /// </summary>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Pause(int userId)
    {
        if (UserId != userId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        Status = RecurringPaymentStatus.Paused;
        return Result.Success;
    }

    /// <summary>
    ///     Resumes the recurring payment when requested by its owner.
    /// </summary>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Resume(int userId)
    {
        if (UserId != userId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        if (Status != RecurringPaymentStatus.Paused)
        {
            return RecurringPaymentErrors.ResumeConflict;
        }

        Status = RecurringPaymentStatus.Active;
        return Result.Success;
    }

    /// <summary>
    ///     Cancels the recurring payment when requested by its owner.
    /// </summary>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Cancel(int userId)
    {
        if (UserId != userId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        Status = RecurringPaymentStatus.Cancelled;
        return Result.Success;
    }

    /// <summary>
    ///     Advances the schedule to the next execution date or cancels it if the schedule is exhausted.
    /// </summary>
    public void AdvanceAfterSuccessfulExecution()
    {
        DateTime nextExecutionDate = ComputeNextRunDate(Frequency, NextExecutionDate);
        if (EndDate.HasValue && nextExecutionDate > EndDate.Value)
        {
            Status = RecurringPaymentStatus.Cancelled;
            return;
        }

        NextExecutionDate = nextExecutionDate;
    }

    /// <summary>
    ///     Pauses the schedule after an execution failure.
    /// </summary>
    public void MarkExecutionFailed()
    {
        Status = RecurringPaymentStatus.Paused;
    }

    private static DateTime ComputeNextRunDate(RecurringFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            RecurringFrequency.Daily => from.AddDays(DailyIntervalDays),
            RecurringFrequency.Weekly => from.AddDays(WeeklyIntervalDays),
            RecurringFrequency.BiWeekly => from.AddDays(BiWeeklyIntervalDays),
            RecurringFrequency.Monthly => from.AddMonths(MonthlyIntervalMonths),
            RecurringFrequency.Quarterly => from.AddMonths(QuarterlyIntervalMonths),
            RecurringFrequency.Yearly => from.AddYears(YearlyIntervalYears),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), $"Unknown frequency: {frequency}")
        };
    }
}
