// <copyright file="TwoFactorState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TwoFactorState values.
// </summary>

namespace BankingApp.Desktop.Enums;

/// <summary>
///     Represents the possible states of the 2FA flow.
/// </summary>
public enum TwoFactorState
{
    /// <summary>
    ///     Awaiting the user to enter their OTP.
    /// </summary>
    Idle,

    /// <summary>
    ///     The OTP is being verified against the server.
    /// </summary>
    Verifying,

    /// <summary>
    ///     Verification succeeded. The user should be navigated to the main application.
    /// </summary>
    Success,

    /// <summary>
    ///     The submitted OTP did not match the expected value.
    /// </summary>
    InvalidOtp,

    /// <summary>
    ///     The OTP was valid but has passed its expiry window.
    /// </summary>
    Expired,

    /// <summary>
    ///     Too many failed verification attempts have been made. Further attempts are blocked.
    /// </summary>
    MaxAttemptsReached,
}