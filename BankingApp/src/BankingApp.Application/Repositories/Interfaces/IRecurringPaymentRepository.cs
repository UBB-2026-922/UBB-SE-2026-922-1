// <copyright file="IRecurringPaymentRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IRecurringPaymentRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines persistence operations for the <see cref="RecurringPayment" /> entity.
/// </summary>
public interface IRecurringPaymentRepository
{
    /// <summary>Retrieves a recurring payment by its unique identifier.</summary>
    /// <param name="id">The recurring payment identifier.</param>
    /// <returns>The matching <see cref="RecurringPayment" />, or an error when not found.</returns>
    ErrorOr<RecurringPayment> GetById(int id);

    /// <summary>Retrieves all recurring payments belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of recurring payments, or an error.</returns>
    ErrorOr<List<RecurringPayment>> GetByUserId(int userId);

    /// <summary>Retrieves all active recurring payments whose next execution date is on or before <paramref name="asOf" />.</summary>
    /// <param name="asOf">The reference date used to filter due payments.</param>
    /// <returns>A list of due recurring payments, or an error.</returns>
    ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf);

    /// <summary>Persists a new recurring payment schedule.</summary>
    /// <param name="payment">The schedule to create.</param>
    /// <returns>The created schedule with its assigned identifier, or an error.</returns>
    ErrorOr<RecurringPayment> Create(RecurringPayment payment);

    /// <summary>Updates a recurring payment schedule (amount, frequency, dates, status).</summary>
    /// <param name="payment">The schedule with updated values.</param>
    /// <returns>The updated schedule, or an error.</returns>
    ErrorOr<RecurringPayment> Update(RecurringPayment payment);

    /// <summary>Cancels a recurring payment schedule by marking its status as Cancelled.</summary>
    /// <param name="id">The identifier of the schedule to cancel.</param>
    /// <returns>Success, or an error when the record is not found.</returns>
    ErrorOr<Success> Cancel(int id);
}
