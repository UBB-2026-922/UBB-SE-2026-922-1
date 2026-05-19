namespace BankingApp.Application.Features.Beneficiaries.Queries;

using Contracts.Features.Beneficiaries.Dtos;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Repositories;
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
            .Select(beneficiary => new BeneficiaryDto
            {
                Id = beneficiary.Id,
                UserId = beneficiary.UserId,
                Name = beneficiary.Name,
                Iban = beneficiary.Iban.Value,
                BankName = beneficiary.BankName,
                LastTransferDate = beneficiary.LastTransferDate,
                TotalAmountSent = beneficiary.TotalAmountSent,
                TransferCount = beneficiary.TransferCount
            })
            .ToList();
    }
}
