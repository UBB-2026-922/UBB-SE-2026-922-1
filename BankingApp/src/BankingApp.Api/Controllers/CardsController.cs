namespace BankingApp.Api.Controllers;

using BankingApp.Application.Features.Cards.Commands;
using BankingApp.Application.Features.Cards.Dtos;
using BankingApp.Application.Features.Cards.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/cards")]
public class CardsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new GetCardsQuery(userId), cancellationToken),
            cards => Ok(cards));
    }

    [HttpPut("{id}/freeze")]
    public async Task<IActionResult> Freeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new FreezeCardCommand(userId, id), cancellationToken));
    }

    [HttpPut("{id}/unfreeze")]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new UnfreezeCardCommand(userId, id), cancellationToken));
    }

    [HttpDelete("{id}")]
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
        return ToActionResult(await Sender.Send(command, cancellationToken), card => Ok(card));
    }
}
