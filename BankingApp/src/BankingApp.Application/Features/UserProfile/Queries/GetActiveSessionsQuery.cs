namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

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
            logger.GetSessionsUserNotFound(query.UserId);
            return UserErrors.NotFound;
        }

        return identity.Sessions
            .Where(s => !s.IsRevoked)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                DeviceInfo = s.DeviceInfo,
                Browser = s.Browser,
                IpAddress = s.IpAddress,
                LastActiveAt = s.LastActiveAt
            })
            .ToList();
    }
}
