namespace BankingApp.Application.Features.BillPayments.Services;

using Domain.Entities;
using BankingApp.Application.Features.BillPayments.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.BillerAggregate;
using Domain.Aggregates.BillPaymentAggregate;

/// <summary>
/// Defines the business logic for handling bill payments.
/// </summary>
public interface IBillPaymentService
{
    /// <summary>
    /// Processes a new bill payment.
    /// </summary>
    /// <param name="request">The payment request details.</param>
    /// <returns>The created bill payment record.</returns>
    public Task<BillPayment> ProcessPaymentAsync(BillPaymentDto request);

    /// <summary>
    /// Retrieves all available billers.
    /// </summary>
    /// <returns>A collection of billers.</returns>
    public Task<IEnumerable<Biller>> GetAllBillersAsync();

    /// <summary>
    /// Retrieves all accounts that can be used for bill payments.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user's source accounts.</returns>
    public Task<IEnumerable<Account>> GetAccountsForUserAsync(int userId);

    /// <summary>
    /// Saves a biller for a user for future use.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="billerId">The biller identifier.</param>
    /// <param name="nickname">A personal nickname for the biller.</param>
    /// <returns>A boolean indicating success.</returns>
    public Task<bool> SaveBillerForUserAsync(int userId, int billerId, string nickname);

    /// <summary>
    /// Checks if a payment amount requires two-factor authentication.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>True if 2FA is required.</returns>
    public bool Requires2Fa(decimal amount);

    /// <summary>
    /// Calculates the processing fee for the supplied payment amount.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>The fee amount.</returns>
    public decimal CalculateFee(decimal amount);
}
