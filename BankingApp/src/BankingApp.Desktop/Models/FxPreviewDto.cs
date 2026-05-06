// <copyright file="FxPreviewDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the FxPreviewDto class.
// </summary>

namespace BankingApp.Desktop.Models;

/// <summary>
///     Response from the FX preview endpoint showing the converted amount and exchange rate.
/// </summary>
public partial class FxPreviewDto
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
