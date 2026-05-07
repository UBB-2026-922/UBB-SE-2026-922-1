using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Dashboard;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class DashboardViewModel
{
    private const string CardAtStartErrorCode = "dashboard.card_at_start";
    private const string CardAtStartErrorDescription = "Already at the first card.";
    private const string CardAtEndErrorCode = "dashboard.card_at_end";
    private const string CardAtEndErrorDescription = "Already at the last card.";
    private const int FirstCardIndex = 0;
    private const int LastCardIndexOffset = 1;
    private const int CardNumberVisibleSuffixLength = 4;
    private const string FullyMaskedCardNumber = "**** **** **** ****";
    private const string CardNumberMaskPrefix = "**** **** ****";
    private readonly IDashboardClientService _dashboardClientService;
    private readonly ILogger<DashboardViewModel> _logger;
    private int _currentCardIndex;

    public DashboardViewModel(IDashboardClientService dashboardClientService, ILogger<DashboardViewModel> logger)
    {
        _dashboardClientService = dashboardClientService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CurrentUser = null;
        State = new ObservableState<DashboardState>(DashboardState.Idle);
        Cards = new List<CardDto>();
        RecentTransactions = new List<TransactionDto>();
        RecentTransactionItems = new List<DashboardTransactionItem>();
        UnreadNotificationCount = 0;
        ErrorMessage = string.Empty;
        _currentCardIndex = FirstCardIndex;
    }

    public ObservableState<DashboardState> State { get; }

    public UserSummaryDto? CurrentUser { get; private set; }

    public List<DashboardTransactionItem> RecentTransactionItems { get; private set; }

    public int UnreadNotificationCount { get; private set; }

    public string ErrorMessage { get; private set; }

    public int CurrentCardIndex
    {
        get => _currentCardIndex;
        private set => _currentCardIndex = Math.Clamp(
            value,
            FirstCardIndex,
            Math.Max(FirstCardIndex, Cards.Count - LastCardIndexOffset));
    }

    public bool CanNavigatePrevious => Cards.Count > 0 && CurrentCardIndex > FirstCardIndex;

    public bool CanNavigateNext => Cards.Count > 0 && CurrentCardIndex < Cards.Count - LastCardIndexOffset;

    public bool HasCards => Cards.Count > 0;

    public IReadOnlyList<CardPageIndicatorViewModel> CardDots
    {
        get
        {
            return Cards.Select((_, index) => new CardPageIndicatorViewModel { IsActive = index == CurrentCardIndex })
                .ToList();
        }
    }

    public bool HasTransactions => RecentTransactionItems.Count > 0;

    public string SelectedCardBrandDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardBrand) ? card.CardType.ToString() : card.CardBrand
            : string.Empty;

    public string SelectedCardHolderDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardholderName)
                ? "CARD HOLDER"
                : card.CardholderName.ToUpperInvariant()
            : string.Empty;

    public string SelectedCardExpiryDisplay =>
        SelectedCard?.ExpiryDate.ToString("MM/yy", CultureInfo.InvariantCulture) ?? string.Empty;

    public string SelectedCardNumberMasked =>
        SelectedCard is { } card ? MaskCardNumber(card.CardNumber) : FullyMaskedCardNumber;

    private List<CardDto> Cards { get; set; }

    private CardDto? SelectedCard => Cards.Count > 0 ? Cards.ElementAt(CurrentCardIndex) : null;

    private List<TransactionDto> RecentTransactions { get; set; }

    public ErrorOr<Success> NavigatePrevious()
    {
        if (!CanNavigatePrevious) return Error.Failure(CardAtStartErrorCode, CardAtStartErrorDescription);

        CurrentCardIndex--;
        return Result.Success;
    }

    public ErrorOr<Success> NavigateNext()
    {
        if (!CanNavigateNext) return Error.Failure(CardAtEndErrorCode, CardAtEndErrorDescription);

        CurrentCardIndex++;
        return Result.Success;
    }

    public string GetSelectedCardDetails()
    {
        if (SelectedCard is not { } card) return string.Empty;

        return
            $"Card Type:       {card.CardType}\n" +
            $"Card Brand:      {card.CardBrand ?? "Mastercard"}\n" +
            $"Card Number:     {MaskCardNumber(card.CardNumber)}\n" +
            $"Cardholder:      {card.CardholderName}\n" +
            $"Expiry Date:     {card.ExpiryDate:MM/yy}\n" +
            $"Status:          {card.Status}\n" +
            $"Contactless:     {(card.IsContactlessEnabled ? "Enabled" : "Disabled")}\n" +
            $"Online Payments: {(card.IsOnlineEnabled ? "Enabled" : "Disabled")}";
    }

    public async Task<ErrorOr<Success>> LoadDashboard(CancellationToken cancellationToken = default)
    {
        State.SetValue(DashboardState.Loading);
        ErrorMessage = string.Empty;
        ErrorOr<DashboardDto> result = await _dashboardClientService.GetDashboardAsync(cancellationToken);
        return result.Match<ErrorOr<Success>>(
            dashboard =>
            {
                if (dashboard.CurrentUser is null)
                {
                    ErrorMessage = UserMessages.Dashboard.IncompleteResponse;
                    State.SetValue(DashboardState.Error);
                    return Error.Validation(description: UserMessages.Dashboard.IncompleteResponse);
                }

                CurrentUser = dashboard.CurrentUser;
                Cards = dashboard.Cards;
                RecentTransactions = dashboard.RecentTransactions;
                RecentTransactionItems = BuildTransactionItems(RecentTransactions);
                UnreadNotificationCount = dashboard.UnreadNotificationCount;
                _currentCardIndex = FirstCardIndex;
                State.SetValue(DashboardState.Success);
                return Result.Success;
            },
            errors =>
            {
                ErrorMessage = errors.First().Type switch
                {
                    ErrorType.Unauthorized => UserMessages.Dashboard.SessionExpired,
                    ErrorType.NotFound => UserMessages.Dashboard.NotFound,
                    _ => UserMessages.Dashboard.LoadFailed,
                };
                _logger.LoadDashboardFailed(errors);
                State.SetValue(DashboardState.Error);
                return errors.First();
            });
    }

    private static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return FullyMaskedCardNumber;

        return cardNumber.Length >= CardNumberVisibleSuffixLength
            ? $"{CardNumberMaskPrefix} {cardNumber[^CardNumberVisibleSuffixLength..]}"
            : FullyMaskedCardNumber;
    }

    private static List<DashboardTransactionItem> BuildTransactionItems(
        IEnumerable<TransactionDto> transactions)
    {
        return transactions
            .Select(transaction => new DashboardTransactionItem
            {
                MerchantDisplayName = GetMerchantDisplayName(transaction),
                Currency = GetValueOrFallback(transaction.Currency, "N/A"),
                AmountDisplay = FormatAmountDisplay(transaction),
            })
            .ToList();
    }

    private static string GetMerchantDisplayName(TransactionDto transaction)
    {
        return FirstNonEmpty(
            transaction.MerchantName,
            transaction.Description,
            transaction.CounterpartyName,
            "Transaction");
    }

    private static string FormatAmountDisplay(TransactionDto transaction)
    {
        string sign = transaction.Direction switch
        {
            TransactionDirection.Out => "-",
            TransactionDirection.In => "+",
            _ => throw new ArgumentOutOfRangeException(
                nameof(transaction),
                transaction.Direction,
                "Unsupported transaction direction."),
        };
        return $"{sign}{transaction.Amount.ToString("N2", CultureInfo.InvariantCulture)}";
    }

    private static string GetValueOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
