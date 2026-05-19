namespace BankingApp.Infrastructure.Http.Features.RecurringPayments.Services;

using Application.Shared.Http;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Http;
using ErrorOr;

public sealed class RecurringPaymentService(IApiClient apiClient) : IRecurringPaymentService
{
    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetAllAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<RecurringPaymentResponse>>(ApiEndpoints.RecurringPayments.Base, ct);

    public Task<ErrorOr<RecurringPaymentResponse>> CreateAsync(CreateRecurringPaymentRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateRecurringPaymentRequest, RecurringPaymentResponse>(ApiEndpoints.RecurringPayments.Base, request, ct);

    public Task<ErrorOr<Success>> PauseAsync(int id, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.RecurringPayments.PauseFull(id), new { }, ct);

    public Task<ErrorOr<Success>> ResumeAsync(int id, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.RecurringPayments.ResumeFull(id), new { }, ct);

    public Task<ErrorOr<Success>> CancelAsync(int id, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.RecurringPayments.ByIdFull(id), ct);
}
