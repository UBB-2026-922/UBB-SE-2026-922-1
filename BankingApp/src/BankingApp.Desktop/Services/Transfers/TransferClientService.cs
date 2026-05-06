using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Beneficiaries;
using BankingApp.Application.DTOs.Transfer;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services.Transfers;

/// <summary>
///     Encapsulates transfer-related HTTP orchestration for the desktop client.
/// </summary>
public sealed class TransferClientService : ITransferClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferClientService" /> class.
    /// </summary>
    public TransferClientService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<List<TransferAccountSelectionResponse>>(ApiEndpoints.TransferAccounts, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ErrorOr<TransferExecutionResponse>> ExecuteTransferAsync(
        int sourceAccountId,
        string recipientName,
        string recipientIban,
        decimal amount,
        string currency,
        string? twoFaToken)
    {
        var request = new CreateTransferRequest
        {
            SourceAccountId = sourceAccountId,
            RecipientName = recipientName,
            RecipientIban = recipientIban,
            Amount = amount,
            Currency = currency,
            TwoFaToken = twoFaToken
        };
        return _apiClient.PostAsync<CreateTransferRequest, TransferExecutionResponse>(ApiEndpoints.TransferExecute, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(string iban)
    {
        return _apiClient.PostAsync<object, TransferIbanValidationResponse>(
            ApiEndpoints.TransferValidateIban,
            new { Iban = iban });
    }

    /// <inheritdoc />
    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(
        string fromCurrency,
        string toCurrency,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        string endpoint = $"{ApiEndpoints.TransferFxPreview}?from={fromCurrency}&to={toCurrency}&amount={amount}";
        return _apiClient.GetAsync<TransferForexPreviewResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<TransferResponse>>> GetTransferHistoryAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<List<TransferResponse>>(ApiEndpoints.TransferHistory, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<BeneficiaryDto>>> GetBeneficiariesAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<List<BeneficiaryDto>>(ApiEndpoints.Beneficiaries, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> AddBeneficiaryAsync(string name, string iban, string bankName)
    {
        var request = new CreateBeneficiaryRequest
        {
            Name = name,
            Iban = iban,
            BankName = bankName
        };
        return _apiClient.PostAsync(ApiEndpoints.Beneficiaries, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> DeleteBeneficiaryAsync(int beneficiaryId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.Beneficiaries}/{beneficiaryId}");
    }
}
