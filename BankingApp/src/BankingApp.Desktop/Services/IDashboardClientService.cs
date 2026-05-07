using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Dashboard;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IDashboardClientService
{
    Task<ErrorOr<DashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
