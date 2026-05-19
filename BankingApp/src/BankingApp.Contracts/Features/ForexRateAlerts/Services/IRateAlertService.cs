namespace BankingApp.Contracts.Features.ForexRateAlerts.Services;

using Dtos;
using ErrorOr;

public interface IRateAlertService
{
    public Task<ErrorOr<List<ForexRateAlertDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<ErrorOr<ForexRateAlertDto>> CreateAsync(ForexRateAlertDto alert, CancellationToken ct = default);
    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default);
}
