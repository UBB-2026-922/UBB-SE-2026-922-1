namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Logging;
using Contracts.Features.UserProfile.Dtos;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record GetNotificationPreferencesQuery(int UserId)
    : IRequest<ErrorOr<List<NotificationPreferenceDto>>>;

public sealed class GetNotificationPreferencesQueryHandler(
    IUserRepository userRepository,
    ILogger<GetNotificationPreferencesQueryHandler> logger)
    : IRequestHandler<GetNotificationPreferencesQuery, ErrorOr<List<NotificationPreferenceDto>>>
{
    public async Task<ErrorOr<List<NotificationPreferenceDto>>> Handle(
        GetNotificationPreferencesQuery query,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.NotificationPreferencesFetchUserNotFound(logger, query.UserId);
            return UserErrors.NotFound;
        }

        return user.NotificationPreferences
            .Select(notificationPreference => new NotificationPreferenceDto
            {
                Id = notificationPreference.Id,
                UserId = notificationPreference.UserId,
                Category = notificationPreference.Category,
                PushEnabled = notificationPreference.PushEnabled,
                EmailEnabled = notificationPreference.EmailEnabled,
                SmsEnabled = notificationPreference.SmsEnabled,
                MinAmountThreshold = notificationPreference.MinAmountThreshold
            })
            .ToList();
    }
}
