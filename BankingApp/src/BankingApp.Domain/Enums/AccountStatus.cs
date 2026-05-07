// <copyright file="AccountStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the AccountStatus values.
// </summary>

namespace BankingApp.Domain.Enums;

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
    Closed
}