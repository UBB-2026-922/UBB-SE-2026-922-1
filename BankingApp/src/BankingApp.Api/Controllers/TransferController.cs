namespace BankingApp.Api.Controllers;

using Application.DTOs.Transfer;
using Application.Repositories.Interfaces;
using Application.Services.Transfers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller responsible for handling transfer-related operations.
///     All endpoints are accessible under the /api/transfer route.
/// </summary>
[ApiController]
[Authorize]
[Route("api/transfers")]
[Route("api/transfer")]
public class TransferController : ApiControllerBase
{
    private readonly ITransferRepository _transferRepository;
    private readonly ITransferService _transferService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferController" /> class.
    /// </summary>
    /// <param name="transferService">The transfer service used to handle business logic.</param>
    /// <param name="transferRepository">The transfer repository used by the raw proxy endpoints.</param>
    public TransferController(ITransferService transferService, ITransferRepository transferRepository)
    {
        _transferService = transferService;
        _transferRepository = transferRepository;
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
    ///     Creates a new transfer for the authenticated user using the desktop transfer wizard contract.
    /// </summary>
    /// <param name="request">The transfer execution request.</param>
    /// <returns>A compact response containing the transaction reference.</returns>
    [HttpPost("execute")]
    public IActionResult ExecuteTransfer([FromBody] CreateTransferRequest request)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            _transferService.CreateTransfer(request, userId),
            transfer => Ok(new TransferExecutionResponse
            {
                TransactionRef = transfer.TransactionRef ?? string.Empty,
            }));
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
            _transferService.GetHistory(userId), Ok);
    }

    /// <summary>
    ///     Returns transfer-ready accounts for the authenticated user.
    /// </summary>
    /// <returns>The available source accounts.</returns>
    [HttpGet("accounts")]
    public IActionResult GetAccounts()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            _transferService.GetAvailableAccounts(userId),
            Ok);
    }

    /// <summary>
    ///     Validates a recipient IBAN and infers the bank name when possible.
    /// </summary>
    /// <param name="request">The IBAN validation request.</param>
    /// <returns>The validation result.</returns>
    [HttpPost("validate-iban")]
    public IActionResult ValidateIban([FromBody] TransferIbanValidationRequest request)
    {
        return ToActionResult(
            _transferService.ValidateRecipientIban(request.Iban),
            Ok);
    }

    /// <summary>
    ///     Returns an FX preview for a transfer amount and currency pair.
    /// </summary>
    /// <param name="from">The source currency code.</param>
    /// <param name="to">The target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>The FX preview result.</returns>
    [HttpGet("fx-preview")]
    public IActionResult GetFxPreview([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
    {
        return ToActionResult(
            _transferService.GetFxPreview(from, to, amount), Ok);
    }

    /// <summary>
    ///     Returns raw transfers for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/{userId:int}")]
    public IActionResult GetTransfersByUserIdRaw(int userId)
    {
        return ToActionResult(_transferRepository.GetByUserId(userId), Ok);
    }

    /// <summary>
    ///     Returns a raw transfer by identifier.
    /// </summary>
    [HttpGet("raw/id/{id:int}")]
    public IActionResult GetTransferByIdRaw(int id)
    {
        return ToActionResult(_transferRepository.GetById(id), Ok);
    }

    /// <summary>
    ///     Persists a raw transfer entity.
    /// </summary>
    [HttpPost("raw")]
    public IActionResult CreateTransferRaw([FromBody] Transfer transfer)
    {
        return ToActionResult(_transferRepository.Create(transfer), Ok);
    }

    /// <summary>
    ///     Updates the status of a raw transfer entity.
    /// </summary>
    [HttpPut("raw/{transferId:int}/status")]
    public IActionResult UpdateTransferStatusRaw(int transferId, [FromBody] UpdateTransferStatusRequest request)
    {
        return ToActionResult(_transferRepository.UpdateStatus(transferId, request.Status), Ok);
    }

    /// <summary>
    ///     Request body for raw transfer-status updates.
    /// </summary>
    public sealed class UpdateTransferStatusRequest
    {
        /// <summary>
        ///     Gets or sets the new transfer status.
        /// </summary>
        public TransferStatus Status { get; set; }
    }
}
