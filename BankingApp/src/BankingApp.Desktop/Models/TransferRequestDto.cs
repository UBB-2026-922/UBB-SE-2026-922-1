// <copyright file="TransferRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferRequestDto class.
// </summary>

namespace BankingApp.Desktop.Models;

/// <summary>
///     Request body sent to the transfer execution endpoint.
/// </summary>
public class TransferRequestDto
{
    /// <summary>
    ///     Gets or sets the source account identifier.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SourceAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the recipient display name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the recipient IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientIBAN { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the transfer amount.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the currency code for the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the optional 2FA token when the amount requires two-factor authentication.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? TwoFAToken { get; set; }
}
