namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Domain.Enums;
using Interfaces;
using ErrorOr;

/// <summary>
///     Provides EF Core data access for transfer records.
/// </summary>
public class TransferDataAccess : ITransferDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context.</param>
    public TransferDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="transfer">The transfer value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Transfer> Add(Transfer transfer)
    {
        try
        {
            _databaseContext.Transfers.Add(transfer);
            _databaseContext.SaveChanges();
            return transfer;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="transferId">The transferId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Transfer> FindById(int transferId)
    {
        Transfer? transfer = _databaseContext.Transfers.FirstOrDefault(transfer => transfer.Id == transferId);
        return transfer ?? (ErrorOr<Transfer>)Error.NotFound(description: "Transfer not found.");
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transfer>> FindByUserId(int userId)
    {
        var transfers = _databaseContext.Transfers
            .Where(transfer => transfer.UserId == userId)
            .OrderByDescending(transfer => transfer.CreatedAt)
            .ToList();
        return transfers;
    }

    /// <inheritdoc />
    /// <param name="transferId">The transferId value.</param>
    /// <param name="status">The status value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdateStatus(int transferId, TransferStatus status)
    {
        try
        {
            Transfer? transfer = _databaseContext.Transfers.FirstOrDefault(transfer => transfer.Id == transferId);
            if (transfer is null)
            {
                return Error.NotFound(description: "Transfer not found.");
            }

            transfer.Status = status;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
