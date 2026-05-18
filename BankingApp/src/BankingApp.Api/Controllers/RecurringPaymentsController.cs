namespace BankingApp.Api.Controllers;

using Application.Features.RecurringPayments.Commands;
using Application.Features.RecurringPayments.Dtos;
using Application.Features.RecurringPayments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/recurring_payments")]
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

    [HttpPut("{id}/pause")]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new PauseRecurringPaymentCommand(userId, id), cancellationToken));
    }

    [HttpPut("{id}/resume")]
    public async Task<IActionResult> ResumePayment(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new ResumeRecurringPaymentCommand(userId, id), cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new CancelRecurringPaymentCommand(userId, id), cancellationToken));
    }
}
