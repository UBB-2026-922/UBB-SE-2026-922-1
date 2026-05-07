using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Exchange;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class ForexClientService : IForexClientService
{
    private readonly IApiClient _apiClient;

    public ForexClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public int? CurrentUserId => _apiClient.CurrentUserId;

    public Task<ErrorOr<ExchangeTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        string endpoint =
            $"{ApiEndpoints.ExchangePreview}?sourceCurrency={sourceCurrency}&targetCurrency={targetCurrency}&amount={amount}";
        return _apiClient.GetAsync<ExchangeTransactionResponse>(endpoint);
    }

    public Task<ErrorOr<ExchangeTransactionResponse>> ExecuteExchangeAsync(ExchangeTransactionRequest request)
    {
        return _apiClient.PostAsync<ExchangeTransactionRequest, ExchangeTransactionResponse>(
            ApiEndpoints.ExchangeExecute, request);
    }
}
