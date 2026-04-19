// <copyright file="TransactionDirection.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionDirection values.
// </summary>

namespace BankApp.Domain.Enums;

/// <summary>
///     Represents the direction of a transaction, inbound or outbound.
/// </summary>
public enum TransactionDirection
{
    /// <summary>
    ///     The transaction is inbound. (Receiving)
    /// </summary>
    In,

    /// <summary>
    ///     The transaction is outbound. (Sending)
    /// </summary>
    Out,
}