// <copyright file="DashboardRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardRepository class.
// </summary>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Provides repository operations for retrieving dashboard data.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly IAccountDataAccess _accountDataAccess;
    private readonly ICardDataAccess _cardDataAccess;
    private readonly INotificationDataAccess _notificationDataAccess;
    private readonly ITransactionDataAccess _transactionDataAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardRepository" /> class.
    /// </summary>
    /// <param name="accountDataAccess">The account data access component.</param>
    /// <param name="cardDataAccess">The card data access component.</param>
    /// <param name="transactionDataAccess">The transaction data access component.</param>
    /// <param name="notificationDataAccess">The notification data access component.</param>
    public DashboardRepository(
        IAccountDataAccess accountDataAccess,
        ICardDataAccess cardDataAccess,
        ITransactionDataAccess transactionDataAccess,
        INotificationDataAccess notificationDataAccess)
    {
        _accountDataAccess = accountDataAccess;
        _cardDataAccess = cardDataAccess;
        _transactionDataAccess = transactionDataAccess;
        _notificationDataAccess = notificationDataAccess;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<List<Account>> GetAccountsByUser(int userId)
    {
        return _accountDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<List<Card>> GetCardsByUser(int userId)
    {
        return _cardDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <param name="accountId">The accountId value.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transaction>> GetRecentTransactions(
        int accountId,
        int limit = IDashboardRepository.DefaultRecentTransactionLimit)
    {
        return _transactionDataAccess.FindRecentByAccountId(accountId, limit);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<int> GetUnreadNotificationCount(int userId)
    {
        return _notificationDataAccess.CountUnreadByUserId(userId);
    }
}