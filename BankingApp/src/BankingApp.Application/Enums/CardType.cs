// <copyright file="CardType.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the CardType values.
// </summary>

namespace BankingApp.Application.Enums;

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
