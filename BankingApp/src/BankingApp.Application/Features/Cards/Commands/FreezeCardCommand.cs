namespace BankingApp.Application.Features.Cards.Commands;

using Common.Contracts;
using Common.Logging;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record FreezeCardCommand(int UserId, int CardId) : IRequest<ErrorOr<Success>>;

public sealed class FreezeCardCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    ILogger<FreezeCardCommandHandler> logger)
    : IRequestHandler<FreezeCardCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(FreezeCardCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(command.UserId, cancellationToken);

        Card? card = accounts
            .SelectMany(account => account.Cards)
            .FirstOrDefault(card => card.Id == command.CardId && card.UserId == command.UserId);

        if (card is null)
        {
            logger.CardNotFound(command.CardId, command.UserId);
            return CardErrors.NotFound;
        }

        if (card.Status == CardStatus.Frozen)
        {
            return CardErrors.AlreadyFrozen;
        }

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == command.CardId));
        card.Freeze();
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CardFrozen(command.CardId, command.UserId);
        return Result.Success;
    }
}
