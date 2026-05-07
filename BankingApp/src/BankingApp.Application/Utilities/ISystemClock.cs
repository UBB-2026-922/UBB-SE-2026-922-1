// <copyright file="ISystemClock.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ISystemClock interface.
// </summary>

namespace BankingApp.Application.Utilities;

/// <summary>
///     Abstracts the system clock so that time-dependent logic can be tested deterministically.
/// </summary>
public interface ISystemClock
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }
}