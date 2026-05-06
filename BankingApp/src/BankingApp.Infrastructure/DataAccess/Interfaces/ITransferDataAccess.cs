using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
///     Defines data access operations for transfer records.
/// </summary>
public interface ITransferDataAccess
{
    /// <summary>Adds a new transfer record.</summary>
    /// <param name="transfer">The transfer to add.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Transfer> Add(Transfer transfer);

    /// <summary>Gets a transfer by its identifier.</summary>
    /// <param name="transferId">The transfer identifier.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Transfer> FindById(int transferId);

    /// <summary>Gets all transfers for a user, newest first.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Transfer>> FindByUserId(int userId);

    /// <summary>Updates the status of a transfer.</summary>
    /// <param name="transferId">The transfer identifier.</param>
    /// <param name="status">The new status.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdateStatus(int transferId, TransferStatus status);
}
