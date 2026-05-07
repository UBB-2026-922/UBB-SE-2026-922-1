namespace BankingApp.Application.Features.RecurringPayments.Repositories;

using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines persistence operations for the <see cref="RecurringPayment" /> entity.
/// </summary>
public interface IRecurringPaymentRepository
{
    /// <summary>Retrieves a recurring payment by its unique identifier.</summary>
    /// <returns>The matching <see cref="RecurringPayment" />, or an error when not found.</returns>
    public ErrorOr<RecurringPayment> GetById(int id);

    /// <summary>Retrieves all recurring payments belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of the user's recurring payments, or an error if the operation failed.</returns>
    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId);

    /// <summary>Retrieves all active recurring payments whose next execution date is on or before <paramref name="asOf" />.</summary>
    /// <param name="asOf">The reference date used to filter due payments.</param>
    /// <returns>A list of due recurring payments, or an error.</returns>
    public ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf);

    /// <summary>Persists a new recurring payment schedule.</summary>
    /// <param name="payment">The schedule to create.</param>
    /// <returns>The created schedule with its assigned identifier, or an error.</returns>
    public ErrorOr<RecurringPayment> Create(RecurringPayment payment);

    /// <summary>Persists changes to an existing recurring payment record.</summary>
    /// <param name="payment">The recurring payment entity with updated values.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    public ErrorOr<Success> Update(RecurringPayment payment);
}
