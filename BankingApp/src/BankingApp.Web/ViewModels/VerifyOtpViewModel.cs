namespace BankingApp.Web.ViewModels;

using System.ComponentModel.DataAnnotations;

public sealed class VerifyOtpViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "OTP code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must contain only digits.")]
    [Display(Name = "6-digit code")]
    public string OtpCode { get; set; } = string.Empty;
}