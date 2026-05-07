// <copyright file="SavedBiller.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SavedBiller entity for the bill payment feature.
// </summary>

using System;

namespace BankingApp.Domain.Entities;

/// <summary>
/// Represents a biller that a user has saved to their personal quick-pay list.
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
    /// Gets or sets the unique identifier for this saved-biller entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who saved this biller.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this saved biller.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the saved biller.
    /// </summary>
    public int BillerId { get; set; }

    /// <summary>
    /// Gets or sets the biller associated with this saved record.
    /// </summary>
    public Biller? Biller { get; set; }

    /// <summary>
    /// Gets or sets the user-defined nickname for this saved biller.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the pre-filled reference number used when paying this biller.
    /// </summary>
    public string? DefaultReference { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) when this entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}