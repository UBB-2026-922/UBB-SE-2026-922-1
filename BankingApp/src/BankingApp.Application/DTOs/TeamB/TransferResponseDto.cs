// <copyright file="TransferResponseDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferResponseDto record.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the result data of a bank transfer operation.
/// </summary>
/// <param name="Id">The unique identifier of the transfer.</param>
/// <param name="RecipientName">The full name of the recipient.</param>
/// <param name="RecipientIban">The recipient's IBAN.</param>
/// <param name="Amount">The transferred amount.</param>
/// <param name="Currency">The ISO 4217 currency code.</param>
/// <param name="Fee">The fee charged for the transfer.</param>
/// <param name="Status">The current processing status.</param>
/// <param name="EstimatedArrival">The estimated arrival date at the recipient bank.</param>
/// <param name="CreatedAt">The timestamp when the transfer was created.</param>
public record TransferResponseDto(
    int Id,
    string RecipientName,
    string RecipientIban,
    decimal Amount,
    string Currency,
    decimal Fee,
    TransferStatus Status,
    DateTime? EstimatedArrival,
    DateTime CreatedAt);
