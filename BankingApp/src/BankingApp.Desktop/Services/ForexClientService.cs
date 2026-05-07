namespace BankingApp.Desktop.Services;

using System;
using System.Threading.Tasks;
using Application.DTOs.Exchange;
using Utilities;
using ErrorOr;

/// <summary>
///     Implements <see cref="IForexClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class ForexClientService : IForexClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexClientService" /> class.
    /// </summary>
    public ForexClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public int? CurrentUserId => _apiClient.CurrentUserId;

    /// <inheritdoc />
    public Task<ErrorOr<ExchangeTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        string endpoint =
            $"{ApiEndpoints.ExchangePreview}?sourceCurrency={sourceCurrency}&targetCurrency={targetCurrency}&amount={amount}";
        return _apiClient.GetAsync<ExchangeTransactionResponse>(endpoint);
    }

    /// <inheritdoc />
    public Task<ErrorOr<ExchangeTransactionResponse>> ExecuteExchangeAsync(ExchangeTransactionRequest request)
    {
        return _apiClient.PostAsync<ExchangeTransactionRequest, ExchangeTransactionResponse>(
            ApiEndpoints.ExchangeExecute, request);
    }
}
