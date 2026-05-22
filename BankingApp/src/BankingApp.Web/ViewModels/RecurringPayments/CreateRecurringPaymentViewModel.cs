namespace BankingApp.Web.ViewModels.RecurringPayments;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using BankingApp.Contracts.Features.Billers.Dtos;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class CreateRecurringPaymentViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Please select a biller.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a biller.")]
    [Display(Name = "Biller")]
    public int SelectedBillerId { get; set; }

    [Required(ErrorMessage = "Please select a source account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a source account.")]
    [Display(Name = "Source Account")]
    public int SelectedSourceAccountId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Display(Name = "Pay full outstanding balance")]
    public bool IsPayInFull { get; set; }

    [Required(ErrorMessage = "Please select a frequency.")]
    [Display(Name = "Frequency")]
    public RecurringFrequency Frequency { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "End Date (optional)")]
    public DateTime? EndDate { get; set; }

    public List<BillerDto> Billers { get; set; } = [];

    public List<AccountDto> Accounts { get; set; } = [];

    public SelectList BillerSelectList =>
        new(Billers, nameof(BillerDto.Id), nameof(BillerDto.Name), SelectedBillerId);

    public SelectList AccountSelectList =>
        new(Accounts.Select(account => new
            {
                account.Id,
                Display = $"{account.AccountName} — {account.Iban} ({account.Currency}) | Balance: {account.Balance:N2}"
            }),
            "Id", "Display", SelectedSourceAccountId);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Start date cannot be in the past.",
                [nameof(StartDate)]);
        }

        if (EndDate.HasValue && EndDate.Value <= StartDate)
        {
            yield return new ValidationResult(
                "End date must be after the start date.",
                [nameof(EndDate)]);
        }
    }
}
