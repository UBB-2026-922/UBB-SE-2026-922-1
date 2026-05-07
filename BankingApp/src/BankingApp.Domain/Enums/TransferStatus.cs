// <copyright file="TransferStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferStatus enum.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the processing status of a bank transfer.
/// </summary>
public enum TransferStatus
{
    /// <summary>
    ///     The transfer has been submitted and is awaiting processing.
    /// </summary>
    Pending,

    /// <summary>
    ///     The transfer was processed and funds were delivered successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     The transfer could not be processed due to an error.
    /// </summary>
    Failed,

    /// <summary>
    ///     The transfer was cancelled before processing.
    /// </summary>
    Cancelled
}