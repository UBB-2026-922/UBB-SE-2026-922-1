// <copyright file="SystemClock.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SystemClock class.
// </summary>

using System;

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Production implementation of <see cref="ISystemClock" /> backed by <see cref="DateTime.UtcNow" />.
/// </summary>
public partial class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
