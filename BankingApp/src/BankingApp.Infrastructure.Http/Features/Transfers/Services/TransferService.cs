namespace BankingApp.Infrastructure.Http.Features.Transfers.Services;

using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using Contracts.Http;
using ErrorOr;

public sealed class TransferService(IHttpClientFactory httpClientFactory) : ITransferService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<TransferResponse>>(ApiEndpoints.TransferHistory, ct);

    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<TransferAccountSelectionResponse>>(ApiEndpoints.TransferAccounts, ct);

    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(TransferIbanValidationRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<TransferIbanValidationRequest, TransferIbanValidationResponse>(ApiEndpoints.TransferValidateIban, request, ct);

    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<TransferForexPreviewResponse>(
            $"{ApiEndpoints.TransferFxPreview}?from={Uri.EscapeDataString(fromCurrency)}&to={Uri.EscapeDataString(toCurrency)}&amount={amount}", ct);

    public Task<ErrorOr<TransferExecutionResponse>> ExecuteAsync(CreateTransferRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<CreateTransferRequest, TransferExecutionResponse>(ApiEndpoints.TransferExecute, request, ct);
}
