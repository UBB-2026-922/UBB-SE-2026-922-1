namespace BankingApp.Infrastructure.Http.Features.RecurringPayments.Services;

using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Http;
using ErrorOr;

public sealed class RecurringPaymentService(IHttpClientFactory httpClientFactory) : IRecurringPaymentService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<RecurringPaymentResponse>>(ApiEndpoints.RecurringPayments, ct);

    public Task<ErrorOr<RecurringPaymentResponse>> CreateAsync(CreateRecurringPaymentRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<CreateRecurringPaymentRequest, RecurringPaymentResponse>(ApiEndpoints.RecurringPayments, request, ct);

    public Task<ErrorOr<Success>> PauseAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync($"{ApiEndpoints.RecurringPayments}/{id}/pause", new { }, ct);

    public Task<ErrorOr<Success>> ResumeAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync($"{ApiEndpoints.RecurringPayments}/{id}/resume", new { }, ct);

    public Task<ErrorOr<Success>> CancelAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync($"{ApiEndpoints.RecurringPayments}/{id}", ct);
}
