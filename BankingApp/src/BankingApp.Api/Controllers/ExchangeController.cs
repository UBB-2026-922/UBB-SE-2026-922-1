// <copyright file="ExchangeController.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeController class.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.TeamB;
using BankingApp.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller for exchange previews, execution, and history.
/// </summary>
[Authorize]
[Route("api/exchange")]
public class ExchangeController : ApiControllerBase
{
    private readonly IBillPaymentRepository _billPaymentRepository;
    private readonly IExchangeService _exchangeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeController" /> class.
    /// </summary>
    /// <param name="exchangeService">The exchange application service.</param>
    /// <param name="billPaymentRepository">The repository used to infer source and target accounts.</param>
    public ExchangeController(IExchangeService exchangeService, IBillPaymentRepository billPaymentRepository)
    {
        _exchangeService = exchangeService;
        _billPaymentRepository = billPaymentRepository;
    }

    /// <summary>
    ///     Returns a preview for the requested currency pair and amount and locks the rate for the user.
    /// </summary>
    /// <param name="sourceCurrency">The source currency code.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>The preview DTO.</returns>
    [HttpGet("preview")]
    public IActionResult GetPreview(
        [FromQuery] string sourceCurrency,
        [FromQuery] string targetCurrency,
        [FromQuery] decimal amount)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<ExchangeTransactionResponseDto> result = _exchangeService.GetRatePreview(sourceCurrency, targetCurrency, amount);
        if (result.IsError)
        {
            return MapError(result.FirstError);
        }

        ErrorOr<LockedRate> lockResult = _exchangeService.LockRate(userId, sourceCurrency, targetCurrency);
        if (lockResult.IsError)
        {
            return MapError(lockResult.FirstError);
        }

        return Ok(result.Value);
    }

    /// <summary>
    ///     Executes a currency exchange for the authenticated user.
    /// </summary>
    /// <param name="request">The exchange request.</param>
    /// <returns>The completed exchange DTO.</returns>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ExchangeTransactionRequestDto request)
    {
        int userId = GetAuthenticatedUserId();
        request.UserId = userId;

        if (request.SourceAccountId <= 0 || request.TargetAccountId <= 0)
        {
            var accounts = (await _billPaymentRepository.GetAccountsByUserIdAsync(userId)).ToList();
            request.SourceAccountId = request.SourceAccountId > 0
                ? request.SourceAccountId
                : accounts.FirstOrDefault(account => string.Equals(account.Currency, request.SourceCurrency, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            request.TargetAccountId = request.TargetAccountId > 0
                ? request.TargetAccountId
                : accounts.FirstOrDefault(account => string.Equals(account.Currency, request.TargetCurrency, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }

        if (request.SourceAccountId > 0 || request.TargetAccountId > 0)
        {
            var accounts = (await _billPaymentRepository.GetAccountsByUserIdAsync(userId)).ToList();
            bool hasSourceAccount = accounts.Any(account => account.Id == request.SourceAccountId);
            bool hasTargetAccount = accounts.Any(account => account.Id == request.TargetAccountId);
            if (!hasSourceAccount || !hasTargetAccount)
            {
                return NotFound(new { error = "The selected exchange accounts do not belong to the authenticated user." });
            }
        }

        if (request.SourceAccountId <= 0 || request.TargetAccountId <= 0)
        {
            return NotFound(new { error = "Matching source and target accounts were not found for the requested currencies." });
        }

        ErrorOr<ExchangeTransactionResponseDto> result = _exchangeService.ExecuteExchange(request);
        return ToActionResult(result, data => Ok(data));
    }

    /// <summary>
    ///     Returns the exchange history for the authenticated user.
    /// </summary>
    /// <returns>The user's exchange history.</returns>
    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<List<ExchangeTransactionResponseDto>> result = _exchangeService.GetExchangeHistory(userId);
        return ToActionResult(result, data => Ok(data));
    }
}
