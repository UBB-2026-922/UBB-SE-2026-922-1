// <copyright file="AccountType.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AccountType values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the type of a bank account.
/// </summary>
public enum AccountType
{
    /// <summary>A standard checking account for everyday transactions.</summary>
    Checking,

    /// <summary>A savings account intended for accumulating funds.</summary>
    Savings,

    /// <summary>A business-purpose account.</summary>
    Business,

    /// <summary>A credit account.</summary>
    Credit,
}