namespace BankingApp.Application.Features.ForexRateAlerts.Repositories;

using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines persistence operations for the <see cref="RateAlert" /> entity.
/// </summary>
public interface IForexRateAlertRepository
{
    /// <summary>Retrieves a rate alert by its unique identifier.</summary>
    /// <param name="id">The rate alert identifier.</param>
    /// <returns>The matching <see cref="RateAlert" />, or an error when not found.</returns>
    public ErrorOr<RateAlert> GetById(int id);

    /// <summary>Retrieves all rate alerts belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of rate alerts, or an error.</returns>
    public ErrorOr<List<RateAlert>> GetByUserId(int userId);

    /// <summary>Retrieves all alerts that have not yet been triggered, for background processing.</summary>
    /// <returns>A list of untriggered alerts, or an error.</returns>
    public ErrorOr<List<RateAlert>> GetUntriggeredAlerts();

    /// <summary>Persists a new rate alert.</summary>
    /// <param name="alert">The alert to create.</param>
    /// <returns>The created alert with its assigned identifier, or an error.</returns>
    public ErrorOr<RateAlert> Create(RateAlert alert);

    /// <summary>Sets the IsTriggered flag to true on the specified alert.</summary>
    /// <param name="alertId">The identifier of the alert to mark as triggered.</param>
    /// <returns>The updated alert, or an error.</returns>
    public ErrorOr<RateAlert> MarkTriggered(int alertId);

    /// <summary>Removes a rate alert.</summary>
    /// <param name="id">The identifier of the alert to remove.</param>
    /// <returns>Success, or an error when the record is not found.</returns>
    public ErrorOr<Success> Delete(int id);
}
