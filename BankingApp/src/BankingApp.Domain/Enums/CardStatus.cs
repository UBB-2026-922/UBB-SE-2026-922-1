// <copyright file="CardStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the CardStatus values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the current status of a payment card.
/// </summary>
public enum CardStatus
{
    /// <summary>
    ///     The card is active and can be used for transactions.
    /// </summary>
    Active,

    /// <summary>
    ///     The card is temporarily frozen by the user.
    /// </summary>
    Frozen,

    /// <summary>
    ///     The card has been permanently cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    ///     The card has passed its expiry date.
    /// </summary>
    Expired
}