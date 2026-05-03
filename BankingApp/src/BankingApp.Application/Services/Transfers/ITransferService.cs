// <copyright file="ITransferService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ITransferService interface.
// </summary>

using BankingApp.Application.DataTransferObjects.Transfer;
using BankingApp.Application.DTOs.Transfer;
using ErrorOr;

namespace BankingApp.Application.Services.Transfers;

/// <summary>
///     Defines operations for creating and querying bank transfers.
/// </summary>
public interface ITransferService
{
    /// <summary>
    ///     Validates, authorizes, and executes a transfer for the specified user.
    /// </summary>
    /// <param name="request">The transfer creation request.</param>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>
    ///     A <see cref="TransferResponse" /> on success,
    ///     or a validation, authorization, or failure error.
    /// </returns>
    ErrorOr<TransferResponse> CreateTransfer(CreateTransferRequest request, int userId);

    /// <summary>
    ///     Returns all transfers initiated by the specified user, newest first.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>
    ///     A list of <see cref="TransferResponse" /> on success,
    ///     or a failure error.
    /// </returns>
    ErrorOr<List<TransferResponse>> GetHistory(int userId);
}