namespace BankingApp.Application.Features.Billers.Commands;

using Domain.Aggregates.SavedBillerAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Shared.Persistence;

public sealed record DeleteSavedBillerCommand(int UserId, int SavedBillerId) : IRequest<ErrorOr<Success>>;

public sealed class DeleteSavedBillerCommandHandler(
    ISavedBillerRepository savedBillerRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSavedBillerCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSavedBillerCommand command, CancellationToken cancellationToken)
    {
        SavedBiller? savedBiller = await savedBillerRepository.GetByIdAsync(command.SavedBillerId, cancellationToken);
        if (savedBiller is null || savedBiller.UserId != command.UserId)
        {
            return BillerErrors.SavedBillerNotFound;
        }

        await savedBillerRepository.DeleteAsync(savedBiller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
