namespace BankingApp.Application.Features.Billers.Queries;

using Contracts.Features.Billers.Dtos;
using Domain.Aggregates.SavedBillerAggregate;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetSavedBillersQuery(int UserId) : IRequest<ErrorOr<List<SavedBillerDto>>>;

public sealed class GetSavedBillersQueryHandler(
    ISavedBillerRepository savedBillerRepository,
    IBillerRepository billerRepository)
    : IRequestHandler<GetSavedBillersQuery, ErrorOr<List<SavedBillerDto>>>
{
    public async Task<ErrorOr<List<SavedBillerDto>>> Handle(GetSavedBillersQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<SavedBiller> savedBillers = await savedBillerRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        List<SavedBillerDto> result = [];
        foreach (SavedBiller savedBiller in savedBillers)
        {
            Biller? biller = await billerRepository.GetByIdAsync(savedBiller.BillerId, cancellationToken);
            result.Add(new SavedBillerDto
            {
                Id = savedBiller.Id,
                UserId = savedBiller.UserId,
                BillerId = savedBiller.BillerId,
                BillerName = biller?.Name ?? string.Empty,
                BillerCategory = biller?.Category.ToString() ?? string.Empty,
                LogoUrl = biller?.LogoUrl,
                Nickname = savedBiller.Nickname,
                DefaultReference = savedBiller.DefaultReference,
                CreatedAt = savedBiller.CreatedAt,
                Biller = biller is null ? null : new BillerDto
                {
                    Id = biller.Id,
                    Name = biller.Name,
                    Category = biller.Category.ToString(),
                    LogoUrl = biller.LogoUrl,
                    IsActive = biller.IsActive
                }
            });
        }

        return result;
    }
}
