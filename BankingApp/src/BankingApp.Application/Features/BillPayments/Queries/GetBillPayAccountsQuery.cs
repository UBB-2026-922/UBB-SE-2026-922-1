namespace BankingApp.Application.Features.BillPayments.Queries;

using Contracts.Features.BillPayments.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

/// <summary>
///     Returns the list of active accounts for the current user to use as a funding source
///     on the bill-payment form.
/// </summary>
public sealed record GetBillPayAccountsQuery(int UserId)
    : IRequest<ErrorOr<List<AccountDto>>>;

public sealed class GetBillPayAccountsQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetBillPayAccountsQuery, ErrorOr<List<AccountDto>>>
{
    public async Task<ErrorOr<List<AccountDto>>> Handle(
        GetBillPayAccountsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Account> accounts =
            await accountRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return accounts
            .Where(account => account.IsActive())
            .Select(account =>
            {
                Domain.Aggregates.AccountAggregate.Entities.Card? primaryCard =
                    account.Cards.FirstOrDefault(card => card.IsActive());

                string? lastFour = primaryCard is not null && primaryCard.CardNumber.Length >= 4
                    ? primaryCard.CardNumber[^4..]
                    : null;

                return new AccountDto
                {
                    Id = account.Id,
                    Iban = account.Iban.Value,
                    Currency = account.Balance.Currency.Code,
                    Balance = account.Balance.Amount,
                    AccountName = account.AccountName ?? string.Empty,
                    CardLastFourDigits = lastFour
                };
            })
            .ToList();
    }
}
