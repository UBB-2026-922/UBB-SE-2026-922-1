namespace BankingApp.Api.Controllers;

using Application.Features.BillPayments.Commands;
using Application.Features.BillPayments.Queries;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles operations related to bill payments.
/// </summary>
[ApiController]
[Route(ApiEndpoints.BillPayments.Base)]
[Authorize]
public class BillPaymentsController : ApiControllerBase
{
    private const decimal LowTierFee = 0.50m;
    private const decimal HighTierFee = 1.00m;
    private const decimal FeeThreshold = 100m;

    /// <summary>
    /// Calculates the fee for a bill payment based on the amount.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>A response containing the calculated fee.</returns>
    [HttpGet(ApiEndpoints.BillPayments.Fee)]
    public IActionResult CalculateFee([FromQuery] decimal amount)
    {
        decimal fee = amount <= FeeThreshold ? LowTierFee : HighTierFee;
        return Ok(new FeeResponse { Fee = fee });
    }

    /// <summary>
    /// Checks whether two-factor authentication is required for a bill payment amount.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>A response indicating if 2FA is required.</returns>
    [HttpGet(ApiEndpoints.BillPayments.Requires2Fa)]
    public IActionResult Requires2Fa([FromQuery] decimal amount)
    {
        bool required = amount >= 1000m;
        return Ok(new RequiresTwoFaResponse { Required = required });
    }

    /// <summary>
    /// Processes a bill payment request.
    /// </summary>
    /// <param name="request">The bill payment request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the payment processing.</returns>
    [HttpPost(ApiEndpoints.BillPayments.Pay)]
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

    /// <summary>
    /// Retrieves the bill payment history for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The bill payment history.</returns>
    [HttpGet(ApiEndpoints.BillPayments.History)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetBillPaymentHistoryQuery(userId), cancellationToken), Ok);
    }
}
