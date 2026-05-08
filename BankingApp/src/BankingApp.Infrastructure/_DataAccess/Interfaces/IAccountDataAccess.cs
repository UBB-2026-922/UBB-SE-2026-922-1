namespace BankingApp.Infrastructure.DataAccess.Interfaces;

using Domain.Aggregates.AccountAggregate;
using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines data access operations for bank accounts.
/// </summary>
public interface IAccountDataAccess
{
    /// <summary>Finds all accounts belonging to the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of accounts owned by the user, or an error if the operation failed.</returns>
    public ErrorOr<List<Account>> FindByUserId(int userId);

    /// <summary>Finds an account by its unique identifier.</summary>
    /// <param name="id">The account identifier.</param>
    /// <returns>The matching <see cref="Account" />, or <see cref="Error.NotFound" /> if not found.</returns>
    public ErrorOr<Account> FindById(int id);

    /// <summary>Debits the specified amount from the account balance.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="amount">The amount to debit.</param>
    /// <returns><see cref="Result.Success" /> on success, or a failure error.</returns>
    public ErrorOr<Success> DebitAccount(int accountId, decimal amount);
}
