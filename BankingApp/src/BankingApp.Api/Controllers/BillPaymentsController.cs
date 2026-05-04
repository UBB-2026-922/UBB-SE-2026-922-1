// <copyright file="BillPaymentsController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentsController class.
// </summary>

using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Services.BillPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Controller for managing and processing bill payments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Presupunem că doar utilizatorii autentificați pot plăti facturi
public class BillPaymentsController : ControllerBase
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
            var billers = await _billPaymentService.GetAllBillersAsync();
            return Ok(billers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Processes a new bill payment transaction.
    /// </summary>
    /// <param name="request">The payment request details.</param>
    /// <returns>The processed bill payment record.</returns>
    [HttpPost("pay")]
    public async Task<IActionResult> ProcessPayment([FromBody] BillPaymentDto request)
    {
        try
        {
            var payment = await _billPaymentService.ProcessPaymentAsync(request);
            return Ok(payment);
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
            var success = await _billPaymentService.SaveBillerForUserAsync(
                request.UserId,
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
}