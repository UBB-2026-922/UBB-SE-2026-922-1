namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record RevokeSessionCommand(int UserId, int SessionId)
    : IRequest<ErrorOr<Success>>;

public sealed class RevokeSessionCommandHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    ILogger<RevokeSessionCommandHandler> logger)
    : IRequestHandler<RevokeSessionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (identity is null)
        {
            logger.RevokeSessionUserNotFound(command.UserId);
            return UserErrors.NotFound;
        }

        if (!identity.TryRevokeSession(command.SessionId))
        {
            logger.RevokeSessionFailed(command.SessionId, command.UserId);
            return AuthErrors.SessionNotFound;
        }

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.SessionRevoked(command.SessionId, command.UserId);
        return Result.Success;
    }
}
