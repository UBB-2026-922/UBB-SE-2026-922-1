namespace BankingApp.Application.Services.Exchange;

using BankingApp.Application.DTOs.Exchange;
using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines application-level operations for the FX currency exchange feature.
/// </summary>
public interface IExchangeService
{
    /// <summary>Returns a live exchange rate preview for the specified currency pair and amount.</summary>
    /// <param name="sourceCurrency">The ISO 4217 source currency code.</param>
    /// <param name="targetCurrency">The ISO 4217 target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>An <see cref="ExchangeTransactionResponse" /> with rate and preview amounts, or an error.</returns>
    public ErrorOr<ExchangeTransactionResponse>
        GetRatePreview(string sourceCurrency, string targetCurrency, decimal amount);

    /// <summary>Locks the current exchange rate for the authenticated user.</summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="sourceCurrency">The source currency code.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <returns>The locked rate details, or an error.</returns>
    public ErrorOr<LockedRate> LockRate(int userId, string sourceCurrency, string targetCurrency);

    /// <summary>Executes a currency exchange between two of the user's accounts.</summary>
    /// <param name="request">The exchange request details.</param>
    /// <returns>The completed exchange transaction DTO, or an error.</returns>
    public ErrorOr<ExchangeTransactionResponse> ExecuteExchange(ExchangeTransactionRequest request);

    /// <summary>Retrieves the exchange transaction history for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of exchange transaction DTOs, or an error.</returns>
    public ErrorOr<List<ExchangeTransactionResponse>> GetExchangeHistory(int userId);
}
