namespace BankingApp.Infrastructure.Http.Features.AccountOverview.Services;

using Contracts.Features.AccountOverview.Dtos;
using Contracts.Features.AccountOverview.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class DashboardService(IHttpClientFactory httpClientFactory, ILogger<DashboardService> logger) : IDashboardService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<DashboardService> _logger = logger;

    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<AccountOverviewDto>(ApiEndpoints.Dashboard, _logger, ct);
}
