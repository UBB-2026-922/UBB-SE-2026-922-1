namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Logging;
using Common.Validation;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record UpdateProfileCommand(
    int UserId,
    string? FullName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Address,
    string? Nationality,
    string? PreferredLanguage)
    : IRequest<ErrorOr<Success>>;

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<UpdateProfileCommandHandler> logger)
    : IRequestHandler<UpdateProfileCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.ProfileUpdateUserNotFound(logger, command.UserId);
            return UserErrors.NotFound;
        }

        string fullName = command.FullName?.Trim() ?? user.FullName;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ProfileErrors.FullNameRequired;
        }

        if (command.PhoneNumber is not null && !InputRules.IsValidPhoneNumber(command.PhoneNumber))
        {
            return ProfileErrors.InvalidPhone;
        }

        string preferredLanguage = command.PreferredLanguage?.Trim() ?? user.PreferredLanguage;
        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return ProfileErrors.PreferredLanguageRequired;
        }

        user.UpdateProfile(
            fullName,
            command.PhoneNumber ?? user.PhoneNumber,
            command.DateOfBirth ?? user.DateOfBirth,
            command.Address?.Trim() ?? user.Address,
            command.Nationality?.Trim() ?? user.Nationality,
            preferredLanguage,
            clock.UtcNow);

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
