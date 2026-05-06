namespace BankingApp.Api.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.BillPayment;
using Application.DTOs.BillPayments;
using Application.Services.BillPayments;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing and processing bill payments.
/// </summary>
[ApiController]
[Route("api/bill-payment")]
[Authorize]
public class BillPaymentsController : ApiControllerBase
{
    private readonly IBillPaymentService _billPaymentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillPaymentsController"/> class.
    /// </summary>
    /// <param name="billPaymentService">The bill payment service.</param>
    public BillPaymentsController(IBillPaymentService billPaymentService)
    {
        _billPaymentService = billPaymentService;
    }

    /// <summary>
    /// Retrieves all available billers from the system.
    /// </summary>
    /// <returns>A list of billers.</returns>
    [HttpGet("billers")]
    public async Task<IActionResult> GetBillers()
    {
        try
        {
            IEnumerable<Biller> billers = await _billPaymentService.GetAllBillersAsync();
            return Ok(billers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves accounts available for the authenticated user's bill payments.
    /// </summary>
    /// <returns>The user's accounts.</returns>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            IEnumerable<Account> accounts = await _billPaymentService.GetAccountsForUserAsync(userId);
            return Ok(accounts.Select(MapAccount).ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calculates the processing fee for a payment amount.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>The calculated fee.</returns>
    [HttpGet("fee")]
    public IActionResult CalculateFee([FromQuery] decimal amount)
    {
        return Ok(new FeeResponseDto { Fee = _billPaymentService.CalculateFee(amount) });
    }

    /// <summary>
    /// Checks whether the supplied payment amount requires 2FA.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>The 2FA requirement response.</returns>
    [HttpGet("requires-2fa")]
    public IActionResult Requires2Fa([FromQuery] decimal amount)
    {
        return Ok(new Requires2FaResponseDto { Required = _billPaymentService.Requires2Fa(amount) });
    }

    /// <summary>
    /// Processes a new bill payment transaction.
    /// </summary>
    /// <param name="request">The payment request details.</param>
    /// <returns>The processed bill payment record.</returns>
    [HttpPost("pay")]
    public async Task<IActionResult> ProcessPayment([FromBody] BillPayRequestDto request)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            BillPayment payment = await _billPaymentService.ProcessPaymentAsync(new BillPaymentDto
            {
                UserId = userId,
                SourceAccountId = request.SourceAccountId,
                BillerId = request.BillerId,
                BillerReference = request.BillerReference,
                Amount = request.Amount,
                IsPayInFull = request.IsPayInFull,
                TwoFaToken = request.TwoFaToken,
            });

            return Ok(new BillPayResponseDto
            {
                Id = payment.Id,
                ReceiptNumber = payment.ReceiptNumber,
                Fee = payment.Fee,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Saves a specific biller to the user's quick-pay list.
    /// </summary>
    /// <param name="request">The save biller request details.</param>
    /// <returns>A success message.</returns>
    [HttpPost("save-biller")]
    public async Task<IActionResult> SaveBiller([FromBody] SaveBillerDto request)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            bool success = await _billPaymentService.SaveBillerForUserAsync(
                userId,
                request.BillerId,
                request.Nickname);

            if (success)
            {
                return Ok(new { message = "Biller saved successfully." });
            }

            return BadRequest(new { error = "Failed to save the biller." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static AccountDto MapAccount(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Iban = account.Iban,
            Currency = account.Currency,
            Balance = account.Balance,
            AccountName = account.AccountName ?? string.Empty,
            Status = account.Status.ToString(),
        };
    }
}