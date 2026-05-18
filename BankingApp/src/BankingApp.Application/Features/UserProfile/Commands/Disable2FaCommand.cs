namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

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
            logger.DisableTwoFactorUserNotFound(command.UserId);
            return UserErrors.NotFound;
        }

        identity.Disable2Fa();
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DisableTwoFactorSucceeded(command.UserId);
        return Result.Success;
    }
}
