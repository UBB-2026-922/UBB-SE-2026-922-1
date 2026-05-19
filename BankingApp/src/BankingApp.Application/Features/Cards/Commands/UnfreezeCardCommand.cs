namespace BankingApp.Application.Features.Cards.Commands;

using Common.Logging;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record UnfreezeCardCommand(int UserId, int CardId) : IRequest<ErrorOr<Success>>;

public sealed class UnfreezeCardCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    ILogger<UnfreezeCardCommandHandler> logger)
    : IRequestHandler<UnfreezeCardCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UnfreezeCardCommand command, CancellationToken cancellationToken)
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

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        if (card.Status != CardStatus.Frozen)
        {
            return CardErrors.NotFrozen;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == command.CardId));
        card.Unfreeze();
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CardUnfrozen(command.CardId, command.UserId);
        return Result.Success;
    }
}
