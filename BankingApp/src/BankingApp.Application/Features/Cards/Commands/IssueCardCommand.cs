namespace BankingApp.Application.Features.Cards.Commands;

using System.Globalization;
using System.Security.Cryptography;
using Contracts.Features.Cards.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;
using Currency = NodaMoney.Currency;

public sealed record IssueCardCommand(
    int UserId,
    CardType CardType,
    string? CardBrand)
    : IRequest<ErrorOr<CardDetailsDto>>;

public sealed class IssueCardCommandHandler(
    IAccountRepository accountRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<IssueCardCommandHandler> logger)
    : IRequestHandler<IssueCardCommand, ErrorOr<CardDetailsDto>>
{
    private const int CardNumberLength = 16;
    private const int CvvLength = 3;
    private const int ExpiryYears = 4;
    private const string DefaultCurrency = "USD";
    private const string IbanCountryCode = "RO";
    private const int IbanCheckDigits = 49;
    private const int IbanBankCode = 12345678;
    private const int IbanAccountNumberDigits = 10;
    private const string IbanBban = "BANK";
    private const int DecimalBase = 10;
    private const int IbanAccountNumberUpperBound = 1_000_000_000;

    public async Task<ErrorOr<CardDetailsDto>> Handle(IssueCardCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.IssueCardAccountNotFound(logger, 0, command.UserId);
            return UserErrors.NotFound;
        }

        string ibanValue = GenerateIban();
        ErrorOr<Iban> ibanResult = Iban.Create(ibanValue);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        AccountType accountType = command.CardType == CardType.Credit
            ? AccountType.Credit
            : AccountType.Checking;

        DateTime now = clock.UtcNow;
        var account = Account.Open(
            command.UserId,
            ibanResult.Value,
            Currency.FromCode(DefaultCurrency),
            accountType,
            null,
            now);

        await accountRepository.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string cardNumber = GenerateCardNumber();
        string cvv = GenerateCvv();
        DateTime expiryDate = now.AddYears(ExpiryYears);
        string cardholderName = user.FullName.ToUpperInvariant();

        Card card = account.IssueCard(
            cardNumber,
            cardholderName,
            expiryDate,
            cvv,
            command.CardType,
            command.CardBrand,
            now);

        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardIssued(logger, card.Id, account.Id, command.UserId);

        return new CardDetailsDto
        {
            Id = card.Id,
            CardNumber = card.GetMaskedNumber(),
            FullCardNumber = card.CardNumber,
            SecurityCode = card.Cvv,
            CardholderName = card.CardholderName,
            ExpiryDate = card.ExpiryDate,
            CardType = card.CardType,
            CardBrand = card.CardBrand,
            Status = card.Status,
            IsContactlessEnabled = card.IsContactlessEnabled,
            IsOnlineEnabled = card.IsOnlineEnabled,
            AccountId = account.Id,
            AccountName = account.AccountName
        };
    }

    private static string GenerateCardNumber()
    {
        Span<byte> bytes = stackalloc byte[CardNumberLength];
        RandomNumberGenerator.Fill(bytes);
        var stringBuilder = new System.Text.StringBuilder(CardNumberLength);
        foreach (byte randomByte in bytes)
        {
            stringBuilder.Append(randomByte % DecimalBase);
        }

        return stringBuilder.ToString();
    }

    private static string GenerateCvv()
    {
        Span<byte> bytes = stackalloc byte[CvvLength];
        RandomNumberGenerator.Fill(bytes);
        var stringBuilder = new System.Text.StringBuilder(CvvLength);
        foreach (byte randomByte in bytes)
        {
            stringBuilder.Append(randomByte % DecimalBase);
        }

        return stringBuilder.ToString();
    }

    private static string GenerateIban()
    {
        int accountSuffix = RandomNumberGenerator.GetInt32(IbanAccountNumberUpperBound);

        string domesticAccountIdentifier =
            $"{IbanBban}{IbanBankCode:D8}{accountSuffix.ToString(CultureInfo.InvariantCulture).PadLeft(IbanAccountNumberDigits, '0')}";

        return $"{IbanCountryCode}{IbanCheckDigits:D2}{domesticAccountIdentifier}";
    }
}