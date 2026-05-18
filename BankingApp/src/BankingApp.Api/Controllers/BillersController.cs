namespace BankingApp.Api.Controllers;

using Application.Features.Billers.Commands;
using Application.Features.Billers.Queries;
using Contracts.Features.Billers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBillers(CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new GetBillersQuery(), cancellationToken), Ok);
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSavedBillers(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetSavedBillersQuery(userId), cancellationToken), Ok);
    }

    [HttpPost("saved")]
    public async Task<IActionResult> SaveBiller([FromBody] SaveBillerRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new SaveBillerCommand(userId, request.BillerId, request.Nickname, request.DefaultReference);
        return ToActionResult(
            await Sender.Send(command, cancellationToken),
            data => CreatedAtAction(nameof(GetSavedBillers), data));
    }

    [HttpDelete("saved/{id:int}")]
    public async Task<IActionResult> RemoveSavedBiller(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new DeleteSavedBillerCommand(userId, id), cancellationToken));
    }
}
