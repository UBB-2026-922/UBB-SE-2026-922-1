namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Logging;
using Contracts.Features.UserProfile.Dtos;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record GetActiveSessionsQuery(int UserId)
    : IRequest<ErrorOr<List<SessionDto>>>;

public sealed class GetActiveSessionsQueryHandler(
    IIdentityRepository identityRepository,
    ILogger<GetActiveSessionsQueryHandler> logger)
    : IRequestHandler<GetActiveSessionsQuery, ErrorOr<List<SessionDto>>>
{
    public async Task<ErrorOr<List<SessionDto>>> Handle(GetActiveSessionsQuery query, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.GetSessionsUserNotFound(logger, query.UserId);
            return UserErrors.NotFound;
        }

        return identity.Sessions
            .Where(session => !session.IsRevoked)
            .Select(session => new SessionDto
            {
                Id = session.Id,
                DeviceInfo = session.DeviceInfo,
                Browser = session.Browser,
                IpAddress = session.IpAddress,
                LastActiveAt = session.LastActiveAt
            })
            .ToList();
    }
}
