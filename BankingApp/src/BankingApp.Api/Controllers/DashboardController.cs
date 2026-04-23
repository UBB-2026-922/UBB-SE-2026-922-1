// <copyright file="DashboardController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardController class.
// </summary>

using BankingApp.Application.DataTransferObjects.Dashboard;
using BankingApp.Application.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller responsible for handling dashboard-related operations.
///     All endpoints are accessible under the /api/dashboard route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardController" /> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service used to handle business logic.</param>
    /// <returns>The result of the operation.</returns>
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    ///     Retrieves the dashboard data for the currently authenticated user.
    ///     The user ID is extracted from the HTTP context, set by the authentication middleware.
    /// </summary>
    /// <returns>
    ///     200 OK with a <see cref="DashboardResponse" /> on success,
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetDashboard()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_dashboardService.GetDashboardData(userId), data => Ok(data));
    }
}