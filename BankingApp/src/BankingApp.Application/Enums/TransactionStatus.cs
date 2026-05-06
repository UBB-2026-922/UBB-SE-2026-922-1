// <copyright file="TransactionStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionStatus values.
// </summary>

namespace BankingApp.Application.Enums;

/// <summary>
///     Defines the application-facing transaction status values exposed to presentation clients.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    ///     The transaction is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     The transaction completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    ///     The transaction failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    ///     The transaction was cancelled.
    /// </summary>
    Cancelled = 3,
}
