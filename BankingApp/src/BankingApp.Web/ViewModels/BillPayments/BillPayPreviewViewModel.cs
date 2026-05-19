namespace BankingApp.Web.ViewModels.BillPayments;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     View model for the bill-payment preview / confirmation step.
///     All fields are echoed as hidden inputs and carry the payment summary.
/// </summary>
public class BillPayPreviewViewModel
{
    /// <summary>Gets or sets the source account id.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the biller id.</summary>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the biller reference.</summary>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the fee that will be charged.</summary>
    public decimal Fee { get; set; }

    /// <summary>Gets or sets the total debited from the account (Amount + Fee).</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets or sets the human-readable biller name for display.</summary>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the account IBAN for display.</summary>
    public string AccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the account currency code for display.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is required for this payment
    ///     (amount >= 1,000).
    /// </summary>
    public bool RequiresTwoFa { get; set; }

    /// <summary>
    ///     Gets or sets the one-time password token supplied by the user when
    ///     <see cref="RequiresTwoFa"/> is <c>true</c>.
    /// </summary>
    [Display(Name = "One-Time Password (OTP)")]
    [MaxLength(10)]
    public string? TwoFaToken { get; set; }

    /// <summary>Gets or sets a server-side validation message to show on the preview page.</summary>
    public string? ErrorMessage { get; set; }
}
