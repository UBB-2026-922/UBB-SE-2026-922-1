// <copyright file="Account.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Account class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a bank account belonging to a user.
/// </summary>
public class Account
{
    /// <summary>
    ///     Gets or sets the unique identifier for the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who owns this account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? AccountName { get; set; }

    /// <summary>
    ///     Gets or sets the International Bank Account Number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the currency code for the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current balance of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the type of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public AccountType AccountType { get; set; }

    /// <summary>
    ///     Gets or sets the operational status of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    /// <summary>
    ///     Gets or sets the date and time when the account was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Returns true when the account is in the Active state.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public bool IsActive()
    {
        return Status == AccountStatus.Active;
    }
}