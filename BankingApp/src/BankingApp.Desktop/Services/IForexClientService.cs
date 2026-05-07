using System.Threading.Tasks;
using BankingApp.Application.DTOs.Exchange;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IForexClientService
{
    int? CurrentUserId { get; }

    Task<ErrorOr<ExchangeTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount);

    Task<ErrorOr<ExchangeTransactionResponse>> ExecuteExchangeAsync(ExchangeTransactionRequest request);
}
