namespace BankingApp.Web.ViewModels.RateAlerts;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     View model for the create rate alert form (GET/POST /RateAlerts/Create).
/// </summary>
public class CreateRateAlertViewModel
{
    /// <summary>Gets or sets the base currency code.</summary>
    [Required(ErrorMessage = "Base currency is required.")]
    [Display(Name = "From Currency")]
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target currency code.</summary>
    [Required(ErrorMessage = "Target currency is required.")]
    [Display(Name = "To Currency")]
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target rate.</summary>
    [Required(ErrorMessage = "Target rate is required.")]
    [Range(0.000001, 1_000_000, ErrorMessage = "Target rate must be a positive number.")]
    [Display(Name = "Target Rate")]
    public decimal TargetRate { get; set; }

    /// <summary>Gets or sets whether this is a buy (above) alert. True = above, false = below.</summary>
    [Display(Name = "Direction")]
    public bool IsBuyAlert { get; set; } = true;

    /// <summary>Gets the list of supported currencies for the dropdowns.</summary>
    public static IReadOnlyList<string> SupportedCurrencies { get; } =
        ["USD", "EUR", "GBP", "RON", "CHF", "JPY", "CAD", "AUD"];
}
