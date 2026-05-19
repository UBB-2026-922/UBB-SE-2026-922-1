namespace BankingApp.Api.Controllers;

using Application.Features.RecurringPayments.Commands;
using Application.Features.RecurringPayments.Queries;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.RecurringPayments.Base)]
public class RecurringPaymentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetRecurringPaymentsQuery(userId), cancellationToken), Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringPaymentRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new CreateRecurringPaymentCommand(
            userId,
            request.BillerId,
            request.SourceAccountId,
            request.Amount,
            request.IsPayInFull,
            request.Frequency,
            request.StartDate,
            request.EndDate);
        return ToActionResult(
            await Sender.Send(command, cancellationToken),
            payment => CreatedAtAction(nameof(GetAll), new { }, payment));
    }

    [HttpPut(ApiEndpoints.RecurringPayments.Pause)]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new PauseRecurringPaymentCommand(userId, id), cancellationToken));
    }

    [HttpPut(ApiEndpoints.RecurringPayments.Resume)]
    public async Task<IActionResult> ResumePayment(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new ResumeRecurringPaymentCommand(userId, id), cancellationToken));
    }

    [HttpDelete(ApiEndpoints.RecurringPayments.ById)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new CancelRecurringPaymentCommand(userId, id), cancellationToken));
    }
}
