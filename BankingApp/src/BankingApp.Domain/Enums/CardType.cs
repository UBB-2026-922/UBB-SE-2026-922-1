// <copyright file="CardType.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the CardType values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the type of a payment card.
/// </summary>
public enum CardType
{
    /// <summary>
    ///     A debit card linked directly to a bank account.
    /// </summary>
    Debit,

    /// <summary>
    ///     A credit card backed by a line of credit.
    /// </summary>
    Credit,

    /// <summary>
    ///     A prepaid card loaded with a fixed amount.
    /// </summary>
    Prepaid,
}