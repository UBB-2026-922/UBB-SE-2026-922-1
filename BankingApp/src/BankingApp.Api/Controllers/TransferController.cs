namespace BankingApp.Api.Controllers;

using Application.DTOs.Transfer;
using Application.Repositories.Interfaces;
using Application.Services.Transfers;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
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
    private readonly IDashboardRepository _dashboardRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferController" /> class.
    /// </summary>
    /// <param name="transferService">The transfer service used to handle business logic.</param>
    /// <param name="transferRepository">The transfer repository used by the raw proxy endpoints.</param>
    /// <param name="dashboardRepository">The dashboard repository for accounts.</param>
    public TransferController(ITransferService transferService, ITransferRepository transferRepository, IDashboardRepository dashboardRepository)
    {
        _transferService = transferService;
        _transferRepository = transferRepository;
        _dashboardRepository = dashboardRepository;
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
        var transfer = new Transfer
        {
            Amount = request.Amount,
            Currency = request.Currency,
            RecipientIban = request.RecipientIban,
            RecipientName = request.RecipientName,
            Reference = request.Reference,
            Status = TransferStatus.Pending
        };

        return ToActionResult(
            _transferRepository.Create(transfer),
            t => CreatedAtAction(nameof(GetHistory), new { }, t));
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
        // Route through Raw proxy logic simulating the transfer
        var transfer = new Transfer
        {
            Amount = request.Amount,
            Currency = request.Currency,
            RecipientIban = request.RecipientIban,
            RecipientName = request.RecipientName,
            Reference = request.Reference,
            Status = TransferStatus.Pending
        };

        return ToActionResult(
            _transferRepository.Create(transfer),
            t => Ok(new TransferExecutionResponse
            {
                TransactionRef = t.Transaction?.TransactionRef ?? string.Empty,
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

        ErrorOr<System.Collections.Generic.List<Transfer>> result = _transferRepository.GetByUserId(userId);
        if (result.IsError)
        {
            return ToActionResult(result.FirstError);
        }

        var responses = result.Value.Select(transfer => new TransferResponse
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccount?.Id ?? 0,
            TransactionId = transfer.Transaction?.Id,
            TransactionRef = transfer.Transaction?.TransactionRef ?? string.Empty,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            RecipientBankName = transfer.RecipientBankName,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            Fee = transfer.Fee,
            Reference = transfer.Reference,
            Status = transfer.Status,
            CreatedAt = transfer.CreatedAt
        }).ToList();

        return Ok(responses);
    }

    /// <summary>
    ///     Returns transfer-ready accounts for the authenticated user.
    /// </summary>
    /// <returns>The available source accounts.</returns>
    [HttpGet("accounts")]
    public IActionResult GetAccounts()
    {
        int userId = GetAuthenticatedUserId();

        ErrorOr<System.Collections.Generic.List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId);
        if (accountsResult.IsError)
        {
            return ToActionResult(accountsResult.FirstError);
        }

        var accounts = accountsResult.Value
            .Where(account => account.Status == AccountStatus.Active)
            .OrderBy(account => account.AccountName)
            .Select(account => new TransferAccountSelectionResponse
            {
                Id = account.Id,
                Iban = account.Iban,
                Currency = account.Currency,
                Balance = account.Balance,
                AccountName = account.AccountName ?? string.Empty
            })
            .ToList();

        return Ok(accounts);
    }

    /// <summary>
    ///     Validates a recipient IBAN and infers the bank name when possible.
    /// </summary>
    /// <param name="request">The IBAN validation request.</param>
    /// <returns>The validation result.</returns>
    [HttpPost("validate-iban")]
    public IActionResult ValidateIban([FromBody] TransferIbanValidationRequest request)
    {
        return Ok(new TransferIbanValidationResponse
        {
            IsValid = Transfer.IsValidRecipientIban(request.Iban),
            BankName = Transfer.IsValidRecipientIban(request.Iban) ? Transfer.InferRecipientBankName(request.Iban) : string.Empty
        });
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
