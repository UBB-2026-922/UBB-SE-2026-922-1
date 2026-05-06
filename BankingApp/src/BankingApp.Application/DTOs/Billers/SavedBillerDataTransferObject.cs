// <copyright file="SavedBillerDataTransferObject.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SavedBillerDataTransferObject class.
// </summary>

namespace BankingApp.Application.DTOs.Billers;

/// <summary>
///     Represents a saved biller entry in API responses.
/// </summary>
public class SavedBillerDataTransferObject
{
    /// <summary>Gets or sets the unique identifier for the saved biller entry.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the name of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the category of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerCategory { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the biller logo.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets the user-assigned nickname for this biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? Nickname { get; set; }

    /// <summary>Gets or sets the default payment reference for this biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? DefaultReference { get; set; }

    /// <summary>Gets or sets the date and time the biller was saved.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}