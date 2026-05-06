// <copyright file="DashboardTransactionItem.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardTransactionItem class.
// </summary>

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Represents a formatted transaction row for dashboard display.
/// </summary>
public partial class DashboardTransactionItem
{
    /// <summary>
    ///     Gets or sets the merchant or fallback display name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string MerchantDisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the formatted amount string.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AmountDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the currency code.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;
}
