namespace BankingApp.Desktop.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Features.AccountOverview.Dtos;
using BankingApp.Application.Common.Utilities;
using ErrorOr;

/// <summary>
///     Implements <see cref="IDashboardClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class DashboardClientService : IDashboardClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardClientService" /> class.
    /// </summary>
    public DashboardClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<AccountOverviewDto>(ApiEndpoints.Dashboard, cancellationToken);
    }
}
