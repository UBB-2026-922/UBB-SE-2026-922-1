namespace BankingApp.Api.Controllers;

using BankingApp.Application.Features.ForexRateAlerts.Commands;
using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using BankingApp.Application.Features.ForexRateAlerts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/exchange/rate-alerts")]
public class RateAlertsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetRateAlertsQuery(userId), cancellationToken), Ok);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] ForexRateAlertDto request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new CreateRateAlertCommand(
            userId,
            request.BaseCurrency,
            request.TargetCurrency,
            request.TargetRate,
            request.IsBuyAlert);
        return ToActionResult(await Sender.Send(command, cancellationToken), Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAlert(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new DeleteRateAlertCommand(userId, id), cancellationToken));
    }
}
