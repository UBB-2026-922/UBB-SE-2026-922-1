namespace BankingApp.Web.ViewModels;

using System.ComponentModel.DataAnnotations;

public sealed class EditBeneficiaryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "IBAN is required.")]
    [MaxLength(34, ErrorMessage = "IBAN must be 34 characters or fewer.")]
    [Display(Name = "IBAN")]
    public string Iban { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Bank name must be 100 characters or fewer.")]
    [Display(Name = "Bank name (optional)")]
    public string? BankName { get; set; }
}