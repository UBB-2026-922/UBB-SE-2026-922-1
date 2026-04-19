// <copyright file="CardStatus.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the CardStatus values.
// </summary>

namespace BankApp.Application.Enums;

/// <summary>
///     Defines the application-facing card status values exposed to presentation clients.
/// </summary>
public enum CardStatus
{
    /// <summary>
    ///     The card is active.
    /// </summary>
    Active = 0,

    /// <summary>
    ///     The card is frozen.
    /// </summary>
    Frozen = 1,

    /// <summary>
    ///     The card has been cancelled.
    /// </summary>
    Cancelled = 2,

    /// <summary>
    ///     The card is expired.
    /// </summary>
    Expired = 3,
}
