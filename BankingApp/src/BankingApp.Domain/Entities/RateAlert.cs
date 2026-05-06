// <copyright file="RateAlert.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlert entity for Team B's FX rate alert feature.
// </summary>

using BankingApp.Domain.Errors;
using ErrorOr;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a user-defined alert that triggers when a currency pair reaches a target rate.
///     Maps to the SQL table <c>RateAlert</c> introduced by Team B.
/// </summary>
/// <remarks>
///     Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One):
///     each alert belongs to exactly one user and monitors a single currency pair.
/// </remarks>
public class RateAlert
{
    private const int RatePrecisionDecimals = 2;

    /// <summary>Gets or sets the unique identifier for this rate alert.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the <see cref="User" /> who created this alert.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the ISO 4217 code of the base currency being monitored (e.g., "EUR").</summary>
    /// <value>Gets or sets the current value.</value>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 code of the target currency being monitored (e.g., "USD").</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the exchange rate at which this alert should fire.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetRate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this alert has already been triggered.
    ///     Once triggered, no further notifications are sent until the user resets it.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsTriggered { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the alert fires when the user intends to buy
    ///     the target currency (<see langword="true" />) or sell it (<see langword="false" />).
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsBuyAlert { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when this alert was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Creates a validated rate alert aggregate.
    /// </summary>
    /// <param name="userId">The alert owner identifier.</param>
    /// <param name="baseCurrency">The base currency.</param>
    /// <param name="targetCurrency">The target currency.</param>
    /// <param name="targetRate">The target exchange rate.</param>
    /// <param name="isBuyAlert">Whether the alert triggers on buy conditions.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The result of the operation.</returns>
    public static ErrorOr<RateAlert> Create(
        int userId,
        string baseCurrency,
        string targetCurrency,
        decimal targetRate,
        bool isBuyAlert,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency)) return RateAlertErrors.BaseCurrencyRequired;
        if (string.IsNullOrWhiteSpace(targetCurrency)) return RateAlertErrors.TargetCurrencyRequired;
        if (baseCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
            return RateAlertErrors.MatchingCurrencies;
        if (targetRate <= 0) return RateAlertErrors.InvalidTargetRate;

        return new RateAlert
        {
            UserId = userId,
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            TargetRate = targetRate,
            IsBuyAlert = isBuyAlert,
            IsTriggered = false,
            CreatedAt = createdAt
        };
    }

    /// <summary>
    ///     Determines whether the alert should trigger for the provided current rate.
    /// </summary>
    /// <param name="currentRate">The current market rate.</param>
    /// <returns>The result of the operation.</returns>
    public bool ShouldTrigger(decimal currentRate)
    {
        decimal roundedCurrentRate = Math.Round(currentRate, RatePrecisionDecimals);
        decimal roundedTargetRate = Math.Round(TargetRate, RatePrecisionDecimals);

        return IsBuyAlert
            ? roundedCurrentRate <= roundedTargetRate
            : roundedCurrentRate >= roundedTargetRate;
    }
}
