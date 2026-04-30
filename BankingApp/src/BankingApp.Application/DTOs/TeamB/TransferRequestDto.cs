// <copyright file="TransferRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferRequestDto record.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to initiate a bank transfer.
/// </summary>
/// <param name="UserId">The identifier of the user initiating the transfer.</param>
/// <param name="SourceAccountId">The identifier of the account to debit.</param>
/// <param name="RecipientName">The full name of the recipient.</param>
/// <param name="RecipientIban">The recipient's IBAN.</param>
/// <param name="RecipientBankName">The recipient's bank name (optional).</param>
/// <param name="Amount">The amount to transfer.</param>
/// <param name="Currency">The ISO 4217 currency code.</param>
/// <param name="Reference">An optional free-text payment reference.</param>
public record TransferRequestDto(
    int UserId,
    int SourceAccountId,
    string RecipientName,
    string RecipientIban,
    string? RecipientBankName,
    decimal Amount,
    string Currency,
    string? Reference);
