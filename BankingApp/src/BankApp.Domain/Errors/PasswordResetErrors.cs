// <copyright file="PasswordResetErrors.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the PasswordResetErrors class.
// </summary>

using ErrorOr;

namespace BankApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for the password reset flow.
/// </summary>
public static class PasswordResetErrors
{
    /// <summary>The supplied reset token is missing, malformed, or not found.</summary>
    public static readonly Error TokenInvalid =
        Error.Validation("token_invalid", "The reset token is invalid.");

    /// <summary>The reset token was already consumed by a previous reset operation.</summary>
    public static readonly Error TokenAlreadyUsed =
        Error.Validation("token_already_used", "The reset token has already been used.");

    /// <summary>The reset token's validity window has passed.</summary>
    public static readonly Error TokenExpired =
        Error.Validation("token_expired", "The reset token has expired.");

    /// <summary>The password was updated but the token could not be marked as used; replay risk exists.</summary>
    public static readonly Error ResetFailedTokenNotInvalidated =
        Error.Failure("reset_failed", "Password was updated but the token could not be invalidated.");

    /// <summary>The password was updated but existing sessions could not be invalidated.</summary>
    public static readonly Error ResetFailedSessionsNotInvalidated =
        Error.Failure("reset_failed", "Password was updated but active sessions could not be invalidated.");

    /// <summary>The reset token record could not be persisted to the data store.</summary>
    public static readonly Error SaveTokenFailed =
        Error.Failure(description: "Failed to save password reset token.");
}