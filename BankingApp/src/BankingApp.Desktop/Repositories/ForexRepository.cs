// <copyright file="ForexRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ForexRepository class.
// </summary>

using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Repositories;

/// <summary>
///     Communicates with the exchange API endpoints through <see cref="IApiClient"/>.
///     Provides a domain-oriented interface so ViewModels never depend on HTTP details.
/// </summary>
public class ForexRepository : IForexRepository
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexRepository"/> class.
    /// </summary>
    /// <param name="apiClient">The HTTP API client.</param>
    public ForexRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ExchangeTransactionResponseDto>> GetRatePreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        string endpoint =
            $"{ApiEndpoints.ExchangePreview}?sourceCurrency={sourceCurrency}&targetCurrency={targetCurrency}&amount={amount}";
        return await _apiClient.GetAsync<ExchangeTransactionResponseDto>(endpoint);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ExchangeTransactionResponseDto>> ExecuteExchangeAsync(
        ExchangeTransactionRequestDto request)
    {
        return await _apiClient
            .PostAsync<ExchangeTransactionRequestDto, ExchangeTransactionResponseDto>(
                ApiEndpoints.ExchangeExecute, request);
    }
}
