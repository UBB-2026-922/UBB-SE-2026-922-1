namespace BankingApp.Application.Features.Cards.Commands;

using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record CancelCardCommand(int UserId, int CardId) : IRequest<ErrorOr<Success>>;

public sealed class CancelCardCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    ISystemClock systemClock,
    ILogger<CancelCardCommandHandler> logger)
    : IRequestHandler<CancelCardCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(CancelCardCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(command.UserId, cancellationToken);

        Card? card = accounts
            .SelectMany(account => account.Cards)
            .FirstOrDefault(card => card.Id == command.CardId && card.UserId == command.UserId);

        if (card is null)
        {
            ApplicationLogMessages.CardNotFound(logger, command.CardId, command.UserId);
            return CardErrors.NotFound;
        }

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == command.CardId));
        card.Cancel(systemClock.UtcNow);
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardCancelled(logger, command.CardId, command.UserId);
        return Result.Success;
    }
}
