namespace BankingApp.Application.Features.Authentication.Commands;

using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record LogoutCommand(string Token)
    : IRequest<ErrorOr<Success>>;

public sealed class LogoutCommandHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetBySessionTokenAsync(command.Token, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.LogoutSessionNotFound(logger);
            return AuthErrors.SessionNotFound;
        }

        Session? session = identity.Sessions.FirstOrDefault(s => s.Token == command.Token && !s.IsRevoked);
        if (session is null)
        {
            ApplicationLogMessages.LogoutSessionNotFound(logger);
            return AuthErrors.SessionNotFound;
        }

        session.Revoke();
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.UserLoggedOut(logger, identity.UserId);
        return Result.Success;
    }
}
