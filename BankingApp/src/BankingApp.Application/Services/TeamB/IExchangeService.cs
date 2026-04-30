// <copyright file="IExchangeService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IExchangeService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for the FX currency exchange feature.
/// </summary>
public interface IExchangeService
{
    /// <summary>Returns a live exchange rate preview for the specified currency pair and amount.</summary>
    /// <param name="sourceCurrency">The ISO 4217 source currency code.</param>
    /// <param name="targetCurrency">The ISO 4217 target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>An <see cref="ExchangeTransactionResponseDto" /> with rate and preview amounts, or an error.</returns>
    ErrorOr<ExchangeTransactionResponseDto> GetRatePreview(string sourceCurrency, string targetCurrency, decimal amount);

    /// <summary>Executes a currency exchange between two of the user's accounts.</summary>
    /// <param name="request">The exchange request details.</param>
    /// <returns>The completed exchange transaction DTO, or an error.</returns>
    ErrorOr<ExchangeTransactionResponseDto> ExecuteExchange(ExchangeTransactionRequestDto request);

    /// <summary>Retrieves the exchange transaction history for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of exchange transaction DTOs, or an error.</returns>
    ErrorOr<List<ExchangeTransactionResponseDto>> GetExchangeHistory(int userId);
}
