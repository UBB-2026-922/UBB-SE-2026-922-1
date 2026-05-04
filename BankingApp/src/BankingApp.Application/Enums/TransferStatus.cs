// <copyright file="TransferStatus.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferStatus enum.
// </summary>

namespace BankingApp.Application.Enums;

/// <summary>
///     Represents the current status of a money transfer.
/// </summary>
public enum TransferStatus
{
    /// <summary>The transfer has been created and is waiting to be processed.</summary>
    Pending,

    /// <summary>The transfer is currently being processed.</summary>
    Processing,

    /// <summary>The transfer completed successfully.</summary>
    Completed,

    /// <summary>The transfer failed.</summary>
    Failed,

    /// <summary>The transfer was cancelled.</summary>
    Cancelled,
}