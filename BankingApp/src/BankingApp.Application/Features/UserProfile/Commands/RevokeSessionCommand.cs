namespace BankingApp.Application.Features.UserProfile.Commands;

using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

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
            ApplicationLogMessages.RevokeSessionUserNotFound(logger, command.UserId);
            return UserErrors.NotFound;
        }

        if (!identity.TryRevokeSession(command.SessionId))
        {
            ApplicationLogMessages.RevokeSessionFailed(logger, command.SessionId, command.UserId);
            return AuthErrors.SessionNotFound;
        }

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.SessionRevoked(logger, command.SessionId, command.UserId);
        return Result.Success;
    }
}
