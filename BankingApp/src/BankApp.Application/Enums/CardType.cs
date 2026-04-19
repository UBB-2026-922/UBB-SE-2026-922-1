// <copyright file="CardType.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the CardType values.
// </summary>

namespace BankApp.Application.Enums;

/// <summary>
///     Defines the application-facing card types exposed to presentation clients.
/// </summary>
public enum CardType
{
    /// <summary>
    ///     A debit card.
    /// </summary>
    Debit = 0,

    /// <summary>
    ///     A credit card.
    /// </summary>
    Credit = 1,
}
