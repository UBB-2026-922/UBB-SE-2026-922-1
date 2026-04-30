// <copyright file="BillPaymentResponseDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentResponseDto record.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the result data of a bill payment operation.
/// </summary>
/// <param name="Id">The unique identifier of the payment.</param>
/// <param name="BillerName">The display name of the biller that was paid.</param>
/// <param name="BillerReference">The biller-specific reference used for this payment.</param>
/// <param name="Amount">The amount paid.</param>
/// <param name="Fee">The processing fee charged.</param>
/// <param name="ReceiptNumber">The unique receipt number issued upon confirmation.</param>
/// <param name="Status">The current processing status.</param>
/// <param name="CreatedAt">The timestamp when the payment was created.</param>
public record BillPaymentResponseDto(
    int Id,
    string BillerName,
    string BillerReference,
    decimal Amount,
    decimal Fee,
    string ReceiptNumber,
    BillPaymentStatus Status,
    DateTime CreatedAt);
