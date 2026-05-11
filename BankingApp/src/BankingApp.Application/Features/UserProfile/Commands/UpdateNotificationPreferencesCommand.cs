namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Contracts;
using Common.Logging;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record UpdateNotificationPreferencesCommand(int UserId, List<NotificationPreferenceDto> Preferences)
    : IRequest<ErrorOr<Success>>;

public sealed class UpdateNotificationPreferencesCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateNotificationPreferencesCommandHandler> logger)
    : IRequestHandler<UpdateNotificationPreferencesCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateNotificationPreferencesCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            logger.NotificationPreferencesUpdateUserNotFound(command.UserId);
            return UserErrors.NotFound;
        }

        foreach (NotificationPreferenceDto pref in command.Preferences)
        {
            user.SetNotificationPreference(
                pref.Category,
                pref.PushEnabled,
                pref.EmailEnabled,
                pref.SmsEnabled,
                pref.MinAmountThreshold);
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
