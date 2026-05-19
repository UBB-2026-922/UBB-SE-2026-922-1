namespace BankingApp.Application.Features.UserProfile.Commands;

using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record Disable2FaCommand(int UserId)
    : IRequest<ErrorOr<Success>>;

public sealed class Disable2FaCommandHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    ILogger<Disable2FaCommandHandler> logger)
    : IRequestHandler<Disable2FaCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(Disable2FaCommand command, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.DisableTwoFactorUserNotFound(logger, command.UserId);
            return UserErrors.NotFound;
        }

        identity.Disable2Fa();
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.DisableTwoFactorSucceeded(logger, command.UserId);
        return Result.Success;
    }
}
