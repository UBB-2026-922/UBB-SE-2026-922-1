namespace BankingApp.Infrastructure.Http.Features.BillPayments.Services;

using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class BillPaymentService(IHttpClientFactory httpClientFactory, ILogger<BillPaymentService> logger) : IBillPaymentService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<BillPaymentService> _logger = logger;

    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<AccountDto>>(ApiEndpoints.BillPayments.AccountsFull, _logger, ct);

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<FeeResponse>($"{ApiEndpoints.BillPayments.FeeFull}?amount={amount}", _logger, ct);

    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<RequiresTwoFaResponse>($"{ApiEndpoints.BillPayments.Requires2FaFull}?amount={amount}", _logger, ct);

    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayments.PayFull, request, _logger, ct);

    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<BillPayResponse>>(ApiEndpoints.BillPayments.HistoryFull, _logger, ct);
}
