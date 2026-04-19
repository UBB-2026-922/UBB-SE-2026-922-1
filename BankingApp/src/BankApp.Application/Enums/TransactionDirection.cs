// <copyright file="TransactionDirection.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransactionDirection values.
// </summary>

namespace BankApp.Application.Enums;

/// <summary>
///     Defines the application-facing transaction directions exposed to presentation clients.
/// </summary>
public enum TransactionDirection
{
    /// <summary>
    ///     Funds are entering the account.
    /// </summary>
    In = 0,

    /// <summary>
    ///     Funds are leaving the account.
    /// </summary>
    Out = 1,
}
