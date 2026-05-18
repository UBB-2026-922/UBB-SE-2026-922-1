namespace BankingApp.Application.Features.AccountOverview.Queries;

using Common.Logging;
using Contracts.Features.AccountOverview.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record GetDashboardQuery(int UserId)
    : IRequest<ErrorOr<AccountOverviewDto>>;

public sealed class GetDashboardQueryHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ILogger<GetDashboardQueryHandler> logger)
    : IRequestHandler<GetDashboardQuery, ErrorOr<AccountOverviewDto>>
{
    private const int RecentTransactionLimit = 5;

    public async Task<ErrorOr<AccountOverviewDto>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            logger.AccountOverviewUserNotFound(query.UserId);
            return UserErrors.NotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);

        var result = new AccountOverviewDto
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = user.FullName,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber,
                Is2FaEnabled = identity?.Is2FaEnabled ?? false
            },
            UnreadNotificationCount = user.Notifications.Count(n => !n.IsRead)
        };

        try
        {
            IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(user.Id, cancellationToken);
            foreach (Account account in accounts)
            {
                foreach (Card card in account.Cards)
                {
                    result.Cards.Add(new CardDto
                    {
                        CardNumber = card.GetMaskedNumber(),
                        AccountName = account.AccountName,
                        AccountBalance = account.Balance.Amount,
                        CardholderName = card.CardholderName,
                        CardType = card.CardType,
                        CardBrand = card.CardBrand,
                        ExpiryDate = card.ExpiryDate,
                        Status = card.Status,
                        IsContactlessEnabled = card.IsContactlessEnabled,
                        IsOnlineEnabled = card.IsOnlineEnabled
                    });
                }

                IReadOnlyCollection<Transaction> transactions = await transactionRepository.ListByAccountIdAsync(account.Id, cancellationToken);
                foreach (Transaction txn in transactions.OrderByDescending(t => t.CreatedAt).Take(RecentTransactionLimit))
                {
                    result.RecentTransactions.Add(new TransactionDto
                    {
                        Id = txn.Id,
                        Direction = txn.Direction,
                        Amount = txn.Amount.Amount,
                        Currency = txn.Amount.Currency.Code,
                        Description = txn.Description,
                        MerchantName = txn.MerchantName,
                        CounterpartyName = txn.CounterpartyName,
                        Status = txn.Status,
                        CreatedAt = txn.CreatedAt
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logger.AccountOverviewFetchAccountsFailed(user.Id, ex.Message);
            return Error.Failure("dashboard.load_failed", "Failed to load dashboard data.");
        }

        result.RecentTransactions = result.RecentTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(RecentTransactionLimit)
            .ToList();

        return result;
    }
}
