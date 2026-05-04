// <copyright file="Beneficiary.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the Beneficiary entity for Team B's beneficiary management feature.
// </summary>

using System;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a beneficiary to whom a user can send money.
/// </summary>
/// <remarks>
///     each beneficiary entry is owned by exactly one user.
///     The composite unique constraint <c>UQ_Beneficiaries_UserIBAN</c> ensures that
///     the same IBAN cannot be saved twice for the same user.
/// </remarks>
public class Beneficiary
{
    /// <summary>
    ///     Gets or sets the unique identifier for this beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who owns this beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the International Bank Account Number of the beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the beneficiary's bank.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? BankName { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the last transfer made to this beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? LastTransferDate { get; set; }

    /// <summary>
    ///     Gets or sets the total amount of money sent to this beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal TotalAmountSent { get; set; }

    /// <summary>
    ///     Gets or sets the total number of transfers made to this beneficiary.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int TransferCount { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) when this beneficiary was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}