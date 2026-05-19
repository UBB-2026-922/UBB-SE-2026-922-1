namespace BankingApp.Infrastructure.Http.Features.Transfers.Services;

using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class TransferService(IHttpClientFactory httpClientFactory, ILogger<TransferService> logger) : ITransferService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<TransferService> _logger = logger;

    public Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<TransferResponse>>(ApiEndpoints.TransferHistory, _logger, ct);

    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<TransferAccountSelectionResponse>>(ApiEndpoints.TransferAccounts, _logger, ct);

    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(TransferIbanValidationRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<TransferIbanValidationRequest, TransferIbanValidationResponse>(ApiEndpoints.TransferValidateIban, request, _logger, ct);

    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<TransferForexPreviewResponse>(
            $"{ApiEndpoints.TransferFxPreview}?from={Uri.EscapeDataString(fromCurrency)}&to={Uri.EscapeDataString(toCurrency)}&amount={amount}", _logger, ct);

    public Task<ErrorOr<TransferExecutionResponse>> ExecuteAsync(CreateTransferRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<CreateTransferRequest, TransferExecutionResponse>(ApiEndpoints.TransferExecute, request, _logger, ct);
}
