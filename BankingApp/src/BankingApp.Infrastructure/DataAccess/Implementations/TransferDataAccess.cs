// <copyright file="TransferDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferDataAccess class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

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
        Transfer? transfer = _databaseContext.Transfers.FirstOrDefault(t => t.Id == transferId);
        if (transfer is null)
        {
            return Error.NotFound(description: "Transfer not found.");
        }

        return transfer;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Transfer>> FindByUserId(int userId)
    {
        List<Transfer> transfers = _databaseContext.Transfers
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
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
            Transfer? transfer = _databaseContext.Transfers.FirstOrDefault(t => t.Id == transferId);
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