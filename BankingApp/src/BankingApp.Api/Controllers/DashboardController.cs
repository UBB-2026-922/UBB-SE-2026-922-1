namespace BankingApp.Api.Controllers;

using Application.Features.AccountOverview.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Exposes the authenticated user's dashboard data.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetAccountOverviewQuery(userId), cancellationToken), Ok);
    }
}
