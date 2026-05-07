using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.RateAlerts;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IRateAlertClientService
{
    int? CurrentUserId { get; }

    Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId);

    Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert);

    Task<ErrorOr<Success>> DeleteAlertAsync(int alertId);
}
