// <copyright file="IRecurringPaymentProcessingService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IRecurringPaymentProcessingService interface.
// </summary>

using ErrorOr;

namespace BankingApp.Application.Services.RecurringPayments;

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
    Task<ErrorOr<Success>> ProcessDuePaymentsAsync(CancellationToken cancellationToken = default);
}
