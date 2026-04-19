// <copyright file="TransactionStatus.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionStatus values.
// </summary>

namespace BankApp.Domain.Enums;

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
    Cancelled,
}