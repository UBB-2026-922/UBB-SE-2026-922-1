// <copyright file="BillPaymentRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentRequestDto record.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to process a one-off bill payment.
/// </summary>
/// <param name="UserId">The identifier of the paying user.</param>
/// <param name="SourceAccountId">The identifier of the account to debit.</param>
/// <param name="BillerId">The identifier of the target biller.</param>
/// <param name="BillerReference">The biller-specific reference (e.g., invoice or contract number).</param>
/// <param name="Amount">The amount to pay.</param>
public record BillPaymentRequestDto(
    int UserId,
    int SourceAccountId,
    int BillerId,
    string BillerReference,
    decimal Amount);
