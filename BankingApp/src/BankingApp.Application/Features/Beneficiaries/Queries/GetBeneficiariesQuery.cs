namespace BankingApp.Application.Features.Beneficiaries.Queries;

using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record GetBeneficiariesQuery(int UserId)
    : IRequest<ErrorOr<List<BeneficiaryDto>>>;

public sealed class GetBeneficiariesQueryHandler(IBeneficiaryRepository beneficiaryRepository)
    : IRequestHandler<GetBeneficiariesQuery, ErrorOr<List<BeneficiaryDto>>>
{
    public async Task<ErrorOr<List<BeneficiaryDto>>> Handle(GetBeneficiariesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Beneficiary> beneficiaries = await beneficiaryRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return beneficiaries
            .Select(b => new BeneficiaryDto
            {
                Id = b.Id,
                UserId = b.UserId,
                Name = b.Name,
                Iban = b.Iban.Value,
                BankName = b.BankName,
                LastTransferDate = b.LastTransferDate,
                TotalAmountSent = b.TotalAmountSent,
                TransferCount = b.TransferCount
            })
            .ToList();
    }
}
