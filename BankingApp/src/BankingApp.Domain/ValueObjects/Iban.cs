namespace BankingApp.Domain.ValueObjects;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Errors;
using BankingApp.Domain.Services;
using ErrorOr;

public sealed record Iban : ValueObject
{
    private Iban()
    {
    }

    public string Value { get; private init; } = string.Empty;

    public static ErrorOr<Iban> Create(string value)
    {
        if (!IbanValidationService.IsValid(value))
        {
            return TransferErrors.InvalidIban;
        }

        return new Iban { Value = value };
    }

    public override string ToString() => Value;
}
