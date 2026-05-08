namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Logging;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

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
            logger.NotificationPreferencesFetchUserNotFound(query.UserId);
            return UserErrors.NotFound;
        }

        return user.NotificationPreferences
            .Select(p => new NotificationPreferenceDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Category = p.Category,
                PushEnabled = p.PushEnabled,
                EmailEnabled = p.EmailEnabled,
                SmsEnabled = p.SmsEnabled,
                MinAmountThreshold = p.MinAmountThreshold
            })
            .ToList();
    }
}
