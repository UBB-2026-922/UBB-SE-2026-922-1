using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Dashboard;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class DashboardClientService : IDashboardClientService
{
    private readonly IApiClient _apiClient;

    public DashboardClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public Task<ErrorOr<DashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<DashboardDto>(ApiEndpoints.Dashboard, cancellationToken);
    }
}
