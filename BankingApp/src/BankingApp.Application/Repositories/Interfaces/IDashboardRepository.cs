namespace BankingApp.Application.Repositories.Interfaces;

using Domain.Entities;
using ErrorOr;

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
    public ErrorOr<List<Account>> GetAccountsByUser(int userId);

    /// <summary>Gets all cards belonging to the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Card>> GetCardsByUser(int userId);

    /// <summary>Gets the most recent transactions for the specified account.</summary>
    /// <param name="accountId">The accountId value.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transaction>> GetRecentTransactions(int accountId, int limit = DefaultRecentTransactionLimit);

    /// <summary>Gets the number of unread notifications for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<int> GetUnreadNotificationCount(int userId);

    /// <summary>
    /// Debits the specified account by the given amount.
    /// </summary>
    /// <param name="accountId">The account identifier to debit.</param>
    /// <param name="amount">The amount to debit from the account.</param>
    /// <returns>
    /// A <see cref="Success"/> result on success wrapped in <see cref="ErrorOr{T}"/>, or errors on failure.
    /// </returns>
    public ErrorOr<Success> DebitAccount(int accountId, decimal amount);

    /// <summary>
    /// Adds a transaction record.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    /// <returns>The added <see cref="Transaction"/> or errors if the operation fails.</returns>
    public ErrorOr<Transaction> AddTransaction(Transaction transaction);
}
