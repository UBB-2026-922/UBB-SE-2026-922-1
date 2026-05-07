namespace BankingApp.Application.Services.Registration;

using Logging;
using Repositories.Interfaces;
using Security;
using Utilities;
using Domain.Entities;
using Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

using BankingApp.Application.DTOs.Auth;

/// <summary>
///     Provides user registration operations.
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

        ErrorOr<Success> createResult = _authRepository.CreateUser(newUserResult.Value);
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
