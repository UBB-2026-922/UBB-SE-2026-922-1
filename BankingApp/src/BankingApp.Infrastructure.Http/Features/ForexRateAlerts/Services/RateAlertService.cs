namespace BankingApp.Infrastructure.Http.Features.ForexRateAlerts.Services;

using Contracts.Features.ForexRateAlerts.Dtos;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class RateAlertService(IHttpClientFactory httpClientFactory, ILogger<RateAlertService> logger) : IRateAlertService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<RateAlertService> _logger = logger;

    public Task<ErrorOr<List<ForexRateAlertDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<ForexRateAlertDto>>(ApiEndpoints.RateAlerts.Base, _logger, ct);

    public Task<ErrorOr<ForexRateAlertDto>> CreateAsync(ForexRateAlertDto alert, CancellationToken ct = default)
        => _http.PostErrorOrAsync<ForexRateAlertDto, ForexRateAlertDto>(ApiEndpoints.RateAlerts.Base, alert, _logger, ct);

    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync(ApiEndpoints.RateAlerts.ByIdFull(id), _logger, ct);
}
