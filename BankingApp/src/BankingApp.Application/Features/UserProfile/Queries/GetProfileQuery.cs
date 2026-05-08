namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Contracts;
using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record GetProfileQuery(int UserId)
    : IRequest<ErrorOr<ProfileDto>>;

public sealed class GetProfileQueryHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    ILogger<GetProfileQueryHandler> logger)
    : IRequestHandler<GetProfileQuery, ErrorOr<ProfileDto>>
{
    public async Task<ErrorOr<ProfileDto>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            logger.ProfileFetchUserNotFound(query.UserId);
            return UserErrors.NotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);

        return new ProfileDto
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            Nationality = user.Nationality,
            PreferredLanguage = user.PreferredLanguage,
            Is2FaEnabled = identity?.Is2FaEnabled ?? false,
            Preferred2FaMethod = identity?.Preferred2FaMethod
        };
    }
}
