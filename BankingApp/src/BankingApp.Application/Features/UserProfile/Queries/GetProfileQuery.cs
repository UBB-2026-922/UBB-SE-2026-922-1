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

public sealed record GetProfileQuery(int UserId)
    : IRequest<ErrorOr<ProfileDto>>;

public sealed class GetProfileQueryHandler(
    IUserRepository userRepository,
    ILogger<GetProfileQueryHandler> logger)
    : IRequestHandler<GetProfileQuery, ErrorOr<ProfileDto>>
{
    public async Task<ErrorOr<ProfileDto>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.ProfileFetchUserNotFound(logger, query.UserId);
            return UserErrors.NotFound;
        }

        return new ProfileDto
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            Nationality = user.Nationality,
            PreferredLanguage = user.PreferredLanguage
        };
    }
}
