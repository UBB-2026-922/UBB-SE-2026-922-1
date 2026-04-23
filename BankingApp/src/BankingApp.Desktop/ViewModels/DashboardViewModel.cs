// <copyright file="DashboardViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Dashboard;
using BankingApp.Application.Enums;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Loads and exposes the data required by the dashboard view.
/// </summary>
public class DashboardViewModel
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
    private readonly IApiClient _apiClient;
    private readonly ILogger<DashboardViewModel> _logger;
    private int _currentCardIndex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for dashboard data requests.</param>
    /// <param name="logger">Logger for dashboard load errors.</param>
    /// <returns>The result of the operation.</returns>
    public DashboardViewModel(IApiClient apiClient, ILogger<DashboardViewModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CurrentUser = null;
        State = new ObservableState<DashboardState>(DashboardState.Idle);
        Cards = new List<CardDataTransferObject>();
        RecentTransactions = new List<TransactionDataTransferObject>();
        RecentTransactionItems = new List<DashboardTransactionItem>();
        UnreadNotificationCount = 0;
        ErrorMessage = string.Empty;
        _currentCardIndex = FirstCardIndex;
    }

    /// <summary>
    ///     Gets the state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<DashboardState> State { get; }

    /// <summary>
    ///     Gets the current user whose dashboard data has been loaded.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public UserSummaryDataTransferObject? CurrentUser { get; private set; }

    /// <summary>
    ///     Gets the formatted dashboard transaction rows for display.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<DashboardTransactionItem> RecentTransactionItems { get; private set; }

    /// <summary>
    ///     Gets the unread notification count.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UnreadNotificationCount { get; private set; }

    /// <summary>
    ///     Gets the latest load error message.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ErrorMessage { get; private set; }

    /// <summary>
    ///     Gets the index of the currently displayed card.
    /// </summary>
    /// <value>
    ///     The index of the currently displayed card.
    /// </value>
    public int CurrentCardIndex
    {
        get => _currentCardIndex;
        private set => _currentCardIndex = Math.Clamp(
            value,
            FirstCardIndex,
            Math.Max(FirstCardIndex, Cards.Count - LastCardIndexOffset));
    }

    /// <summary>
    ///     Gets a value indicating whether the user can navigate to the previous card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool CanNavigatePrevious => Cards.Count > 0 && CurrentCardIndex > FirstCardIndex;

    /// <summary>
    ///     Gets a value indicating whether the user can navigate to the next card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool CanNavigateNext => Cards.Count > 0 && CurrentCardIndex < Cards.Count - LastCardIndexOffset;

    /// <summary>
    ///     Gets a value indicating whether the user has any linked cards.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasCards => Cards.Count > 0;

    /// <summary>
    ///     Gets the ordered list of card-dot view models for the carousel indicator.
    ///     Each dot knows whether it represents the currently active card.
    /// </summary>
    /// <value>
    ///     The ordered list of card-dot view models for the carousel indicator.
    ///     Each dot knows whether it represents the currently active card.
    /// </value>
    public IReadOnlyList<CardPageIndicatorViewModel> CardDots
    {
        get
        {
            return Cards.Select((_, index) => new CardPageIndicatorViewModel { IsActive = index == CurrentCardIndex })
                .ToList();
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the user has any recent transactions.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasTransactions => RecentTransactionItems.Count > 0;

    /// <summary>
    ///     Gets the display name of the selected card's brand (falls back to card type when brand is absent).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string SelectedCardBrandDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardBrand) ? card.CardType.ToString() : card.CardBrand
            : string.Empty;

    /// <summary>
    ///     Gets the upper-cased cardholder name, or a placeholder when absent.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string SelectedCardHolderDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardholderName)
                ? "CARD HOLDER"
                : card.CardholderName.ToUpperInvariant()
            : string.Empty;

    /// <summary>
    ///     Gets the formatted expiry date (MM/yy) of the selected card.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string SelectedCardExpiryDisplay => SelectedCard?.ExpiryDate.ToString("MM/yy") ?? string.Empty;

    /// <summary>
    ///     Gets the masked card number of the selected card.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string SelectedCardNumberMasked =>
        SelectedCard is { } card ? MaskCardNumber(card.CardNumber) : FullyMaskedCardNumber;

    /// <summary>
    ///     Gets or sets the cards.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    private List<CardDataTransferObject> Cards { get; set; }

    /// <summary>
    ///     Gets the currently selected card, or <see langword="null" /> if no cards are available.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    private CardDataTransferObject? SelectedCard => Cards.Count > 0 ? Cards.ElementAt(CurrentCardIndex) : null;

    private List<TransactionDataTransferObject> RecentTransactions { get; set; }

    /// <summary>
    ///     Navigates to the previous card if possible.
    /// </summary>
    /// <returns>
    ///     <see cref="Result.Success" /> if navigation occurred;
    ///     otherwise an <see cref="Error" /> when already at the first card.
    /// </returns>
    public ErrorOr<Success> NavigatePrevious()
    {
        if (!CanNavigatePrevious)
        {
            return Error.Failure(CardAtStartErrorCode, CardAtStartErrorDescription);
        }

        CurrentCardIndex--;
        return Result.Success;
    }

    /// <summary>
    ///     Navigates to the next card if possible.
    /// </summary>
    /// <returns>
    ///     <see cref="Result.Success" /> if navigation occurred;
    ///     otherwise an <see cref="Error" /> when already at the last card.
    /// </returns>
    public ErrorOr<Success> NavigateNext()
    {
        if (!CanNavigateNext)
        {
            return Error.Failure(CardAtEndErrorCode, CardAtEndErrorDescription);
        }

        CurrentCardIndex++;
        return Result.Success;
    }

    /// <summary>
    ///     Builds a human-readable details string for the currently selected card.
    ///     The result is suitable for display in a dialog; it contains no UI types.
    /// </summary>
    /// <returns>A multi-line string with card details, or an empty string when no card is selected.</returns>
    public string GetSelectedCardDetails()
    {
        if (SelectedCard is not { } card)
        {
            return string.Empty;
        }

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

    /// <summary>
    ///     Fetches dashboard data for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the load operation.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> if all dashboard data loaded successfully;
    ///     otherwise an <see cref="Error" /> describing what went wrong.
    /// </returns>
    public async Task<ErrorOr<Success>> LoadDashboard(CancellationToken cancellationToken = default)
    {
        State.SetValue(DashboardState.Loading);
        ErrorMessage = string.Empty;
        ErrorOr<DashboardResponse> result = await _apiClient.GetAsync<DashboardResponse>(
            ApiEndpoints.Dashboard,
            cancellationToken);
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
                // Reset card navigation to first card after a fresh load.
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
                    _ => UserMessages.Dashboard.LoadFailed
                };
                _logger.LogError("LoadDashboard failed: {Errors}", errors);
                State.SetValue(DashboardState.Error);
                return errors.First();
            });
    }

    /// <summary>
    ///     Returns a masked representation of a card number, showing only the last four digits.
    /// </summary>
    /// <param name="cardNumber">The raw card number.</param>
    /// <returns>A masked string such as "**** **** **** 1234".</returns>
    private static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return FullyMaskedCardNumber;
        }

        return cardNumber.Length >= CardNumberVisibleSuffixLength
            ? $"{CardNumberMaskPrefix} {cardNumber[^CardNumberVisibleSuffixLength..]}"
            : FullyMaskedCardNumber;
    }

    private static List<DashboardTransactionItem> BuildTransactionItems(
        IEnumerable<TransactionDataTransferObject> transactions)
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

    private static string GetMerchantDisplayName(TransactionDataTransferObject transaction)
    {
        return FirstNonEmpty(
            transaction.MerchantName,
            transaction.Description,
            transaction.CounterpartyName,
            "Transaction");
    }

    private static string FormatAmountDisplay(TransactionDataTransferObject transaction)
    {
        string sign = transaction.Direction switch
        {
            TransactionDirection.Out => "-",
            TransactionDirection.In => "+",
            _ => throw new ArgumentOutOfRangeException(nameof(transaction.Direction), transaction.Direction, null)
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
