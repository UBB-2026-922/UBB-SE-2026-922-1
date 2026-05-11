namespace BankingApp.Desktop.Services;

using System;
using System.Threading.Tasks;
using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Application.Common.Utilities;
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
    public Task<ErrorOr<ForexTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        string endpoint =
            $"{ApiEndpoints.ExchangePreview}?sourceCurrency={sourceCurrency}&targetCurrency={targetCurrency}&amount={amount}";
        return _apiClient.GetAsync<ForexTransactionResponse>(endpoint);
    }

    /// <inheritdoc />
    public Task<ErrorOr<ForexTransactionResponse>> ExecuteExchangeAsync(ForexTransactionRequest request)
    {
        return _apiClient.PostAsync<ForexTransactionRequest, ForexTransactionResponse>(
            ApiEndpoints.ExchangeExecute, request);
    }
}
