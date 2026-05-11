namespace BankingApp.Api.Controllers;

using BankingApp.Application.Features.Transfers.Commands;
using BankingApp.Application.Features.Transfers.Dtos;
using BankingApp.Application.Features.Transfers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/transfers")]
[Route("api/transfer")]
public class TransferController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new ExecuteTransferCommand(
            userId,
            request.SourceAccountId,
            request.RecipientName,
            request.RecipientIban,
            request.Amount,
            request.Currency,
            request.Reference,
            request.TwoFaToken);
        return ToActionResult(
            await Sender.Send(command, cancellationToken),
            transfer => CreatedAtAction(nameof(GetHistory), new { }, transfer));
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new ExecuteTransferCommand(
            userId,
            request.SourceAccountId,
            request.RecipientName,
            request.RecipientIban,
            request.Amount,
            request.Currency,
            request.Reference,
            request.TwoFaToken);
        return ToActionResult(
            await Sender.Send(command, cancellationToken),
            transfer => Ok(new TransferExecutionResponse { TransactionRef = transfer.TransactionRef ?? string.Empty }));
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetTransferHistoryQuery(userId), cancellationToken), Ok);
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetTransferAccountsQuery(userId), cancellationToken), Ok);
    }

    [HttpPost("validate-iban")]
    public async Task<IActionResult> ValidateIban([FromBody] TransferIbanValidationRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new ValidateIbanQuery(request.Iban), cancellationToken), Ok);
    }

    [HttpGet("fx-preview")]
    public async Task<IActionResult> GetFxPreview([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new GetForexPreviewQuery(from, to, amount), cancellationToken), Ok);
    }
}
