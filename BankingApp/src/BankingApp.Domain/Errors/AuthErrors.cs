// <copyright file="AuthErrors.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AuthErrors class.
// </summary>

using ErrorOr;

namespace BankingApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for authentication and login flows.
/// </summary>
public static class AuthErrors
{
    /// <summary>The provided email address has an invalid format.</summary>
    public static readonly Error InvalidEmail =
        Error.Validation("invalid_email", "Invalid email format.");

    /// <summary>The email/password combination did not match any account.</summary>
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("invalid_credentials", "Invalid email or password.");

    /// <summary>The account was locked after too many consecutive failures.</summary>
    public static readonly Error TooManyFailedAttempts =
        Error.Unauthorized("invalid_credentials", "Too many failed attempts. Please try again later.");

    /// <summary>The account is temporarily locked; the lockout window is still active.</summary>
    public static readonly Error AccountLocked =
        Error.Forbidden("account_locked", "Account is locked. Try again later.");

    /// <summary>The account was just locked because the maximum failed-attempt threshold was reached.</summary>
    public static readonly Error AccountLockedTooManyAttempts =
        Error.Forbidden("account_locked", "Account locked due to too many failed attempts.");

    /// <summary>The supplied OTP code was invalid or has expired.</summary>
    public static readonly Error InvalidOtp =
        Error.Unauthorized("invalid_otp", "Invalid or expired OTP code.");

    /// <summary>The maximum number of failed OTP attempts was reached; the challenge has been invalidated.</summary>
    public static readonly Error OtpAttemptsExceeded =
        Error.Unauthorized("otp_attempts_exceeded", "Too many incorrect OTP entries. Please restart login.");

    /// <summary>The requested OAuth provider is not supported.</summary>
    public static readonly Error UnsupportedProvider =
        Error.Validation("unsupported_provider", "Unsupported OAuth Provider.");

    /// <summary>The Google ID token could not be validated.</summary>
    public static readonly Error InvalidGoogleToken =
        Error.Validation("invalid_google_token", "Invalid Google authentication token.");

    /// <summary>No user account exists for the given identifier.</summary>
    public static readonly Error UserNotFound =
        Error.NotFound("user_not_found", "User not found.");

    /// <summary>An account with the given email address already exists.</summary>
    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("email_registered", "Email is already registered.");

    /// <summary>The OAuth account is already registered; the user should log in instead.</summary>
    public static readonly Error OAuthAlreadyRegistered =
        Error.Conflict("oauth_already_registered", "This OAuth account is already registered. Please login.");

    /// <summary>The Google OAuth provider is already linked to this account.</summary>
    public static readonly Error OAuthAlreadyLinked =
        Error.Conflict("oauth_already_linked", "Google OAuth is already linked.");

    /// <summary>No linked Google OAuth account was found for this user.</summary>
    public static readonly Error OAuthLinkNotFound =
        Error.NotFound("oauth_link_not_found", "Google OAuth is not linked.");

}
