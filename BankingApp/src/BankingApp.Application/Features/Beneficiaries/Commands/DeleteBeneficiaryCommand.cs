namespace BankingApp.Application.Features.Beneficiaries.Commands;

using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record DeleteBeneficiaryCommand(int UserId, int BeneficiaryId)
    : IRequest<ErrorOr<Success>>;

public sealed class DeleteBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBeneficiaryCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteBeneficiaryCommand command, CancellationToken cancellationToken)
    {
        Beneficiary? beneficiary = await beneficiaryRepository.GetByIdAsync(command.BeneficiaryId, cancellationToken);
        if (beneficiary is null || beneficiary.UserId != command.UserId)
        {
            return BeneficiaryErrors.NotFound;
        }

        await beneficiaryRepository.DeleteAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
