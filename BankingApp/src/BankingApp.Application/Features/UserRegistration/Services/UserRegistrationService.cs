namespace BankingApp.Application.Features.UserRegistration.Services;

using BankingApp.Application.Common.Logging;
using BankingApp.Application.Common.Security;
using BankingApp.Application.Common.Utilities;
using BankingApp.Application.Features.Authentication.Repositories;
using BankingApp.Application.Features.UserRegistration.Dtos;
using Domain.Entities;
using Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides user registration operations.
/// </summary>
public class UserRegistrationService : IUserRegistrationService
{
    private const string DefaultLanguage = "en";
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IHashService _hashService;
    private readonly ILogger<UserRegistrationService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRegistrationService" /> class.
    /// </summary>
    /// <param name="authenticationRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="logger">The _logger.</param>
    public UserRegistrationService(
        IAuthenticationRepository authenticationRepository,
        IHashService hashService,
        ILogger<UserRegistrationService> logger)
    {
        _authenticationRepository = authenticationRepository;
        _hashService = hashService;
        _logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Register(RegisterRequest request)
    {
        Error? validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return validationError.Value;
        }

        ErrorOr<User> existingUserResult = _authenticationRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            _logger.RegistrationRejectedEmailAlreadyRegistered();
            return AuthErrors.EmailAlreadyRegistered;
        }

        if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            _logger.RegistrationExistingUserCheckFailed(existingUserResult.FirstError.Description);
            return UserErrors.DatabaseError;
        }

        ErrorOr<User> newUserResult = CreateUserFromRequest(request);
        if (newUserResult.IsError)
        {
            return newUserResult.FirstError;
        }

        ErrorOr<Success> createResult = _authenticationRepository.CreateUser(newUserResult.Value);
        if (createResult.IsError)
        {
            _logger.UserCreationFailedDuringRegistration(createResult.FirstError.Description);
            return UserErrors.UserCreationFailed;
        }

        _logger.UserRegisteredSuccessfully();
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
        if (!hashResult.IsError)
        {
            return new User
            {
                Email = request.Email,
                PasswordHash = hashResult.Value,
                FullName = request.FullName,
                PreferredLanguage = DefaultLanguage,
                Is2FaEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0
            };
        }

        _logger.RegistrationHashGenerationFailed();
        return hashResult.FirstError;
    }
}