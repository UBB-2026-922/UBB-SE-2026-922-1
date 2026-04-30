// <copyright file="RateAlertDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertDto record.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries rate alert configuration data between the application and presentation layers.
/// </summary>
/// <param name="Id">The unique identifier (0 for new alerts).</param>
/// <param name="UserId">The owning user's identifier.</param>
/// <param name="BaseCurrency">The ISO 4217 base currency code (e.g., "EUR").</param>
/// <param name="TargetCurrency">The ISO 4217 target currency code (e.g., "USD").</param>
/// <param name="TargetRate">The rate at which the alert fires.</param>
/// <param name="IsBuyAlert">True when the alert targets a buy rate; false for a sell rate.</param>
/// <param name="IsTriggered">Whether the alert has already been triggered.</param>
/// <param name="CreatedAt">The timestamp when this alert was created.</param>
public record RateAlertDto(
    int Id,
    int UserId,
    string BaseCurrency,
    string TargetCurrency,
    decimal TargetRate,
    bool IsBuyAlert,
    bool IsTriggered,
    DateTime CreatedAt);
