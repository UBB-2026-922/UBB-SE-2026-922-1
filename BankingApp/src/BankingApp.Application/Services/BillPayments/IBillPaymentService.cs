// <copyright file="IBillPaymentService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IBillPaymentService interface.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Domain.Entities;

namespace BankingApp.Application.Services.BillPayments;

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
    Task<BillPayment> ProcessPaymentAsync(BillPaymentDto request);

    /// <summary>
    /// Retrieves all available billers.
    /// </summary>
    /// <returns>A collection of billers.</returns>
    Task<IEnumerable<Biller>> GetAllBillersAsync();

    /// <summary>
    /// Saves a biller for a user for future use.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="billerId">The biller identifier.</param>
    /// <param name="nickname">A personal nickname for the biller.</param>
    /// <returns>A boolean indicating success.</returns>
    Task<bool> SaveBillerForUserAsync(int userId, int billerId, string nickname);

    /// <summary>
    /// Checks if a payment amount requires two-factor authentication.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>True if 2FA is required.</returns>
    bool Requires2Fa(decimal amount);
}