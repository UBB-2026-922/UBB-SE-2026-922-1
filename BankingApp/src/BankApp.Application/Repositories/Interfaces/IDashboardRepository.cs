// <copyright file="IDashboardRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IDashboardRepository interface.
// </summary>

using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Application.Repositories.Interfaces;

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
}