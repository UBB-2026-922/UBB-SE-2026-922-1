// <copyright file="IBillPaymentRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IBillPaymentRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines persistence operations for the <see cref="BillPayment" /> and <see cref="Biller" /> entities.
/// </summary>
public interface IBillPaymentRepository
{
    /// <summary>Retrieves a bill payment by its unique identifier.</summary>
    /// <param name="id">The bill payment identifier.</param>
    /// <returns>The matching <see cref="BillPayment" />, or an error when not found.</returns>
    ErrorOr<BillPayment> GetById(int id);

    /// <summary>Retrieves all bill payments belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of bill payments, or an error.</returns>
    ErrorOr<List<BillPayment>> GetByUserId(int userId);

    /// <summary>Retrieves the complete list of registered billers.</summary>
    /// <returns>A list of all active and inactive billers, or an error.</returns>
    ErrorOr<List<Biller>> GetAllBillers();

    /// <summary>Persists a new bill payment record.</summary>
    /// <param name="payment">The payment to create.</param>
    /// <returns>The created payment with its assigned identifier, or an error.</returns>
    ErrorOr<BillPayment> Create(BillPayment payment);

    /// <summary>Updates the processing status of an existing bill payment.</summary>
    /// <param name="paymentId">The identifier of the payment to update.</param>
    /// <param name="status">The new status value.</param>
    /// <returns>The updated payment, or an error.</returns>
    ErrorOr<BillPayment> UpdateStatus(int paymentId, BillPaymentStatus status);
}
