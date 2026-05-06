// <copyright file="ExchangeTransactionResponseDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeTransactionResponseDto class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.Exchange;

/// <summary>
///     Carries the result data of a currency exchange operation or rate preview.
/// </summary>
public class ExchangeTransactionResponseDto
{
    /// <summary>Gets or sets the unique identifier of the exchange (0 for previews).</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the ISO 4217 source currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 target currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount deducted from the source account.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal SourceAmount { get; set; }

    /// <summary>Gets or sets the amount credited to the target account.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetAmount { get; set; }

    /// <summary>Gets or sets the applied exchange rate.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the commission charged for this exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Commission { get; set; }

    /// <summary>Gets or sets the processing status.</summary>
    /// <value>Gets or sets the current value.</value>
    public ExchangeTransactionStatus Status { get; set; }

    /// <summary>Gets or sets the timestamp when this exchange was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}
