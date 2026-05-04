// <copyright file="PaymentStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the PaymentStatus enum.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
/// Specifies the current status of a payment or recurring payment setup.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment is pending and has not been processed yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The payment has been successfully completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The payment failed during processing.
    /// </summary>
    Failed,

    /// <summary>
    /// The recurring payment setup is currently active.
    /// </summary>
    Active,

    /// <summary>
    /// The recurring payment setup is temporarily paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The payment or recurring payment setup has been cancelled.
    /// </summary>
    Cancelled,
}