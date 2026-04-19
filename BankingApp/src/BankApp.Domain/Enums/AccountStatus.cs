// <copyright file="AccountStatus.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AccountStatus values.
// </summary>

namespace BankApp.Domain.Enums;

/// <summary>
///     Represents the operational status of a bank account.
/// </summary>
public enum AccountStatus
{
    /// <summary>The account is fully operational.</summary>
    Active,

    /// <summary>The account has been temporarily suspended.</summary>
    Suspended,

    /// <summary>The account has been permanently closed.</summary>
    Closed,
}