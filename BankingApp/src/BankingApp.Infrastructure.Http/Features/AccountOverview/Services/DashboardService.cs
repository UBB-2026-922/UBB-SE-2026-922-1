namespace BankingApp.Infrastructure.Http.Features.AccountOverview.Services;

using Contracts.Features.AccountOverview.Dtos;
using Contracts.Features.AccountOverview.Services;
using Contracts.Http;
using ErrorOr;

public sealed class DashboardService(IHttpClientFactory httpClientFactory) : IDashboardService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<AccountOverviewDto>(ApiEndpoints.Dashboard, ct);
}
