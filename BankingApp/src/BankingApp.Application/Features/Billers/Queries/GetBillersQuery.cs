namespace BankingApp.Application.Features.Billers.Queries;

using Domain.ReferenceData.Billers;
using Domain.Repositories;
using Dtos;
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
            .Select(b => new BillerDto
            {
                Id = b.Id,
                Name = b.Name,
                Category = b.Category.ToString(),
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive
            })
            .ToList();
    }
}
