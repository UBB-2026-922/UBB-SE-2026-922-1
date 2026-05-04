// <copyright file="TransferAccountDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferAccountDto class.
// </summary>

namespace BankingApp.Desktop.Models;

/// <summary>
///     Represents a bank account available for transfers, as returned by the API.
/// </summary>
public class TransferAccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the account IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string IBAN { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account currency code (e.g. EUR, USD, RON).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current account balance.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AccountName { get; set; } = string.Empty;
}
