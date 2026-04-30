// <copyright file="Biller.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
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
    /// <summary>
    ///     Gets or sets the unique identifier for this biller.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique display name of the biller (e.g., "Enel Energie").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the business sector category of this biller.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public BillerCategory Category { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the biller's logo image, or <see langword="null" /> if not available.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? LogoUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this biller is currently accepting payments.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsActive { get; set; } = true;
}
