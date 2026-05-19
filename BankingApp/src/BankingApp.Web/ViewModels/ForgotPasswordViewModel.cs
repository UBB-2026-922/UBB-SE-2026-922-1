namespace BankingApp.Web.ViewModels;

using System.ComponentModel.DataAnnotations;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;
}
