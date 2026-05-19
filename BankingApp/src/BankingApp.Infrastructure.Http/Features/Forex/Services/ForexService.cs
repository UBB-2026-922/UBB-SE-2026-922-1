namespace BankingApp.Infrastructure.Http.Features.Forex.Services;

using Contracts.Features.Forex.Dtos;
using Contracts.Features.Forex.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class ForexService(IHttpClientFactory httpClientFactory, ILogger<ForexService> logger) : IForexService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<ForexService> _logger = logger;

    public Task<ErrorOr<ForexRatePreviewResponse>> GetPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<ForexRatePreviewResponse>(
            $"{ApiEndpoints.ForexPreview}?sourceCurrency={Uri.EscapeDataString(fromCurrency)}&targetCurrency={Uri.EscapeDataString(toCurrency)}&amount={amount}", _logger, ct);

    public Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(ForexTransactionRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<ForexTransactionRequest, ForexTransactionResponse>(ApiEndpoints.ForexExecute, request, _logger, ct);
}
