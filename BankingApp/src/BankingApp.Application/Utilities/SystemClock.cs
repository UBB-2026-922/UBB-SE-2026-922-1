// <copyright file="SystemClock.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
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
