namespace BankingApp.Desktop.Services;

using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Dashboard;
using ErrorOr;

/// <summary>
///     Defines the desktop client boundary for loading dashboard data.
/// </summary>
public interface IDashboardClientService
{
    /// <summary>
    ///     Loads the authenticated user's dashboard payload.
    /// </summary>
    public Task<ErrorOr<DashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
