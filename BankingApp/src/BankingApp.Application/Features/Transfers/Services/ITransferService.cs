namespace BankingApp.Application.Features.Transfers.Services;

using BankingApp.Application.Features.Transfers.Dtos;
using ErrorOr;

/// <summary>
///     Defines operations for creating and querying bank transfers.
/// </summary>
public interface ITransferService
{
    /// <summary>
    ///     Returns transfer-ready accounts for the authenticated user.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>
    ///     A list of <see cref="TransferAccountSelectionResponse" /> on success,
    ///     or a failure error.
    /// </returns>
    public ErrorOr<List<TransferAccountSelectionResponse>> GetAvailableAccounts(int userId);

    /// <summary>
    ///     Validates a recipient IBAN and infers the bank name when valid.
    /// </summary>
    /// <param name="iban">The IBAN to validate.</param>
    /// <returns>
    ///     A <see cref="TransferIbanValidationResponse" /> describing the result.
    /// </returns>
    public ErrorOr<TransferIbanValidationResponse> ValidateRecipientIban(string iban);

    /// <summary>
    ///     Returns an FX preview for the given source currency, target currency, and amount.
    /// </summary>
    /// <param name="sourceCurrency">The source currency code.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <param name="amount">The amount to convert.</param>
    /// <returns>
    ///     A <see cref="TransferForexPreviewResponse" /> on success,
    ///     or a validation or failure error.
    /// </returns>
    public ErrorOr<TransferForexPreviewResponse> GetFxPreview(string sourceCurrency, string targetCurrency, decimal amount);

    /// <summary>
    ///     Returns whether the specified amount requires two-factor authentication.
    /// </summary>
    /// <param name="amount">The transfer amount.</param>
    /// <returns><see langword="true" /> when the amount requires two-factor authentication.</returns>
    public bool RequiresTwoFactorAuthentication(decimal amount);

    /// <summary>
    ///     Validates, authorizes, and executes a transfer for the specified user.
    /// </summary>
    /// <param name="request">The transfer creation request.</param>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>
    ///     A <see cref="TransferResponse" /> on success,
    ///     or a validation, authorization, or failure error.
    /// </returns>
    public ErrorOr<TransferResponse> CreateTransfer(CreateTransferRequest request, int userId);

    /// <summary>
    ///     Returns all transfers initiated by the specified user, newest first.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>
    ///     A list of <see cref="TransferResponse" /> on success,
    ///     or a failure error.
    /// </returns>
    public ErrorOr<List<TransferResponse>> GetHistory(int userId);
}
