// <copyright file="ITransferService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ITransferService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for the bank transfer feature.
/// </summary>
public interface ITransferService
{
    /// <summary>Initiates a new outbound bank transfer.</summary>
    /// <param name="request">The transfer details.</param>
    /// <returns>The created transfer response DTO, or an error.</returns>
    ErrorOr<TransferResponseDto> InitiateTransfer(TransferRequestDto request);

    /// <summary>Retrieves the transfer history for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of transfer response DTOs, or an error.</returns>
    ErrorOr<List<TransferResponseDto>> GetTransferHistory(int userId);
}