namespace BankingApp.Application.Features.Beneficiaries.Commands;

using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;
using Shared.Persistence;

public sealed record UpdateBeneficiaryCommand(int UserId, int BeneficiaryId, string Name, string Iban, string? BankName)
    : IRequest<ErrorOr<Success>>;

public sealed class UpdateBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBeneficiaryCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateBeneficiaryCommand command, CancellationToken cancellationToken)
    {
        Beneficiary? beneficiary = await beneficiaryRepository.GetByIdAsync(command.BeneficiaryId, cancellationToken);
        if (beneficiary is null || beneficiary.UserId != command.UserId)
        {
            return BeneficiaryErrors.NotFound;
        }

        ErrorOr<Iban> ibanResult = Iban.Create(command.Iban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        beneficiary.Update(command.Name.Trim(), ibanResult.Value, command.BankName);
        await beneficiaryRepository.UpdateAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
