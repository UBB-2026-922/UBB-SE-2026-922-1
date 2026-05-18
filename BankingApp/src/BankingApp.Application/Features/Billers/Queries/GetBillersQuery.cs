namespace BankingApp.Application.Features.Billers.Queries;

using Contracts.Features.Billers.Dtos;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetBillersQuery : IRequest<ErrorOr<List<BillerDto>>>;

public sealed class GetBillersQueryHandler(IBillerRepository billerRepository)
    : IRequestHandler<GetBillersQuery, ErrorOr<List<BillerDto>>>
{
    public async Task<ErrorOr<List<BillerDto>>> Handle(GetBillersQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Biller> billers = await billerRepository.ListActiveAsync(cancellationToken);

        return billers
            .Select(biller => new BillerDto
            {
                Id = biller.Id,
                Name = biller.Name,
                Category = biller.Category.ToString(),
                LogoUrl = biller.LogoUrl,
                IsActive = biller.IsActive
            })
            .ToList();
    }
}
