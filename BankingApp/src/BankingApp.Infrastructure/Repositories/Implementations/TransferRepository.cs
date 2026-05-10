namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using DataAccess;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

/// <summary>
///     EF Core implementation of <see cref="ITransferRepository" />.
/// </summary>
public class TransferRepository : ITransferRepository
{
    private readonly AppDatabaseContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferRepository" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TransferRepository(AppDatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> GetById(int id)
    {
        try
        {
            Transfer? transfer = _context.Transfers
                .Include(transferRecord => transferRecord.User)
                .Include(transferRecord => transferRecord.SourceAccount)
                .Include(transferRecord => transferRecord.Transaction)
                .FirstOrDefault(transferRecord => transferRecord.Id == id);
            if (transfer is null)
            {
                return Error.NotFound("Transfer.NotFound", $"Transfer with id {id} was not found.");
            }

            return transfer;
        }
        catch (Exception exception)
        {
            return Error.Failure("Transfer.GetById.Failed", exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<List<Transfer>> GetByUserId(int userId)
    {
        try
        {
            var transfers = _context.Transfers
                .Include(transferRecord => transferRecord.User)
                .Include(transferRecord => transferRecord.SourceAccount)
                .Include(transferRecord => transferRecord.Transaction)
                .Where(transferRecord => EF.Property<int>(transferRecord, "UserId") == userId)
                .OrderByDescending(transferRecord => transferRecord.CreatedAt)
                .ToList();

            return transfers;
        }
        catch (Exception exception)
        {
            return Error.Failure("Transfer.GetByUserId.Failed", exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> Create(Transfer transfer)
    {
        try
        {
            if (transfer.User?.Id is int userId)
            {
                User? user = _context.Users.Find(userId);
                if (user is null)
                {
                    return Error.NotFound("Transfer.UserNotFound", $"User with id {userId} was not found.");
                }

                transfer.User = user;
            }

            if (transfer.SourceAccount?.Id is int sourceAccountId)
            {
                Account? sourceAccount = _context.Accounts.Find(sourceAccountId);
                if (sourceAccount is null)
                {
                    return Error.NotFound(
                        "Transfer.SourceAccountNotFound",
                        $"Account with id {sourceAccountId} was not found.");
                }

                transfer.SourceAccount = sourceAccount;
            }

            if (transfer.Transaction?.Id is int transactionId)
            {
                Transaction? transaction = _context.Transactions.Find(transactionId);
                if (transaction is null)
                {
                    return Error.NotFound(
                        "Transfer.TransactionNotFound",
                        $"Transaction with id {transactionId} was not found.");
                }

                transfer.Transaction = transaction;
            }

            _context.Transfers.Add(transfer);
            _context.SaveChanges();
            return transfer;
        }
        catch (Exception exception)
        {
            return Error.Failure("Transfer.Create.Failed", exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> UpdateStatus(int transferId, TransferStatus status)
    {
        try
        {
            Transfer? transfer = _context.Transfers.FirstOrDefault(transferRecord => transferRecord.Id == transferId);
            if (transfer is null)
            {
                return Error.NotFound("Transfer.NotFound", $"Transfer with id {transferId} was not found.");
            }

            transfer.Status = status;
            _context.SaveChanges();
            return transfer;
        }
        catch (Exception exception)
        {
            return Error.Failure("Transfer.UpdateStatus.Failed", exception.Message);
        }
    }
}
