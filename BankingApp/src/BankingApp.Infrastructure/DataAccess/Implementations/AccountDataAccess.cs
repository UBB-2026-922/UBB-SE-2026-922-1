namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides SQL Server data access for bank account records.
/// </summary>
public class AccountDataAccess : IAccountDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public AccountDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Account> FindById(int id)
    {
        Account? account = _databaseContext.Accounts.FirstOrDefault(a => a.Id == id);
        if (account == null)
        {
            return Error.NotFound(description: "Account not found.");
        }

        return account;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Account>> FindByUserId(int userId)
    {
        var accounts = _databaseContext.Accounts.Where(a => EF.Property<int>(a, "UserId") == userId).ToList();
        return accounts;
    }

    /// <inheritdoc />
    /// <param name="accountId">The accountId value.</param>
    /// <param name="amount">The amount value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> DebitAccount(int accountId, decimal amount)
    {
        try
        {
            Account? account = _databaseContext.Accounts.Find(accountId);
            if (account is null)
            {
                return Error.NotFound(description: "Account not found.");
            }

            account.Balance -= amount;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
