namespace BankingApp.Application.Features.BillPayments.Queries;

using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;
using Dtos;
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
            .Where(a => a.IsActive())
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Iban = a.Iban.Value,
                Currency = a.Balance.Currency.Code,
                Balance = a.Balance.Amount,
                AccountName = a.AccountName ?? string.Empty
            })
            .ToList();
    }
}
