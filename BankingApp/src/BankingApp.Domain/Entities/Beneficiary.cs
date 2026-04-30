// <copyright file="Beneficiary.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Beneficiary entity for Team B's beneficiary management feature.
// </summary>

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a saved transfer recipient that belongs to a user's address book.
///     Maps to the SQL table <c>Beneficiaries</c> introduced by Team B.
/// </summary>
/// <remarks>
///     Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One):
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
    ///     Gets or sets the identifier of the <see cref="User" /> who owns this beneficiary entry.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the full display name of the beneficiary.
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
    ///     Gets or sets the name of the beneficiary's bank, if known.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? BankName { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the most recent transfer to this beneficiary,
    ///     or <see langword="null" /> if no transfer has been made yet.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? LastTransferDate { get; set; }

    /// <summary>
    ///     Gets or sets the cumulative amount sent to this beneficiary across all transfers.
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
