namespace BankingApp.Infrastructure.Http.Features.ForexRateAlerts.Services;

using Contracts.Features.ForexRateAlerts.Dtos;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Http;
using ErrorOr;

public sealed class RateAlertService(IHttpClientFactory httpClientFactory) : IRateAlertService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<ForexRateAlertDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<ForexRateAlertDto>>(ApiEndpoints.RateAlerts, ct);

    public Task<ErrorOr<ForexRateAlertDto>> CreateAsync(ForexRateAlertDto alert, CancellationToken ct = default)
        => _http.PostErrorOrAsync<ForexRateAlertDto, ForexRateAlertDto>(ApiEndpoints.RateAlerts, alert, ct);

    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync($"{ApiEndpoints.RateAlerts}/{id}", ct);
}
