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
    /// <summary>Gets or sets the unique identifier for the saved biller entry.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who saved the biller.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the biller that was saved.</summary>
    public int BillerId { get; set; }

    /// <summary>Gets or sets an optional user-assigned nickname for the biller.</summary>
    public string? Nickname { get; set; }

    /// <summary>Gets or sets an optional default payment reference for this biller.</summary>
    public string? DefaultReference { get; set; }

    /// <summary>Gets or sets the date and time the biller was saved.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the navigation property to the associated <see cref="Biller" />.</summary>
    public Biller? Biller { get; set; }
}
