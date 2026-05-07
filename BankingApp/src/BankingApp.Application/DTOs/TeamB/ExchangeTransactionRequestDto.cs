// <copyright file="ExchangeTransactionRequestDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeTransactionRequestDto class.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to execute a currency exchange.
/// </summary>
public class ExchangeTransactionRequestDto
{
    /// <summary>Gets or sets the identifier of the user initiating the exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the account debited in the source currency.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the identifier of the account credited in the target currency.</summary>
    /// <value>Gets or sets the current value.</value>
    public int TargetAccountId { get; set; }

    /// <summary>Gets or sets the ISO 4217 source currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 target currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount to convert from the source account.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal SourceAmount { get; set; }
}