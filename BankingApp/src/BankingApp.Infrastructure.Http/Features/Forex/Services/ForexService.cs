namespace BankingApp.Infrastructure.Http.Features.Forex.Services;

using Contracts.Features.Forex.Dtos;
using Contracts.Features.Forex.Services;
using Contracts.Http;
using ErrorOr;

public sealed class ForexService(IHttpClientFactory httpClientFactory) : IForexService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<ForexRatePreviewResponse>> GetPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<ForexRatePreviewResponse>(
            $"{ApiEndpoints.ForexPreview}?sourceCurrency={Uri.EscapeDataString(fromCurrency)}&targetCurrency={Uri.EscapeDataString(toCurrency)}&amount={amount}", ct);

    public Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(ForexTransactionRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<ForexTransactionRequest, ForexTransactionResponse>(ApiEndpoints.ForexExecute, request, ct);
}
