// <copyright file="TransactionStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionStatus values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the status of a transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    ///     The transaction is pending.
    /// </summary>
    Pending,

    /// <summary>
    ///     The transaction was completed.
    /// </summary>
    Completed,

    /// <summary>
    ///     The transaction failed.
    /// </summary>
    Failed,

    /// <summary>
    ///     The transaction was canceled.
    /// </summary>
    Cancelled
}