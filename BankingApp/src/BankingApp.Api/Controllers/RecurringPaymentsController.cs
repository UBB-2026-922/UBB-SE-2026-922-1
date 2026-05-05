// <copyright file="RecurringPaymentsController.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentsController class.
// </summary>

using BankingApp.Application.DTOs.RecurringPayments;
using BankingApp.Application.Services.RecurringPayments;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller responsible for managing recurring payment schedules.
///     All endpoints are accessible under the /api/recurringpayments route
///     and require an authenticated session.
/// </summary>
[ApiController]
[Route("api/recurringpayments")]
public class RecurringPaymentsController : ApiControllerBase
{
    private readonly IRecurringPaymentService _recurringPaymentService;

    /// <summary>Initializes a new instance of the <see cref="RecurringPaymentsController" /> class.</summary>
    /// <param name="recurringPaymentService">The recurring payment service.</param>
    public RecurringPaymentsController(IRecurringPaymentService recurringPaymentService)
    {
        _recurringPaymentService = recurringPaymentService;
    }

    /// <summary>
    ///     Returns all recurring payment schedules owned by the authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a list of <see cref="RecurringPaymentResponse" /> on success,
    ///     or an appropriate error response.
    /// </returns>
    [HttpGet]
    public IActionResult GetAll()
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<List<RecurringPaymentResponse>> result = _recurringPaymentService.GetByUser(userId);
        return ToActionResult(result, payments => Ok(payments));
    }

    /// <summary>
    ///     Creates a new recurring payment schedule for the authenticated user.
    /// </summary>
    /// <param name="request">The creation request containing schedule details.</param>
    /// <returns>
    ///     201 Created with the new <see cref="RecurringPaymentResponse" /> on success,
    ///     400 Bad Request if validation fails,
    ///     or an appropriate error response.
    /// </returns>
    [HttpPost]
    public IActionResult Create([FromBody] CreateRecurringPaymentRequest request)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<RecurringPaymentResponse> result = _recurringPaymentService.Create(userId, request);
        return ToActionResult(result, payment => CreatedAtAction(nameof(GetAll), new { }, payment));
    }

    /// <summary>
    ///     Pauses an active recurring payment schedule.
    /// </summary>
    /// <param name="id">The identifier of the recurring payment to pause.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     404 Not Found if the schedule does not exist,
    ///     403 Forbidden if the schedule belongs to another user,
    ///     or an appropriate error response.
    /// </returns>
    [HttpPut("{id}/pause")]
    public IActionResult Pause(int id)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<Success> result = _recurringPaymentService.Pause(userId, id);
        return ToActionResult(result);
    }

    /// <summary>
    ///     Resumes a paused recurring payment schedule.
    /// </summary>
    /// <param name="id">The identifier of the recurring payment to resume.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     404 Not Found if the schedule does not exist,
    ///     403 Forbidden if the schedule belongs to another user,
    ///     or an appropriate error response.
    /// </returns>
    [HttpPut("{id}/resume")]
    public IActionResult Resume(int id)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<Success> result = _recurringPaymentService.Resume(userId, id);
        return ToActionResult(result);
    }

    /// <summary>
    ///     Permanently cancels a recurring payment schedule.
    /// </summary>
    /// <param name="id">The identifier of the recurring payment to cancel.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     404 Not Found if the schedule does not exist,
    ///     403 Forbidden if the schedule belongs to another user,
    ///     or an appropriate error response.
    /// </returns>
    [HttpDelete("{id}")]
    public IActionResult Cancel(int id)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<Success> result = _recurringPaymentService.Cancel(userId, id);
        return ToActionResult(result);
    }
}
