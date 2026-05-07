namespace BankingApp.Desktop.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.RateAlerts;
using ErrorOr;

/// <summary>
///     Defines the desktop client boundary for rate-alert management.
/// </summary>
public interface IRateAlertClientService
{
    /// <summary>
    ///     Gets the authenticated user identifier cached by the client.
    /// </summary>
    public int? CurrentUserId { get; }

    /// <summary>
    ///     Loads all rate alerts for the specified user.
    /// </summary>
    public Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId);

    /// <summary>
    ///     Creates a new rate alert.
    /// </summary>
    public Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert);

    /// <summary>
    ///     Deletes an existing rate alert.
    /// </summary>
    public Task<ErrorOr<Success>> DeleteAlertAsync(int alertId);
}
