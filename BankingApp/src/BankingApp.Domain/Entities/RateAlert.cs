// <copyright file="RateAlert.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlert entity for Team B's FX rate alert feature.
// </summary>

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
}
