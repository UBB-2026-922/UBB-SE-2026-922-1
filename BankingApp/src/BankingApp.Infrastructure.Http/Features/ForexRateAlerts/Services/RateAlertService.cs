namespace BankingApp.Infrastructure.Http.Features.ForexRateAlerts.Services;

using Application.Shared.Http;
using Contracts.Features.ForexRateAlerts.Dtos;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Http;
using ErrorOr;

public sealed class RateAlertService(IApiClient apiClient) : IRateAlertService
{
    public Task<ErrorOr<List<ForexRateAlertDto>>> GetAllAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<ForexRateAlertDto>>(ApiEndpoints.RateAlerts.Base, ct);

    public Task<ErrorOr<ForexRateAlertDto>> CreateAsync(ForexRateAlertDto alert, CancellationToken ct = default)
        => apiClient.PostAsync<ForexRateAlertDto, ForexRateAlertDto>(ApiEndpoints.RateAlerts.Base, alert, ct);

    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.RateAlerts.ByIdFull(id), ct);
}
