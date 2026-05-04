// <copyright file="BillerDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillerDto class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries biller master data to the presentation layer.
/// </summary>
public class BillerDto
{
    /// <summary>Gets or sets the unique identifier of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the business sector category.</summary>
    /// <value>Gets or sets the current value.</value>
    public BillerCategory Category { get; set; }

    /// <summary>Gets or sets the URL of the biller's logo, or <see langword="null" /> if not available.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the biller is currently accepting payments.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsActive { get; set; }
}
