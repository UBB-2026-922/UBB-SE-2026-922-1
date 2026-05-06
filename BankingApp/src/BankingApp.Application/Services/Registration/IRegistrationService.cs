// <copyright file="IRegistrationService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IRegistrationService interface.
// </summary>

using ErrorOr;

namespace BankingApp.Application.Services.Registration;

using DTOs.Auth;

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
    ErrorOr<Success> Register(RegisterRequest request);
}