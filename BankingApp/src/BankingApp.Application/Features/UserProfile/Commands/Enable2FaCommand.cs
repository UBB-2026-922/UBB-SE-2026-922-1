namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record Enable2FaCommand(int UserId, TwoFactorMethod Method)
    : IRequest<ErrorOr<Success>>;

public sealed class Enable2FaCommandHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    ILogger<Enable2FaCommandHandler> logger)
    : IRequestHandler<Enable2FaCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(Enable2FaCommand command, CancellationToken cancellationToken)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (identity is null)
        {
            logger.EnableTwoFactorUserNotFound(command.UserId);
            return UserErrors.NotFound;
        }

        identity.Enable2Fa(command.Method);
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.EnableTwoFactorSucceeded(command.UserId, command.Method);
        return Result.Success;
    }
}
