// <copyright file="ExchangeTransactionRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeTransactionRequestDto record.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to execute a currency exchange.
/// </summary>
/// <param name="UserId">The identifier of the user initiating the exchange.</param>
/// <param name="SourceAccountId">The identifier of the account debited in the source currency.</param>
/// <param name="TargetAccountId">The identifier of the account credited in the target currency.</param>
/// <param name="SourceCurrency">The ISO 4217 source currency code.</param>
/// <param name="TargetCurrency">The ISO 4217 target currency code.</param>
/// <param name="SourceAmount">The amount to convert from the source account.</param>
public record ExchangeTransactionRequestDto(
    int UserId,
    int SourceAccountId,
    int TargetAccountId,
    string SourceCurrency,
    string TargetCurrency,
    decimal SourceAmount);
