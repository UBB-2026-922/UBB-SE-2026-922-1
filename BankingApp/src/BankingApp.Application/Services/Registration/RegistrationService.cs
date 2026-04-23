// <copyright file="RegistrationService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RegistrationService class.
// </summary>

using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Registration;

/// <summary>
///     Provides user registration operations, including standard and OAuth registration.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private const string DefaultLanguage = "en";
    private readonly IAuthRepository _authRepository;
    private readonly IHashService _hashService;
    private readonly ILogger<RegistrationService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistrationService" /> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="logger">The _logger.</param>
    public RegistrationService(
        IAuthRepository authRepository,
        IHashService hashService,
        ILogger<RegistrationService> logger)
    {
        _authRepository = authRepository;
        _hashService = hashService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Register(RegisterRequest request)
    {
        Error? validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return validationError.Value;
        }

        ErrorOr<User> existingUserResult = _authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            _logger.LogInformation("Registration rejected: email already registered.");
            return AuthErrors.EmailAlreadyRegistered;
        }

        if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            _logger.LogError(
                "Database error while checking existing user: {Error}",
                existingUserResult.FirstError.Description);
            return UserErrors.DatabaseError;
        }

        ErrorOr<User> newUserResult = CreateUserFromRequest(request);
        if (newUserResult.IsError)
        {
            return newUserResult.FirstError;
        }

        ErrorOr<Success> createResult = _authRepository.CreateUser(newUserResult.Value);
        if (createResult.IsError)
        {
            _logger.LogError("User creation failed during registration: {Error}", createResult.FirstError.Description);
            return UserErrors.UserCreationFailed;
        }

        _logger.LogInformation("User registered successfully.");
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> OAuthRegister(OAuthRegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return AuthErrors.InvalidEmail;
        }

        ErrorOr<OAuthLink> existingLinkResult = _authRepository.FindOAuthLink(request.Provider, request.ProviderToken);
        if (!existingLinkResult.IsError)
        {
            return AuthErrors.OAuthAlreadyRegistered;
        }

        if (existingLinkResult.FirstError.Type != ErrorType.NotFound)
        {
            _logger.LogError(
                "Database error while checking OAuth link: {Error}",
                existingLinkResult.FirstError.Description);
            return UserErrors.DatabaseError;
        }

        int targetUserId;
        ErrorOr<User> existingUserResult = _authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            targetUserId = existingUserResult.Value.Id;
        }
        else if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            _logger.LogError(
                "Database error while checking existing user during OAuth register: {Error}",
                existingUserResult.FirstError.Description);
            return UserErrors.DatabaseError;
        }
        else
        {
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PreferredLanguage = DefaultLanguage,
                Is2FaEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0,
            };
            if (_authRepository.CreateUser(newUser).IsError)
            {
                return UserErrors.UserCreationFailed;
            }

            ErrorOr<User> savedUserResult = _authRepository.FindUserByEmail(request.Email);
            if (savedUserResult.IsError)
            {
                return UserErrors.UserRetrievalFailed;
            }

            targetUserId = savedUserResult.Value.Id;
        }

        var newLink = new OAuthLink
        {
            UserId = targetUserId,
            Provider = request.Provider,
            ProviderUserId = request.ProviderToken,
            ProviderEmail = request.Email,
        };
        if (_authRepository.CreateOAuthLink(newLink).IsError)
        {
            return UserErrors.OAuthLinkFailed;
        }

        return Result.Success;
    }

    private static Error? ValidateRegistration(RegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return AuthErrors.InvalidEmail;
        }

        if (!ValidationUtilities.IsStrongPassword(request.Password))
        {
            return ProfileErrors.WeakPassword;
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return ProfileErrors.FullNameRequired;
        }

        return null;
    }

    private ErrorOr<User> CreateUserFromRequest(RegisterRequest request)
    {
        ErrorOr<string> hashResult = _hashService.GetHash(request.Password);
        if (hashResult.IsError)
        {
            _logger.LogError("Hash generation failed during registration.");
            return hashResult.FirstError;
        }

        return new User
        {
            Email = request.Email,
            PasswordHash = hashResult.Value,
            FullName = request.FullName,
            PreferredLanguage = DefaultLanguage,
            Is2FaEnabled = false,
            IsLocked = false,
            FailedLoginAttempts = 0,
        };
    }
}