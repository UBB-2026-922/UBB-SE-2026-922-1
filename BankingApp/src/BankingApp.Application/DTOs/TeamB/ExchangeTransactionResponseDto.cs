// <copyright file="ExchangeTransactionResponseDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeTransactionResponseDto record.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the result data of a currency exchange operation or rate preview.
/// </summary>
/// <param name="Id">The unique identifier of the exchange (0 for previews).</param>
/// <param name="SourceCurrency">The ISO 4217 source currency code.</param>
/// <param name="TargetCurrency">The ISO 4217 target currency code.</param>
/// <param name="SourceAmount">The amount deducted from the source account.</param>
/// <param name="TargetAmount">The amount credited to the target account.</param>
/// <param name="ExchangeRate">The applied exchange rate.</param>
/// <param name="Commission">The commission charged for this exchange.</param>
/// <param name="Status">The processing status.</param>
/// <param name="CreatedAt">The timestamp when this exchange was created.</param>
public record ExchangeTransactionResponseDto(
    int Id,
    string SourceCurrency,
    string TargetCurrency,
    decimal SourceAmount,
    decimal TargetAmount,
    decimal ExchangeRate,
    decimal Commission,
    ExchangeTransactionStatus Status,
    DateTime CreatedAt);
