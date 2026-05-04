// <copyright file="IRecurringPaymentRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
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
    /// <returns>The matching <see cref="RecurringPayment" />, or an error when not found.</returns>
    ErrorOr<RecurringPayment> GetById(int id);

    /// <summary>Retrieves all recurring payments belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of the user's recurring payments, or an error if the operation failed.</returns>
    ErrorOr<List<RecurringPayment>> GetByUserId(int userId);

    /// <summary>Retrieves all active recurring payments whose next execution date is on or before <paramref name="asOf" />.</summary>
    /// <param name="asOf">The reference date used to filter due payments.</param>
    /// <returns>A list of due recurring payments, or an error.</returns>
    ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf);

    /// <summary>Persists a new recurring payment schedule.</summary>
    /// <param name="payment">The schedule to create.</param>
    /// <returns>The created schedule with its assigned identifier, or an error.</returns>
    ErrorOr<RecurringPayment> Create(RecurringPayment payment);
    /// <summary>Returns all recurring payments whose next execution date falls at or before the given threshold.</summary>
    /// <param name="dueBy">The UTC cutoff date and time.</param>
    /// <returns>A list of due payments, or an error if the operation failed.</returns>
    ErrorOr<List<RecurringPayment>> GetDueBefore(DateTime dueBy);

    /// <summary>Persists changes to an existing recurring payment record.</summary>
    /// <param name="payment">The recurring payment entity with updated values.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Update(RecurringPayment payment);
    /// <summary>Updates a recurring payment schedule (amount, frequency, dates, status).</summary>
    /// <param name="payment">The schedule with updated values.</param>
    /// <returns>The updated schedule, or an error.</returns>
    ErrorOr<RecurringPayment> Update(RecurringPayment payment);

    /// <summary>Cancels a recurring payment schedule by marking its status as Cancelled.</summary>
    /// <param name="id">The identifier of the schedule to cancel.</param>
    /// <returns>Success, or an error when the record is not found.</returns>
    ErrorOr<Success> Cancel(int id);
}
