namespace BankingApp.Application.Features.Transfers.Queries;

using Contracts.Features.Transfers.Dtos;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

public sealed record ValidateIbanQuery(string Iban)
    : IRequest<ErrorOr<TransferIbanValidationResponse>>;

public sealed class
    ValidateIbanQueryHandler : IRequestHandler<ValidateIbanQuery, ErrorOr<TransferIbanValidationResponse>>
{
    public Task<ErrorOr<TransferIbanValidationResponse>> Handle(ValidateIbanQuery query,
        CancellationToken cancellationToken)
    {
        ErrorOr<Iban> result = Iban.Create(query.Iban);
        return Task.FromResult<ErrorOr<TransferIbanValidationResponse>>(new TransferIbanValidationResponse
        {
            IsValid = !result.IsError,
            BankName = result.IsError ? string.Empty : InferBankName(query.Iban)
        });
    }

    private static string InferBankName(string iban)
    {
        if (iban.Length < 2)
        {
            return string.Empty;
        }

        return iban[..2].ToUpperInvariant() switch
        {
            "RO" => "Romanian Bank",
            "DE" => "German Bank",
            "GB" => "UK Bank",
            "FR" => "French Bank",
            _ => string.Empty
        };
    }
}