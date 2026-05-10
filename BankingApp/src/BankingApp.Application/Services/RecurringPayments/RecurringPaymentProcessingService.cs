namespace BankingApp.Application.Services.RecurringPayments;

using BankingApp.Application.DTOs.BillPayments;
using Logging;
using Repositories.Interfaces;
using BillPayments;
using Utilities;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Processes recurring payments that have reached their next execution date.
/// </summary>
public class RecurringPaymentProcessingService : IRecurringPaymentProcessingService
{
    private readonly IBillPaymentService _billPaymentService;
    private readonly ISystemClock _clock;
    private readonly ILogger<RecurringPaymentProcessingService> _logger;
    private readonly IRecurringPaymentRepository _recurringPaymentRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentProcessingService" /> class.
    /// </summary>
    /// <param name="recurringPaymentRepository">The recurring payment repository.</param>
    /// <param name="billPaymentService">The bill-payment service.</param>
    /// <param name="clock">The system clock.</param>
    /// <param name="logger">The logger.</param>
    public RecurringPaymentProcessingService(
        IRecurringPaymentRepository recurringPaymentRepository,
        IBillPaymentService billPaymentService,
        ISystemClock clock,
        ILogger<RecurringPaymentProcessingService> logger)
    {
        _recurringPaymentRepository = recurringPaymentRepository;
        _billPaymentService = billPaymentService;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> ProcessDuePaymentsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ErrorOr<List<RecurringPayment>> duePaymentsResult = _recurringPaymentRepository.GetDuePayments(_clock.UtcNow);
        if (duePaymentsResult.IsError)
        {
            return duePaymentsResult.FirstError;
        }

        foreach (RecurringPayment payment in duePaymentsResult.Value.Where(payment =>
                     payment.Status == RecurringPaymentStatus.Active))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _billPaymentService.ProcessPaymentAsync(new BillPaymentDto
                {
                    UserId = payment.User?.Id ?? 0,
                    SourceAccountId = payment.SourceAccount?.Id ?? 0,
                    BillerId = payment.Biller?.Id ?? 0,
                    BillerReference = string.Empty,
                    Amount = payment.Amount,
                    IsPayInFull = payment.IsPayInFull
                });

                payment.AdvanceAfterSuccessfulExecution();

                ErrorOr<Success> updateResult = _recurringPaymentRepository.Update(payment);
                if (updateResult.IsError)
                {
                    _logger.RecurringPaymentUpdateAfterExecutionFailed(payment.Id, updateResult.FirstError.Description);
                }
            }
            catch (Exception exception)
            {
                payment.MarkExecutionFailed();
                _recurringPaymentRepository.Update(payment);
                _logger.RecurringPaymentBackgroundExecutionFailed(exception, payment.Id);
            }
        }

        return Result.Success;
    }
}
