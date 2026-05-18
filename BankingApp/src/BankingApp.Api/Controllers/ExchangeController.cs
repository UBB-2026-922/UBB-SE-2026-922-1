namespace BankingApp.Api.Controllers;

using Application.Features.Forex.Commands;
using Application.Features.Forex.Dtos;
using Application.Features.Forex.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/exchange")]
public class ExchangeController : ApiControllerBase
{
    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview(
        [FromQuery] string sourceCurrency,
        [FromQuery] string targetCurrency,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var query = new GetRatePreviewQuery(userId, sourceCurrency, targetCurrency, amount);
        return ToActionResult(await Sender.Send(query, cancellationToken), Ok);
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ForexTransactionRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new ExecuteForexCommand(
            userId,
            request.SourceAccountId,
            request.TargetAccountId,
            request.SourceCurrency,
            request.TargetCurrency,
            request.SourceAmount);
        return ToActionResult(await Sender.Send(command, cancellationToken), Ok);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetForexHistoryQuery(userId), cancellationToken), Ok);
    }
}
