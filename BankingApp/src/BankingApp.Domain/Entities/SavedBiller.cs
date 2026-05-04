// <copyright file="SavedBiller.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SavedBiller entity for Team B's bill payment feature.
// </summary>

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a biller that a user has saved to their personal quick-pay list.
///     Maps to the SQL table <c>SavedBiller</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One):
///         each saved-biller entry belongs to exactly one user.
///     </para>
///     <para>
///         References <see cref="Biller" /> via <see cref="BillerId" /> (Many-to-One):
///         the biller being saved must exist in the <c>Biller</c> master table.
///     </para>
/// </remarks>
public class SavedBiller
{
    /// <summary>
    ///     Gets or sets the unique identifier for this saved-biller entry.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the <see cref="User" /> who saved this biller.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the saved <see cref="Biller" />.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the user-defined nickname for this saved biller
    ///     (e.g., "My electricity bill"), or <see langword="null" /> if not set.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Nickname { get; set; }

    /// <summary>
    ///     Gets or sets the pre-filled reference number used when paying this biller
    ///     (e.g., the user's contract number), or <see langword="null" /> if not set.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? DefaultReference { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) when this entry was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}
