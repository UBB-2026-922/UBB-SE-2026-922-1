// <copyright file="SystemClock.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SystemClock class.
// </summary>

namespace BankingApp.Application.Utilities;

/// <summary>
///     Production implementation of <see cref="ISystemClock" /> that delegates to <see cref="DateTime.UtcNow" />.
/// </summary>
public class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}