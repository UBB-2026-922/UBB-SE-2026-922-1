namespace BankingApp.Application.Features.Transfers.Queries;

using Contracts.Features.Transfers.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetTransferAccountsQuery(int UserId)
    : IRequest<ErrorOr<List<TransferAccountSelectionResponse>>>;

public sealed class GetTransferAccountsQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetTransferAccountsQuery, ErrorOr<List<TransferAccountSelectionResponse>>>
{
    public async Task<ErrorOr<List<TransferAccountSelectionResponse>>> Handle(
        GetTransferAccountsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return accounts
            .Where(account => account.IsActive())
            .Select(account => new TransferAccountSelectionResponse
            {
                Id = account.Id,
                Iban = account.Iban.Value,
                Currency = account.Balance.Currency.Code,
                Balance = account.Balance.Amount,
                AccountName = account.AccountName ?? string.Empty
            })
            .ToList();
    }
}
