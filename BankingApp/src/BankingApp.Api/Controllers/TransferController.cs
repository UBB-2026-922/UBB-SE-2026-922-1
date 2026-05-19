namespace BankingApp.Api.Controllers;

using Application.Features.Transfers.Commands;
using Application.Features.Transfers.Queries;
using Contracts.Features.Transfers.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.Transfers.Base)]
[Route(ApiEndpoints.Transfers.LegacyBase)]
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

    [HttpPost(ApiEndpoints.Transfers.Execute)]
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

    [HttpGet(ApiEndpoints.Transfers.Accounts)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetTransferAccountsQuery(userId), cancellationToken), Ok);
    }

    [HttpPost(ApiEndpoints.Transfers.ValidateIban)]
    public async Task<IActionResult> ValidateIban([FromBody] TransferIbanValidationRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new ValidateIbanQuery(request.Iban), cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Transfers.FxPreview)]
    public async Task<IActionResult> GetFxPreview([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new GetForexPreviewQuery(from, to, amount), cancellationToken), Ok);
    }
}
