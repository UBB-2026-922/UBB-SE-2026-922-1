// <copyright file="IDashboardRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IDashboardRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines repository operations for retrieving dashboard data.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>The default maximum number of transactions to return.</summary>
    public const int DefaultRecentTransactionLimit = 10;

    /// <summary>Gets all accounts belonging to the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Account>> GetAccountsByUser(int userId);

    /// <summary>Gets all cards belonging to the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Card>> GetCardsByUser(int userId);

    /// <summary>Gets the most recent transactions for the specified account.</summary>
    /// <param name="accountId">The accountId value.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Transaction>> GetRecentTransactions(int accountId, int limit = DefaultRecentTransactionLimit);

    /// <summary>Gets the number of unread notifications for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<int> GetUnreadNotificationCount(int userId);

    /// <summary>
    /// Adds a new transfer request for the specified user and source account.
    /// </summary>
    /// <param name="transfer">The transfer to add. Required fields include <see cref="Transfer.UserId"/>, <see cref="Transfer.SourceAccountId"/>, and <see cref="Transfer.Amount"/>.</param>
    /// <returns>
    /// The added <see cref="Transfer"/> on success, or an <see cref="ErrorOr{T}"/> containing errors if the operation fails.
    /// </returns>
    ErrorOr<Transfer> AddTransfer(Transfer transfer);

    /// <summary>
    /// Gets all transfers created by the specified user.
    /// </summary>
    /// <param name="userId">The user identifier whose transfers will be returned.</param>
    /// <returns>A list of <see cref="Transfer"/> instances or errors.</returns>
    ErrorOr<List<Transfer>> GetTransfersByUserId(int userId);

    /// <summary>
    /// Debits the specified account by the given amount.
    /// </summary>
    /// <param name="accountId">The account identifier to debit.</param>
    /// <param name="amount">The amount to debit from the account.</param>
    /// <returns>
    /// A <see cref="Success"/> result on success wrapped in <see cref="ErrorOr{T}"/>, or errors on failure.
    /// </returns>
    ErrorOr<Success> DebitAccount(int accountId, decimal amount);

    /// <summary>
    /// Adds a transaction record.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    /// <returns>The added <see cref="Transaction"/> or errors if the operation fails.</returns>
    ErrorOr<Transaction> AddTransaction(Transaction transaction);
}