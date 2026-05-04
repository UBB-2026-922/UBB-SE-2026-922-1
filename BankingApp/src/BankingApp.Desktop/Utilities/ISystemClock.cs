// <copyright file="ISystemClock.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ISystemClock interface.
// </summary>

using System;

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Abstracts the system clock to allow deterministic time-based testing.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    ///     Gets the current UTC time.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    DateTime UtcNow { get; }
}