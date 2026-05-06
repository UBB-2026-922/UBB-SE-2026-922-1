namespace BankingApp.Application.Services.Registration;

using ErrorOr;

using BankingApp.Application.DTOs.Auth;

/// <summary>
///     Defines operations for user registration.
/// </summary>
public interface IRegistrationService
{
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
}
