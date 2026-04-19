// <copyright file="UserErrors.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the UserErrors class.
// </summary>

using ErrorOr;

namespace BankApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for user management, profile updates, and 2FA operations.
/// </summary>
public static class UserErrors
{
    /// <summary>The user account could not be created in the data store.</summary>
    public static readonly Error UserCreationFailed =
        Error.Failure("user_creation_failed", "Failed to create user account.");

    /// <summary>The newly created user could not be retrieved after creation.</summary>
    public static readonly Error UserRetrievalFailed =
        Error.Failure("user_retrieval_failed", "Failed to retrieve created user.");

    /// <summary>The OAuth link record could not be persisted.</summary>
    public static readonly Error OAuthLinkFailed =
        Error.Failure("oauth_link_failed", "Failed to link OAuth account.");

    /// <summary>A login session could not be created for the user.</summary>
    public static readonly Error SessionCreationFailed =
        Error.Failure("session_failed", "Failed to create session.");

    /// <summary>The user record could not be updated.</summary>
    public static readonly Error UpdateFailed =
        Error.Failure("update_failed", "Could not update user.");

    /// <summary>The password hash could not be persisted.</summary>
    public static readonly Error PasswordUpdateFailed =
        Error.Failure("update_failed", "Could not update password. Please try again.");

    /// <summary>Two-factor authentication could not be enabled for the user.</summary>
    public static readonly Error Enable2FaFailed =
        Error.Failure("update_failed", "Failed to enable 2FA.");

    /// <summary>Two-factor authentication could not be disabled for the user.</summary>
    public static readonly Error Disable2FaFailed =
        Error.Failure("update_failed", "Failed to disable 2FA.");

    /// <summary>The notification preferences could not be saved.</summary>
    public static readonly Error NotificationPreferencesUpdateFailed =
        Error.Failure("update_failed", "Failed to update notification preferences.");

    /// <summary>A transient database error prevented the operation from completing.</summary>
    public static readonly Error DatabaseError =
        Error.Failure("database_error", "A service error occurred. Please try again later.");
}