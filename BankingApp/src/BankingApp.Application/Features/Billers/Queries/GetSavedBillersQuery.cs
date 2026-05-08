namespace BankingApp.Application.Features.Billers.Queries;

using Domain.Aggregates.SavedBillerAggregate;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using Dtos;
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
        foreach (SavedBiller sb in savedBillers)
        {
            Biller? biller = await billerRepository.GetByIdAsync(sb.BillerId, cancellationToken);
            result.Add(new SavedBillerDto
            {
                Id = sb.Id,
                UserId = sb.UserId,
                BillerId = sb.BillerId,
                BillerName = biller?.Name ?? string.Empty,
                BillerCategory = biller?.Category.ToString() ?? string.Empty,
                LogoUrl = biller?.LogoUrl,
                Nickname = sb.Nickname,
                DefaultReference = sb.DefaultReference,
                CreatedAt = sb.CreatedAt,
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
