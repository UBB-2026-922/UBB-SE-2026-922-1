// <copyright file="Category.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the Category class.
// </summary>

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a transaction category.
/// </summary>
public class Category
{
    /// <summary>
    ///     Gets or sets the unique identifier for the category.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the category.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the icon identifier for the category.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Icon { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is a system-defined category.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsSystem { get; set; } = true;
}