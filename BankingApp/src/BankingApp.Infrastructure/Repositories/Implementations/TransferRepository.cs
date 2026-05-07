namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using DataAccess;
using ErrorOr;

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
            Transfer? transfer = _context.Transfers.FirstOrDefault(t => t.Id == id);
            if (transfer is null)
            {
                return Error.NotFound("Transfer.NotFound", $"Transfer with id {id} was not found.");
            }

            return transfer;
        }
        catch (Exception ex)
        {
            return Error.Failure("Transfer.GetById.Failed", ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<List<Transfer>> GetByUserId(int userId)
    {
        try
        {
            var transfers = _context.Transfers
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return transfers;
        }
        catch (Exception ex)
        {
            return Error.Failure("Transfer.GetByUserId.Failed", ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> Create(Transfer transfer)
    {
        try
        {
            _context.Transfers.Add(transfer);
            _context.SaveChanges();
            return transfer;
        }
        catch (Exception ex)
        {
            return Error.Failure("Transfer.Create.Failed", ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> UpdateStatus(int transferId, TransferStatus status)
    {
        try
        {
            Transfer? transfer = _context.Transfers.FirstOrDefault(t => t.Id == transferId);
            if (transfer is null)
            {
                return Error.NotFound("Transfer.NotFound", $"Transfer with id {transferId} was not found.");
            }

            transfer.Status = status;
            _context.SaveChanges();
            return transfer;
        }
        catch (Exception ex)
        {
            return Error.Failure("Transfer.UpdateStatus.Failed", ex.Message);
        }
    }
}
