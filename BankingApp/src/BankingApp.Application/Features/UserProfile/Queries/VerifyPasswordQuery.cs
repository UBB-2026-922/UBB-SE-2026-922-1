namespace BankingApp.Application.Features.UserProfile.Queries;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Security;

public sealed record VerifyPasswordQuery(int UserId, string Password)
    : IRequest<ErrorOr<bool>>;

public sealed class VerifyPasswordQueryHandler(
    IIdentityRepository identityRepository,
    IHashService hashService,
    ILogger<VerifyPasswordQueryHandler> logger)
    : IRequestHandler<VerifyPasswordQuery, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(VerifyPasswordQuery query, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (identity is null)
        {
            logger.PasswordChangeUserNotFound(query.UserId);
            return UserErrors.NotFound;
        }

        if (identity.PasswordHash is null)
        {
            return false;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(query.Password, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            logger.PasswordChangeHashVerificationFailed(query.UserId);
            return verifyResult.FirstError;
        }

        return verifyResult.Value;
    }
}
