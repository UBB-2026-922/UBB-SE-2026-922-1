namespace BankingApp.Desktop.Models;

/// <summary>
///     Response from the FX preview endpoint showing the converted amount and exchange rate.
/// </summary>
public abstract class ForexPreviewDto
{
    /// <summary>
    ///     Gets or sets the exchange rate applied for the currency conversion.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    ///     Gets or sets the converted amount in the target currency.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal ConvertedAmount { get; set; }
}
