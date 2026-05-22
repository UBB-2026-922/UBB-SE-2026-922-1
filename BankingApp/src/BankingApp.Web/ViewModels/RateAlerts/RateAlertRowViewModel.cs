namespace BankingApp.Web.ViewModels.RateAlerts;

/// <summary>
///     Represents a single row in the rate alerts table.
/// </summary>
public class RateAlertRowViewModel
{
    /// <summary>Gets or sets the alert id.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the base currency code.</summary>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target currency code.</summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets the currency pair display string (e.g. "EUR/USD").</summary>
    public string CurrencyPair => $"{BaseCurrency}/{TargetCurrency}";

    /// <summary>Gets or sets the target rate that triggers the alert.</summary>
    public decimal TargetRate { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a buy (above) alert.</summary>
    public bool IsBuyAlert { get; set; }

    /// <summary>Gets the direction label (Above / Below).</summary>
    public string Direction => IsBuyAlert ? "Above" : "Below";

    /// <summary>Gets the direction badge class.</summary>
    public string DirectionBadgeClass => IsBuyAlert ? "bg-success" : "bg-danger";

    /// <summary>Gets or sets whether the alert has been triggered.</summary>
    public bool IsTriggered { get; set; }

    /// <summary>Gets the status display text.</summary>
    public string Status => IsTriggered ? "Triggered" : "Active";

    /// <summary>Gets the status badge class.</summary>
    public string StatusBadgeClass => IsTriggered ? "bg-secondary" : "bg-primary";

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }
}
