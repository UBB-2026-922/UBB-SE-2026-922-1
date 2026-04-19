// <copyright file="IOtpAttemptTracker.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IOtpAttemptTracker interface.
// </summary>

namespace BankApp.Application.Services.Login;

/// <summary>
///     Tracks consecutive failed OTP verification attempts per user.
/// </summary>
public interface IOtpAttemptTracker
{
    /// <summary>
    ///     Records a failed OTP attempt for the user and returns the new total.
    /// </summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    int RecordFailure(int userId);

    /// <summary>
    ///     Clears the failure counter for the user (on success, resend, or max exceeded).
    /// </summary>
    /// <param name="userId">The userId value.</param>
    void Reset(int userId);
}