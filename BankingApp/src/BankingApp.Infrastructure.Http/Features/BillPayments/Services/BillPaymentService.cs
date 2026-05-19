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
        => _http.GetErrorOrAsync<List<AccountDto>>(ApiEndpoints.BillPayAccounts, _logger, ct);

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<FeeResponse>($"{ApiEndpoints.BillPayFee}?amount={amount}", _logger, ct);

    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<RequiresTwoFaResponse>($"{ApiEndpoints.BillPayRequires2Fa}?amount={amount}", _logger, ct);

    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayPay, request, _logger, ct);

    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<BillPayResponse>>(ApiEndpoints.BillPayHistory, _logger, ct);
}
