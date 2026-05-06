namespace BankingApp.Api.Controllers;

using Application.DTOs.Dashboard;
using Application.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

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
    ///     200 OK with a <see cref="DashboardDto" /> on success,
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetDashboard()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_dashboardService.GetDashboardData(userId), Ok);
    }
}