namespace BankingApp.Application.Features.Billers.Commands;

using Common.Contracts;
using Common.Utilities;
using Domain.Aggregates.SavedBillerAggregate;
using Domain.Common.Errors;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record SaveBillerCommand(
    int UserId,
    int BillerId,
    string? Nickname,
    string? DefaultReference)
    : IRequest<ErrorOr<SavedBillerDto>>;

public sealed class SaveBillerCommandHandler(
    IBillerRepository billerRepository,
    ISavedBillerRepository savedBillerRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IRequestHandler<SaveBillerCommand, ErrorOr<SavedBillerDto>>
{
    public async Task<ErrorOr<SavedBillerDto>> Handle(SaveBillerCommand command, CancellationToken cancellationToken)
    {
        Biller? biller = await billerRepository.GetByIdAsync(command.BillerId, cancellationToken);
        if (biller is null)
        {
            return BillerErrors.BillerNotFound;
        }

        IReadOnlyCollection<SavedBiller> existing = await savedBillerRepository.ListByUserIdAsync(command.UserId, cancellationToken);
        if (existing.Any(s => s.BillerId == command.BillerId))
        {
            return BillerErrors.BillerAlreadySaved;
        }

        var saved = SavedBiller.Create(
            command.UserId,
            command.BillerId,
            command.Nickname,
            command.DefaultReference,
            clock.UtcNow);

        await savedBillerRepository.AddAsync(saved, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SavedBillerDto
        {
            Id = saved.Id,
            UserId = saved.UserId,
            BillerId = saved.BillerId,
            BillerName = biller.Name,
            BillerCategory = biller.Category.ToString(),
            LogoUrl = biller.LogoUrl,
            Nickname = saved.Nickname,
            DefaultReference = saved.DefaultReference,
            CreatedAt = saved.CreatedAt,
            Biller = new BillerDto
            {
                Id = biller.Id,
                Name = biller.Name,
                Category = biller.Category.ToString(),
                LogoUrl = biller.LogoUrl,
                IsActive = biller.IsActive
            }
        };
    }
}
