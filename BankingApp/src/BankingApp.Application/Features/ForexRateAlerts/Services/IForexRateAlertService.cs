namespace BankingApp.Application.Features.ForexRateAlerts.Services;

using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using ErrorOr;

/// <summary>
///     Defines application-level operations for managing FX rate alerts.
/// </summary>
public interface IForexRateAlertService
{
    /// <summary>Retrieves all rate alerts for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of rate alert DTOs, or an error.</returns>
    public ErrorOr<List<ForexRateAlertDto>> GetAlerts(int userId);

    /// <summary>Creates a new rate alert for a currency pair.</summary>
    /// <param name="dto">The alert configuration.</param>
    /// <returns>The created alert DTO, or an error.</returns>
    public ErrorOr<ForexRateAlertDto> CreateAlert(ForexRateAlertDto dto);

    /// <summary>Removes a rate alert.</summary>
    /// <param name="id">The alert identifier.</param>
    /// <returns>Success, or an error when not found.</returns>
    public ErrorOr<Success> DeleteAlert(int id);

    /// <summary>Checks all untriggered alerts against current rates and triggers those that match.</summary>
    /// <returns>The number of alerts triggered, or an error.</returns>
    public ErrorOr<int> ProcessAlerts();
}
