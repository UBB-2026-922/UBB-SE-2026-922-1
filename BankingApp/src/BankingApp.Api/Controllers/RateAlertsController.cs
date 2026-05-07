namespace BankingApp.Api.Controllers;

using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using BankingApp.Application.Features.ForexRateAlerts.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller for exchange rate-alert operations.
/// </summary>
[Authorize]
[Route("api/exchange/rate-alerts")]
public class RateAlertsController : ApiControllerBase
{
    private readonly IForexRateAlertService _rateAlertService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertsController" /> class.
    /// </summary>
    /// <param name="rateAlertService">The rate-alert application service.</param>
    public RateAlertsController(IForexRateAlertService rateAlertService)
    {
        _rateAlertService = rateAlertService;
    }

    /// <summary>
    ///     Returns all alerts for the authenticated user.
    /// </summary>
    /// <returns>The user's alerts.</returns>
    [HttpGet]
    public IActionResult GetAlerts()
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<List<ForexRateAlertDto>> result = _rateAlertService.GetAlerts(userId);
        return ToActionResult(result, Ok);
    }

    /// <summary>
    ///     Creates a new alert for the authenticated user.
    /// </summary>
    /// <param name="request">The alert request.</param>
    /// <returns>The created alert.</returns>
    [HttpPost]
    public IActionResult CreateAlert([FromBody] ForexRateAlertDto request)
    {
        request.UserId = GetAuthenticatedUserId();
        ErrorOr<ForexRateAlertDto> result = _rateAlertService.CreateAlert(request);
        return ToActionResult(result, Ok);
    }

    /// <summary>
    ///     Deletes an alert that belongs to the authenticated user.
    /// </summary>
    /// <param name="id">The alert identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")]
    public IActionResult DeleteAlert(int id)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<List<ForexRateAlertDto>> alertsResult = _rateAlertService.GetAlerts(userId);
        if (alertsResult.IsError)
        {
            return MapError(alertsResult.FirstError);
        }

        if (alertsResult.Value.All(alert => alert.Id != id))
        {
            return NotFound(new { error = "Rate alert not found." });
        }

        ErrorOr<Success> result = _rateAlertService.DeleteAlert(id);
        return ToActionResult(result);
    }
}
