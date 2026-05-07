namespace BankingApp.Desktop.Services;

using System.Threading.Tasks;
using BankingApp.Application.Features.Forex.Dtos;
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
    public Task<ErrorOr<ForexTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount);

    /// <summary>
    ///     Executes a foreign-exchange transaction.
    /// </summary>
    public Task<ErrorOr<ForexTransactionResponse>> ExecuteExchangeAsync(ForexTransactionRequest request);
}
