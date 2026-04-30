// <copyright file="ITransferRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ITransferRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines persistence operations for the <see cref="Transfer" /> entity.
/// </summary>
public interface ITransferRepository
{
    /// <summary>Retrieves a transfer by its unique identifier.</summary>
    /// <param name="id">The transfer identifier.</param>
    /// <returns>The matching <see cref="Transfer" />, or an error when not found.</returns>
    ErrorOr<Transfer> GetById(int id);

    /// <summary>Retrieves all transfers belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of transfers, or an error.</returns>
    ErrorOr<List<Transfer>> GetByUserId(int userId);

    /// <summary>Persists a new transfer record.</summary>
    /// <param name="transfer">The transfer to create.</param>
    /// <returns>The created transfer with its assigned identifier, or an error.</returns>
    ErrorOr<Transfer> Create(Transfer transfer);

    /// <summary>Updates the processing status of an existing transfer.</summary>
    /// <param name="transferId">The identifier of the transfer to update.</param>
    /// <param name="status">The new status value.</param>
    /// <returns>The updated transfer, or an error.</returns>
    ErrorOr<Transfer> UpdateStatus(int transferId, TransferStatus status);
}
