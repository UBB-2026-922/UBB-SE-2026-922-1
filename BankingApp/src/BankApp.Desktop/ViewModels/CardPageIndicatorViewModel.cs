// <copyright file="CardPageIndicatorViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the CardPageIndicatorViewModel class.
// </summary>

namespace BankApp.Desktop.ViewModels;

/// <summary>
///     Represents the display state of a single card-navigation dot in the dashboard carousel.
/// </summary>
public class CardPageIndicatorViewModel
{
    /// <summary>
    ///     Gets a value indicating whether this dot corresponds to the currently displayed card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsActive { get; init; }
}