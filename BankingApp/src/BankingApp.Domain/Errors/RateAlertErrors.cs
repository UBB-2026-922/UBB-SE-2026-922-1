using ErrorOr;

namespace BankingApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for FX rate alert validation.
/// </summary>
public static class RateAlertErrors
{
    /// <summary>The base currency is required.</summary>
    public static readonly Error BaseCurrencyRequired =
        Error.Validation("rate_alert.base_currency_required", "Base currency cannot be empty.");

    /// <summary>The target currency is required.</summary>
    public static readonly Error TargetCurrencyRequired =
        Error.Validation("rate_alert.target_currency_required", "Target currency cannot be empty.");

    /// <summary>The base and target currencies must differ.</summary>
    public static readonly Error MatchingCurrencies =
        Error.Validation("rate_alert.matching_currencies", "Base and target currencies must differ.");

    /// <summary>The target rate must be positive.</summary>
    public static readonly Error InvalidTargetRate =
        Error.Validation("rate_alert.invalid_target_rate", "Target rate must be greater than zero.");
}
