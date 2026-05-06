using ErrorOr;

namespace BankingApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for recurring payment validation and lifecycle transitions.
/// </summary>
public static class RecurringPaymentErrors
{
    /// <summary>The payment amount must be greater than zero.</summary>
    public static readonly Error InvalidAmount =
        Error.Validation("recurring_payment.invalid_amount", "Amount must be greater than zero.");

    /// <summary>The schedule end date must be after the start date.</summary>
    public static readonly Error InvalidEndDate =
        Error.Validation("recurring_payment.invalid_end_date", "EndDate must be after StartDate.");

    /// <summary>The caller does not own the recurring payment.</summary>
    public static readonly Error Forbidden =
        Error.Forbidden(
            "recurring_payment.forbidden",
            "You do not have permission to modify this recurring payment.");

    /// <summary>Only paused recurring payments can be resumed.</summary>
    public static readonly Error ResumeConflict =
        Error.Conflict(
            "recurring_payment.resume_conflict",
            "Only paused recurring payments can be resumed.");
}
