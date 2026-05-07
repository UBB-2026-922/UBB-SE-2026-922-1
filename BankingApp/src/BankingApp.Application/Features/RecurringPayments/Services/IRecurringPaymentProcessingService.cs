namespace BankingApp.Application.Features.RecurringPayments.Services;

using ErrorOr;

/// <summary>
///     Defines background processing operations for recurring payments.
/// </summary>
public interface IRecurringPaymentProcessingService
{
    /// <summary>
    ///     Processes all due recurring payments.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success when processing completes, or an error when the batch fails.</returns>
    public Task<ErrorOr<Success>> ProcessDuePaymentsAsync(CancellationToken cancellationToken = default);
}
