// <copyright file="IBillPaymentRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IBillPaymentRepository interface.
// </summary>

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
/// Defines the data access operations for bill payments and billers.
/// </summary>
public interface IBillPaymentRepository
{
    /// <summary>
    /// Gets all available billers.
    /// </summary>
    /// <returns>A list of billers.</returns>
    Task<IEnumerable<Biller>> GetBillersAsync();

    /// <summary>
    /// Gets a specific biller by ID.
    /// </summary>
    /// <param name="billerId">The biller identifier.</param>
    /// <returns>The biller if found.</returns>
    Task<Biller?> GetBillerByIdAsync(int billerId);

    /// <summary>
    /// Saves a new bill payment record.
    /// </summary>
    /// <param name="payment">The payment entity.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddPaymentAsync(BillPayment payment);

    /// <summary>
    /// Gets the payment history for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of past bill payments.</returns>
    Task<IEnumerable<BillPayment>> GetUserPaymentHistoryAsync(int userId);

    /// <summary>
    /// Gets the list of billers saved by a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of saved biller records.</returns>
    Task<IEnumerable<SavedBiller>> GetSavedBillersAsync(int userId);

    /// <summary>
    /// Saves a biller to the user's quick pay list.
    /// </summary>
    /// <param name="savedBiller">The saved biller entity.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddSavedBillerAsync(SavedBiller savedBiller);

    /// <summary>
    /// Gets a specific account by ID.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>The account entity if found.</returns>
    Task<Account?> GetAccountByIdAsync(int accountId);

    /// <summary>
    /// Gets all accounts owned by a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user's accounts.</returns>
    Task<IEnumerable<Account>> GetAccountsByUserIdAsync(int userId);

    /// <summary>
    /// Updates an existing account's balance.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <returns>A task representing the operation.</returns>
    Task UpdateAccountAsync(Account account);

    /// <summary>
    /// Saves a new global transaction record.
    /// </summary>
    /// <param name="transaction">The transaction entity.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddTransactionAsync(Transaction transaction);
}