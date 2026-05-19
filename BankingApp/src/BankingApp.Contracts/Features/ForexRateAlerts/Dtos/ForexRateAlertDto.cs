namespace BankingApp.Contracts.Features.ForexRateAlerts.Dtos;

/// <summary>
///     Carries rate alert configuration data between the application and presentation layers.
/// </summary>
public class ForexRateAlertDto
{
    /// <summary>Gets or sets the unique identifier (0 for new alerts).</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning user's identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the ISO 4217 base currency code (e.g., "EUR").</summary>
    /// <value>Gets or sets the current value.</value>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 target currency code (e.g., "USD").</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the rate at which the alert fires.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetRate { get; set; }

    /// <summary>Gets or sets a value indicating whether the alert targets a buy rate (<see langword="true" />) or a sell rate (<see langword="false" />).</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsBuyAlert { get; set; }

    /// <summary>Gets or sets a value indicating whether the alert has already been triggered.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsTriggered { get; set; }

    /// <summary>Gets or sets the timestamp when this alert was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}
