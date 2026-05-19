namespace BankingApp.Api.Controllers;

using Application.Features.Cards.Commands;
using Application.Features.Cards.Queries;
using Contracts.Features.Cards.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.Cards.Base)]
public class CardsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new GetCardsQuery(userId), cancellationToken),
            Ok);
    }

    [HttpPut(ApiEndpoints.Cards.Freeze)]
    public async Task<IActionResult> Freeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new FreezeCardCommand(userId, id), cancellationToken));
    }

    [HttpPut(ApiEndpoints.Cards.Unfreeze)]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new UnfreezeCardCommand(userId, id), cancellationToken));
    }

    [HttpDelete(ApiEndpoints.Cards.ById)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new CancelCardCommand(userId, id), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Issue([FromBody] IssueCardRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new IssueCardCommand(userId, request.CardType, request.CardBrand);
        return ToActionResult(await Sender.Send(command, cancellationToken), Ok);
    }
}
