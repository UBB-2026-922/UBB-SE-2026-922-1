namespace BankingApp.Web.ViewModels.BillPayments;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using Contracts.Features.Billers.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
///     View model for the bill-payment entry form (GET /BillPayments).
/// </summary>
public class BillPayViewModel
{
    /// <summary>Gets or sets the list of the user's saved billers.</summary>
    public List<SavedBillerDto> SavedBillers { get; set; } = [];

    /// <summary>Gets or sets all active billers for the search dropdown.</summary>
    public List<BillerDto> AllBillers { get; set; } = [];

    /// <summary>Gets or sets the user's active accounts for the source-account dropdown.</summary>
    public List<AccountDto> Accounts { get; set; } = [];

    /// <summary>Gets a <see cref="SelectList"/> built from <see cref="AllBillers"/>.</summary>
    public SelectList BillerSelectList =>
        new(AllBillers, nameof(BillerDto.Id), nameof(BillerDto.Name), SelectedBillerId);

    /// <summary>Gets a <see cref="SelectList"/> built from <see cref="Accounts"/>.</summary>
    public SelectList AccountSelectList =>
        new(Accounts.Select(a => new
        {
            a.Id,
            Display = $"{a.AccountName} — {a.Iban} ({a.Currency}) | Balance: {a.Balance:N2}"
        }),
            "Id", "Display", SelectedAccountId);

    /// <summary>Gets or sets the selected biller id.</summary>
    [Required(ErrorMessage = "Please select a biller.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a biller.")]
    public int SelectedBillerId { get; set; }

    /// <summary>Gets or sets the selected source account id.</summary>
    [Required(ErrorMessage = "Please select an account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
    public int SelectedAccountId { get; set; }

    /// <summary>Gets or sets the customer reference / invoice number at the biller.</summary>
    [Required(ErrorMessage = "Biller reference is required.")]
    [MaxLength(100, ErrorMessage = "Reference must be at most 100 characters.")]
    [Display(Name = "Biller Reference / Account Number")]
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment amount.</summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
}
