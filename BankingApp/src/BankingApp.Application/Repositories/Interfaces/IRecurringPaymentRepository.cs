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
///     Defines repository operations for recurring payment schedules.
/// </summary>
public interface IRecurringPaymentRepository
{
    /// <summary>Creates a new recurring payment record and returns the persisted entity.</summary>
    /// <param name="payment">The recurring payment entity to persist.</param>
    /// <returns>The saved entity with its generated identifier, or an error if the operation failed.</returns>
    ErrorOr<RecurringPayment> Create(RecurringPayment payment);

    /// <summary>Finds a recurring payment by its unique identifier.</summary>
    /// <param name="id">The recurring payment identifier.</param>
    /// <returns>The matching entity, or <see cref="Error.NotFound" /> if no record exists.</returns>
    ErrorOr<RecurringPayment> GetById(int id);

    /// <summary>Returns all recurring payments belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of the user's recurring payments, or an error if the operation failed.</returns>
    ErrorOr<List<RecurringPayment>> GetByUserId(int userId);

    /// <summary>Returns all recurring payments whose next execution date falls at or before the given threshold.</summary>
    /// <param name="dueBy">The UTC cutoff date and time.</param>
    /// <returns>A list of due payments, or an error if the operation failed.</returns>
    ErrorOr<List<RecurringPayment>> GetDueBefore(DateTime dueBy);

    /// <summary>Persists changes to an existing recurring payment record.</summary>
    /// <param name="payment">The recurring payment entity with updated values.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Update(RecurringPayment payment);
}
