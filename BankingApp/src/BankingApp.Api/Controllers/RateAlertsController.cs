// <copyright file="RateAlertsController.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertsController class.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Services.TeamB;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller for exchange rate-alert operations.
/// </summary>
[Authorize]
[Route("api/exchange/rate-alerts")]
public class RateAlertsController : ApiControllerBase
{
    private readonly IRateAlertService _rateAlertService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertsController" /> class.
    /// </summary>
    /// <param name="rateAlertService">The rate-alert application service.</param>
    public RateAlertsController(IRateAlertService rateAlertService)
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
        ErrorOr<List<RateAlertDto>> result = _rateAlertService.GetAlerts(userId);
        return ToActionResult(result, data => Ok(data));
    }

    /// <summary>
    ///     Creates a new alert for the authenticated user.
    /// </summary>
    /// <param name="request">The alert request.</param>
    /// <returns>The created alert.</returns>
    [HttpPost]
    public IActionResult CreateAlert([FromBody] RateAlertDto request)
    {
        request.UserId = GetAuthenticatedUserId();
        ErrorOr<RateAlertDto> result = _rateAlertService.CreateAlert(request);
        return ToActionResult(result, data => Ok(data));
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
        ErrorOr<List<RateAlertDto>> alertsResult = _rateAlertService.GetAlerts(userId);
        if (alertsResult.IsError)
        {
            return MapError(alertsResult.FirstError);
        }

        if (!alertsResult.Value.Any(alert => alert.Id == id))
        {
            return NotFound(new { error = "Rate alert not found." });
        }

        ErrorOr<Success> result = _rateAlertService.DeleteAlert(id);
        return ToActionResult(result);
    }
}
