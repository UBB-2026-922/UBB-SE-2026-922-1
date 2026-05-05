// <copyright file="IForexRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IForexRepository interface.
// </summary>

using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Desktop.Repositories;

/// <summary>
///     Defines the data-access contract for currency exchange operations
///     on the client side. Decouples ViewModels from the HTTP transport layer.
/// </summary>
public interface IForexRepository
{
    /// <summary>
    ///     Fetches a rate preview for the given currency pair and amount.
    /// </summary>
    /// <param name="sourceCurrency">ISO 4217 source currency code.</param>
    /// <param name="targetCurrency">ISO 4217 target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>The exchange preview, or an error if the request fails.</returns>
    Task<ErrorOr<ExchangeTransactionResponseDto>> GetRatePreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount);

    /// <summary>
    ///     Executes a currency exchange transaction.
    /// </summary>
    /// <param name="request">The exchange request details.</param>
    /// <returns>The completed exchange response, or an error if the request fails.</returns>
    Task<ErrorOr<ExchangeTransactionResponseDto>> ExecuteExchangeAsync(
        ExchangeTransactionRequestDto request);
}
