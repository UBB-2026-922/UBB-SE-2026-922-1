// <copyright file="RecurringPaymentProcessingService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentProcessingService class.
// </summary>

using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.BillPayments;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.RecurringPayments;

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
        if (duePaymentsResult.IsError) return duePaymentsResult.FirstError;

        foreach (RecurringPayment payment in duePaymentsResult.Value.Where(payment =>
                     payment.Status == RecurringPaymentStatus.Active))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _billPaymentService.ProcessPaymentAsync(new BillPaymentDto
                {
                    UserId = payment.UserId,
                    SourceAccountId = payment.SourceAccountId,
                    BillerId = payment.BillerId,
                    BillerReference = string.Empty,
                    Amount = payment.Amount,
                    IsPayInFull = payment.IsPayInFull
                });

                payment.AdvanceAfterSuccessfulExecution();

                ErrorOr<Success> updateResult = _recurringPaymentRepository.Update(payment);
                if (updateResult.IsError)
                    _logger.LogWarning(
                        "Failed to update recurring payment {RecurringPaymentId} after execution: {Error}",
                        payment.Id,
                        updateResult.FirstError.Description);
            }
            catch (Exception exception)
            {
                payment.MarkExecutionFailed();
                _recurringPaymentRepository.Update(payment);
                _logger.LogWarning(
                    exception,
                    "Recurring payment {RecurringPaymentId} failed during background execution.",
                    payment.Id);
            }
        }

        return Result.Success;
    }
}
