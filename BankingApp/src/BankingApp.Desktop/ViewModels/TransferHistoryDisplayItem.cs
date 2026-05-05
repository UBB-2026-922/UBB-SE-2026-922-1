// <copyright file="TransferHistoryDisplayItem.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferHistoryDisplayItem class.
// </summary>

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Represents a single pre-formatted transfer row for display in the transfer history page.
/// </summary>
public class TransferHistoryDisplayItem
{
    /// <summary>
    ///     Gets or sets the recipient's full name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the recipient's IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the recipient's bank name inferred from the IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the pre-formatted amount string including currency (e.g. "-100.00 EUR").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AmountDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the pre-formatted local date and time string.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string DateDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the human-readable transfer status (e.g. "Completed", "Pending").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string StatusDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the transaction reference returned by the server, or "—" when none was assigned.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ReferenceDisplay { get; set; } = string.Empty;
}
