namespace BankingApp.Api.Controllers;

using BankingApp.Application.Features.BillPayments.Commands;
using BankingApp.Application.Features.BillPayments.Dtos;
using BankingApp.Application.Features.BillPayments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/bill-payment")]
[Authorize]
public class BillPaymentsController : ApiControllerBase
{
    private const decimal LowTierFee = 0.50m;
    private const decimal HighTierFee = 1.00m;
    private const decimal FeeThreshold = 100m;

    [HttpGet("fee")]
    public IActionResult CalculateFee([FromQuery] decimal amount)
    {
        decimal fee = amount <= FeeThreshold ? LowTierFee : HighTierFee;
        return Ok(new FeeResponse { Fee = fee });
    }

    [HttpGet("requires-2fa")]
    public IActionResult Requires2Fa([FromQuery] decimal amount)
    {
        bool required = amount >= 1000m;
        return Ok(new RequiresTwoFaResponse { Required = required });
    }

    [HttpPost("pay")]
    public async Task<IActionResult> ProcessPayment([FromBody] BillPayRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new ProcessBillPaymentCommand(
            userId,
            request.SourceAccountId,
            request.BillerId,
            request.BillerReference,
            request.Amount,
            request.TwoFaToken);
        return ToActionResult(await Sender.Send(command, cancellationToken), Ok);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetBillPaymentHistoryQuery(userId), cancellationToken), Ok);
    }
}
