namespace BankingApp.Application.Features.Transfers.Queries;

using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;
using Dtos;
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
            .Where(a => a.IsActive())
            .Select(a => new TransferAccountSelectionResponse
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
