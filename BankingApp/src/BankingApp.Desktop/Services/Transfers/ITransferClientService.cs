namespace BankingApp.Desktop.Services.Transfers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Beneficiaries;
using Application.DTOs.Transfer;
using ErrorOr;

/// <summary>
///     Defines the desktop client-service boundary for transfer and beneficiary workflows.
/// </summary>
public interface ITransferClientService
{
    /// <summary>
    ///     Loads the authenticated user's selectable source accounts.
    /// </summary>
    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Submits a transfer for execution.
    /// </summary>
    public Task<ErrorOr<TransferExecutionResponse>> ExecuteTransferAsync(
        int sourceAccountId,
        string recipientName,
        string recipientIban,
        decimal amount,
        string currency,
        string? twoFaToken);

    /// <summary>
    ///     Validates a recipient IBAN and returns the inferred bank details.
    /// </summary>
    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(string iban);

    /// <summary>
    ///     Loads a transfer FX preview for the given currencies and amount.
    /// </summary>
    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(
        string fromCurrency,
        string toCurrency,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Loads the authenticated user's transfer history.
    /// </summary>
    public Task<ErrorOr<List<TransferResponse>>> GetTransferHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Loads the authenticated user's saved beneficiaries.
    /// </summary>
    public Task<ErrorOr<List<BeneficiaryDto>>> GetBeneficiariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new beneficiary.
    /// </summary>
    public Task<ErrorOr<Success>> AddBeneficiaryAsync(string name, string iban, string bankName);

    /// <summary>
    ///     Deletes an existing beneficiary.
    /// </summary>
    public Task<ErrorOr<Success>> DeleteBeneficiaryAsync(int beneficiaryId);
}
