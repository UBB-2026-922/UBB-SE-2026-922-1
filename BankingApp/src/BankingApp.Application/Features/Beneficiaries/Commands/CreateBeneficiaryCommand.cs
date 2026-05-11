namespace BankingApp.Application.Features.Beneficiaries.Commands;

using Common.Contracts;
using Common.Logging;
using Common.Utilities;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record CreateBeneficiaryCommand(int UserId, string Name, string Iban, string? BankName)
    : IRequest<ErrorOr<BeneficiaryDto>>;

public sealed class CreateBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaryRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<CreateBeneficiaryCommandHandler> logger)
    : IRequestHandler<CreateBeneficiaryCommand, ErrorOr<BeneficiaryDto>>
{
    public async Task<ErrorOr<BeneficiaryDto>> Handle(CreateBeneficiaryCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<Iban> ibanResult = Iban.Create(command.Iban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        IReadOnlyCollection<Beneficiary> existing = await beneficiaryRepository.ListByUserIdAsync(command.UserId, cancellationToken);
        if (existing.Any(b => b.Iban.Value.Equals(command.Iban, StringComparison.OrdinalIgnoreCase)))
        {
            return BeneficiaryErrors.Duplicate;
        }

        var beneficiary = Beneficiary.Create(command.UserId, command.Name.Trim(), ibanResult.Value, command.BankName, clock.UtcNow);
        await beneficiaryRepository.AddAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.BeneficiaryCreated(beneficiary.Id, command.UserId);

        return new BeneficiaryDto
        {
            Id = beneficiary.Id,
            UserId = beneficiary.UserId,
            Name = beneficiary.Name,
            Iban = beneficiary.Iban.Value,
            BankName = beneficiary.BankName,
            LastTransferDate = beneficiary.LastTransferDate,
            TotalAmountSent = beneficiary.TotalAmountSent,
            TransferCount = beneficiary.TransferCount
        };
    }
}
