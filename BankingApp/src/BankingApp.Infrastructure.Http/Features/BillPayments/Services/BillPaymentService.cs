namespace BankingApp.Infrastructure.Http.Features.BillPayments.Services;

using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Http;
using ErrorOr;

public sealed class BillPaymentService(IHttpClientFactory httpClientFactory) : IBillPaymentService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<AccountDto>>(ApiEndpoints.BillPayAccounts, ct);

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<FeeResponse>($"{ApiEndpoints.BillPayFee}?amount={amount}", ct);

    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount, CancellationToken ct = default)
        => _http.GetErrorOrAsync<RequiresTwoFaResponse>($"{ApiEndpoints.BillPayRequires2Fa}?amount={amount}", ct);

    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayPay, request, ct);

    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<BillPayResponse>>(ApiEndpoints.BillPayHistory, ct);
}
