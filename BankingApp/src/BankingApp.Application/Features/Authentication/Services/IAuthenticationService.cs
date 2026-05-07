namespace BankingApp.Application.Features.Authentication.Services;

using ErrorOr;

using BankingApp.Application.Features.Authentication.Dtos;
using BankingApp.Application.Features.UserRegistration.Dtos;

/// <summary>
///     Defines operations for user authentication, registration, and password management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>
    ///     A <see cref="FullLogin" /> if credentials are correct and 2FA is not required,
    ///     a <see cref="RequiresTwoFactor" /> if 2FA is enabled and an OTP has been dispatched,
    ///     a validation error with code <c>invalid_email</c> if the email format is invalid,
    ///     an unauthorized error with code <c>invalid_credentials</c> if the email/password is wrong,
    ///     or a forbidden error with code <c>account_locked</c> if the account is locked.
    /// </returns>
    public ErrorOr<LoginSuccess> Login(LoginRequest request);

    /// <summary>
    ///     Registers a new user with email and password.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success,
    ///     a validation error with code <c>invalid_email</c> if the email format is invalid,
    ///     a validation error with code <c>weak_password</c> if the password does not meet strength requirements,
    ///     a validation error with code <c>full_name_required</c> if the full name is empty,
    ///     a conflict error with code <c>email_registered</c> if the email is already in use,
    ///     or a failure error if user creation fails.
    /// </returns>
    public ErrorOr<Success> Register(RegisterRequest request);

    /// <summary>
    ///     Verifies an OTP for 2FA.
    /// </summary>
    /// <param name="request">The OTP verification details.</param>
    /// <returns>
    ///     A <see cref="FullLogin" /> on success,
    ///     a not-found error if the user does not exist,
    ///     or an unauthorized error with code <c>invalid_otp</c> if the code is invalid or expired.
    /// </returns>
    public ErrorOr<LoginSuccess> VerifyOtp(VerifyOtpRequest request);

    /// <summary>
    ///     Resends an OTP to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="method">The delivery method (e.g., "email").</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success,
    ///     or an error if the user does not exist or OTP generation fails.
    /// </returns>
    public ErrorOr<Success> ResendOtp(int userId, string method);

    /// <summary>
    ///     Initiates a password reset flow for the given email address.
    /// </summary>
    /// <param name="email">The email address of the user requesting a reset.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success,
    ///     or an error if no account exists for the given email or the token could not be saved.
    /// </returns>
    public ErrorOr<Success> RequestPasswordReset(string email);

    /// <summary>
    ///     Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new plain-text password.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> if the password was reset and all post-reset steps succeeded,
    ///     a validation error with code <c>token_expired</c> if the token has expired,
    ///     a validation error with code <c>token_already_used</c> if the token was already consumed,
    ///     a validation error with code <c>token_invalid</c> if the token does not exist or the password update failed,
    ///     or a failure error with code <c>reset_failed</c> if the password was updated but a post-reset
    ///     security step (marking the token as used or invalidating active sessions) failed.
    /// </returns>
    public ErrorOr<Success> ResetPassword(string token, string newPassword);

    /// <summary>
    ///     Logs out the user by invalidating the specified session token.
    /// </summary>
    /// <param name="token">The session token to invalidate.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success,
    ///     or an error if no active session exists for the given token.
    /// </returns>
    public ErrorOr<Success> Logout(string token);

    /// <summary>
    ///     Validates a password reset token without consuming it.
    /// </summary>
    /// <param name="token">The reset token to verify.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> if the token is valid,
    ///     a validation error with code <c>token_expired</c> if the token has expired,
    ///     a validation error with code <c>token_already_used</c> if the token was already consumed,
    ///     or a validation error with code <c>token_invalid</c> if the token does not exist.
    /// </returns>
    public ErrorOr<Success> VerifyResetToken(string token);
}
