// <copyright file="TransferController.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferController class.
// </summary>

using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.Services.Transfers;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller responsible for handling transfer-related operations.
///     All endpoints are accessible under the /api/transfer route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransferController : ApiControllerBase
{
    private readonly ITransferService _transferService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferController" /> class.
    /// </summary>
    /// <param name="transferService">The transfer service used to handle business logic.</param>
    public TransferController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    ///     Creates a new transfer for the currently authenticated user.
    /// </summary>
    /// <param name="request">The transfer creation request.</param>
    /// <returns>
    ///     201 Created with a <see cref="TransferResponse" /> on success,
    ///     or an error response if validation, authorization, or persistence fails.
    /// </returns>
    [HttpPost]
    public IActionResult CreateTransfer([FromBody] CreateTransferRequest request)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            _transferService.CreateTransfer(request, userId),
            transfer => CreatedAtAction(nameof(GetHistory), new { }, transfer));
    }

    /// <summary>
    ///     Retrieves the transfer history for the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a list of <see cref="TransferResponse" /> on success,
    ///     or an error response if the operation fails.
    /// </returns>
    [HttpGet]
    public IActionResult GetHistory()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            _transferService.GetHistory(userId),
            history => Ok(history));
    }
}
