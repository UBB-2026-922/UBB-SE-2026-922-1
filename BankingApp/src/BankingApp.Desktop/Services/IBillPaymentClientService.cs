namespace BankingApp.Desktop.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.BillPayments;
using Application.DTOs.Billers;
using Application.DTOs.RecurringPayments;
using ErrorOr;

/// <summary>
///     Defines the desktop client boundary for bill-payment, biller, and recurring-payment operations.
/// </summary>
public interface IBillPaymentClientService
{
    /// <summary>
    ///     Loads available billers, optionally filtered by search text and category.
    /// </summary>
    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null);

    /// <summary>
    ///     Loads billers saved by the authenticated user.
    /// </summary>
    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync();

    /// <summary>
    ///     Loads accounts that can be used as bill-payment sources.
    /// </summary>
    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync();

    /// <summary>
    ///     Loads the bill-payment fee for the specified amount.
    /// </summary>
    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount);

    /// <summary>
    ///     Determines whether the specified payment amount requires two-factor confirmation.
    /// </summary>
    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount);

    /// <summary>
    ///     Executes a bill payment.
    /// </summary>
    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request);

    /// <summary>
    ///     Saves a biller for future reuse.
    /// </summary>
    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request);

    /// <summary>
    ///     Loads recurring payments for the authenticated user.
    /// </summary>
    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetRecurringPaymentsAsync();

    /// <summary>
    ///     Creates a recurring payment.
    /// </summary>
    public Task<ErrorOr<RecurringPaymentResponse>> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request);

    /// <summary>
    ///     Pauses a recurring payment.
    /// </summary>
    public Task<ErrorOr<Success>> PauseRecurringPaymentAsync(int paymentId);

    /// <summary>
    ///     Resumes a recurring payment.
    /// </summary>
    public Task<ErrorOr<Success>> ResumeRecurringPaymentAsync(int paymentId);

    /// <summary>
    ///     Cancels a recurring payment.
    /// </summary>
    public Task<ErrorOr<Success>> CancelRecurringPaymentAsync(int paymentId);
}
