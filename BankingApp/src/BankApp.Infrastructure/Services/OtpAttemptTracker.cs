// <copyright file="OtpAttemptTracker.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OtpAttemptTracker class.
// </summary>

using System.Collections.Concurrent;
using BankApp.Application.Services.Login;

namespace BankApp.Infrastructure.Services;

/// <summary>
///     In-process, thread-safe implementation of <see cref="IOtpAttemptTracker" />.
///     Registered as a singleton so the counters survive individual request scopes.
/// </summary>
public class OtpAttemptTracker : IOtpAttemptTracker
{
    private readonly ConcurrentDictionary<int, int> _failedAttempts = new();

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public int RecordFailure(int userId)
    {
        return _failedAttempts.AddOrUpdate(userId, 1, (_, count) => count + 1);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    public void Reset(int userId)
    {
        _failedAttempts.TryRemove(userId, out _);
    }
}