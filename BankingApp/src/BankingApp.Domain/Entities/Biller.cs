// <copyright file="Biller.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the Biller entity for Team B's bill payment feature.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a registered biller (utility company, telecom operator, etc.)
///     that users can pay through the application.
///     Maps to the SQL table <c>Biller</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         <c>Biller</c> is a master-data (lookup) entity and does not directly reference
///         the base entities <c>User</c>, <c>Account</c>, or <c>Transaction</c>.
///     </para>
///     <para>
///         It is referenced by <see cref="BillPayment" />, <see cref="SavedBiller" />,
///         and <see cref="RecurringPayment" /> via One-to-Many relationships.
///     </para>
/// </remarks>
public class Biller
{
    /// <summary>Gets or sets the unique identifier for the biller.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name of the biller.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category the biller belongs to (e.g. "Utilities").</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the biller's logo image.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the biller is currently active.</summary>
    public bool IsActive { get; set; } = true;
}
