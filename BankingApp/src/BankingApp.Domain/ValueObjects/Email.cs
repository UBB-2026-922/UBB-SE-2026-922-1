namespace BankingApp.Domain.ValueObjects;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Errors;
using ErrorOr;

public sealed record Email : ValueObject
{
    private Email()
    {
    }

    public string Value { get; private init; } = string.Empty;

    public static ErrorOr<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return UserErrors.InvalidEmail;
        }

        int atIndex = value.IndexOf('@');
        if (atIndex <= 0 || atIndex == value.Length - 1 || value.IndexOf('.', atIndex) < 0)
        {
            return UserErrors.InvalidEmail;
        }

        return new Email { Value = value };
    }

    public override string ToString() => Value;
}
