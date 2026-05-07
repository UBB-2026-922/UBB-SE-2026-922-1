namespace BankingApp.Desktop.Services;

using System.Threading.Tasks;
using Application.DTOs.Exchange;
using ErrorOr;

/// <summary>
///     Defines the desktop client boundary for foreign-exchange preview and execution flows.
/// </summary>
public interface IForexClientService
{
    /// <summary>
    ///     Gets the authenticated user identifier cached by the client.
    /// </summary>
    public int? CurrentUserId { get; }

    /// <summary>
    ///     Loads an exchange preview for the specified currencies and amount.
    /// </summary>
    public Task<ErrorOr<ExchangeTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount);

    /// <summary>
    ///     Executes a foreign-exchange transaction.
    /// </summary>
    public Task<ErrorOr<ExchangeTransactionResponse>> ExecuteExchangeAsync(ExchangeTransactionRequest request);
}
