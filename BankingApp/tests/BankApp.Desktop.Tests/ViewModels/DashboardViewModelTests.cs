// <copyright file="DashboardViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Globalization;
using BankApp.Application.DataTransferObjects.Dashboard;
using BankApp.Application.Enums;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for the <see cref="DashboardViewModel" />.
/// </summary>
public class DashboardViewModelTests
{
    private const int CardNumberVisibleSuffixLength = 4;

    private readonly Mock<IApiClient> _apiClient;
    private readonly DashboardViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardViewModelTests" /> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public DashboardViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Strict);
        _viewModel = new DashboardViewModel(_apiClient.Object, NullLogger<DashboardViewModel>.Instance);
    }

    /// <summary>
    ///     In LoadDashboard, when the response is valid the view model should be populated.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenResponseIsValid_PopulatesViewModel()
    {
        // Arrange
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        const string cardBrand = "Visa";
        const CardType cardType = CardType.Debit;
        const string cardNumber = "1234567812345678";
        var cardExpiry = new DateTime(2027, 12, 1);
        const string merchantName = "Coffee Shop";
        const string currency = "USD";
        const decimal transactionAmount = 12.5m;
        const int unreadCount = 4;

        var response = new DashboardResponse
        {
            CurrentUser = new UserSummaryDataTransferObject
            {
                FullName = fullName,
                Email = email,
            },
            Cards =
            [
                new CardDataTransferObject
                {
                    CardBrand = cardBrand,
                    CardType = cardType,
                    CardholderName = fullName,
                    CardNumber = cardNumber,
                    ExpiryDate = cardExpiry,
                    Status = CardStatus.Active,
                    IsContactlessEnabled = true,
                    IsOnlineEnabled = true,
                },
            ],
            RecentTransactions =
            [
                new TransactionDataTransferObject
                {
                    MerchantName = merchantName,
                    Direction = TransactionDirection.Out,
                    Amount = transactionAmount,
                    Currency = currency,
                },
            ],
            UnreadNotificationCount = unreadCount,
        };
        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard();

        // Assert — result and state
        result.IsError.Should().BeFalse();
        _viewModel.State.Value.Should().Be(DashboardState.Success);
        _viewModel.ErrorMessage.Should().BeEmpty();

        // Assert — current user
        _viewModel.CurrentUser.Should().BeEquivalentTo(
            new UserSummaryDataTransferObject
            {
                FullName = fullName,
                Email = email,
            });

        // Assert — selected card display properties
        _viewModel.CardDots.Should().ContainSingle();
        _viewModel.SelectedCardBrandDisplay.Should().Be(cardBrand);
        _viewModel.SelectedCardHolderDisplay.Should().Be(fullName.ToUpperInvariant());
        _viewModel.SelectedCardNumberMasked.Should()
            .Be($"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}");
        _viewModel.SelectedCardExpiryDisplay.Should().Be(cardExpiry.ToString("MM/yy"));

        // Assert — transaction item
        var expectedAmountDisplay = $"-{transactionAmount.ToString("N2", CultureInfo.InvariantCulture)}";
        _viewModel.RecentTransactionItems.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new DashboardTransactionItem
                {
                    MerchantDisplayName = merchantName,
                    AmountDisplay = expectedAmountDisplay,
                    Currency = currency,
                });

        // Assert — notification count
        _viewModel.UnreadNotificationCount.Should().Be(unreadCount);
    }

    /// <summary>
    ///     In LoadDashboard(), when the current user is missing the error state should be set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardResponse());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Value.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.IncompleteResponse);
    }

    /// <summary>
    ///     In LoadDashboard, when the request is unauthorized the session expired message should be set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Value.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.SessionExpired);
    }

    /// <summary>
    ///     In LoadDashboard, when the request returns not found the not found message should be set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenNotFound_SetsNotFoundMessage()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Value.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.NotFound);
    }

    /// <summary>
    ///     In LoadDashboard, when the API returns a general failure the load failed message should be set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenApiFailureOccurs_SetsLoadFailedMessage()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Value.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.LoadFailed);
    }

    /// <summary>
    ///     In NavigatePrevious, when no cards are loaded an error should be returned.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigatePrevious_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     In NavigatePrevious, when already at the first card an error should be returned
    ///     and the card index should remain at zero.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigatePrevious_WhenAtFirstCard_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    /// <summary>
    ///     In NavigatePrevious, when not at the first card the operation should succeed
    ///     and the card index should decrement by one.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigatePrevious_WhenNotAtFirstCard_SucceedsAndDecrementsIndex()
    {
        // Arrange
        await LoadViewModelWithCards(2);
        _viewModel.NavigateNext();

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeFalse();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    /// <summary>
    ///     In NavigateNext, when no cards are loaded an error should be returned.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigateNext_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     In NavigateNext, when already at the last card an error should be returned
    ///     and the card index should remain unchanged.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigateNext_WhenAtLastCard_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(1);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    /// <summary>
    ///     In NavigateNext, when not at the last card the operation should succeed
    ///     and the card index should increment by one.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task NavigateNext_WhenNotAtLastCard_SucceedsAndIncrementsIndex()
    {
        // Arrange
        await LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeFalse();
        _viewModel.CurrentCardIndex.Should().Be(1);
    }

    /// <summary>
    ///     When a card brand is set, <see cref="DashboardViewModel.SelectedCardBrandDisplay" />
    ///     should return the brand name.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsPresent_ReturnsBrand()
    {
        // Arrange
        const string cardBrand = "Visa";
        await LoadViewModelWithCards(1, cardBrand);

        // Act
        string display = _viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(cardBrand);
    }

    /// <summary>
    ///     When the card brand is empty, <see cref="DashboardViewModel.SelectedCardBrandDisplay" />
    ///     should fall back to the card type string.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsAbsent_FallsBackToCardType()
    {
        // Arrange
        const CardType cardType = CardType.Credit;
        await LoadViewModelWithCards(1, string.Empty, cardType);

        // Act
        string display = _viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(cardType.ToString());
    }

    /// <summary>
    ///     When a cardholder name is set, <see cref="DashboardViewModel.SelectedCardHolderDisplay" />
    ///     should return the name in upper case.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsPresent_ReturnsUpperCasedName()
    {
        // Arrange
        const string cardholderName = "Ada Lovelace";
        await LoadViewModelWithCards(1, cardholderName: cardholderName);

        // Act
        string display = _viewModel.SelectedCardHolderDisplay;

        // Assert
        display.Should().Be(cardholderName.ToUpperInvariant());
    }

    /// <summary>
    ///     When the cardholder name is empty, <see cref="DashboardViewModel.SelectedCardHolderDisplay" />
    ///     should return a placeholder string.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsAbsent_ReturnsPlaceholder()
    {
        // Arrange
        const string expectedSelectedCardHolderDisplay = "CARD HOLDER";
        await LoadViewModelWithCards(1, cardholderName: string.Empty);

        // Act
        string display = _viewModel.SelectedCardHolderDisplay;

        // Assert
        display.Should().Be(expectedSelectedCardHolderDisplay);
    }

    /// <summary>
    ///     When a valid card number is set, <see cref="DashboardViewModel.SelectedCardNumberMasked" />
    ///     should expose only the last four digits and mask the rest.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsValid_ShowsOnlyLastFourDigits()
    {
        // Arrange
        const string cardNumber = "1234567890123456";
        var expectedMaskedCardNumber = $"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}";
        await LoadViewModelWithCards(1, cardNumber: cardNumber);

        // Act
        string masked = _viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    /// <summary>
    ///     When the card number is too short to extract four digits,
    ///     <see cref="DashboardViewModel.SelectedCardNumberMasked" /> should return a fully masked string.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsTooShort_ReturnsFullyMasked()
    {
        // Arrange
        const string expectedMaskedCardNumber = "**** **** **** ****";
        await LoadViewModelWithCards(1, cardNumber: "123");

        // Act
        string masked = _viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    /// <summary>
    ///     When no cards are loaded, <see cref="DashboardViewModel.GetSelectedCardDetails" />
    ///     should return an empty string.
    /// </summary>
    [Fact]
    public void GetSelectedCardDetails_WhenNoCardIsSelected_ReturnsEmptyString()
    {
        // Arrange — viewModel starts with no cards loaded

        // Act
        string details = _viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().BeEmpty();
    }

    /// <summary>
    ///     When a card is selected, <see cref="DashboardViewModel.GetSelectedCardDetails" /> should return
    ///     a string containing the card type, brand, masked number, and cardholder name.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetSelectedCardDetails_WhenCardIsSelected_ReturnsFormattedDetails()
    {
        // Arrange
        const CardType cardType = CardType.Debit;
        const string cardBrand = "Visa";
        const string cardNumber = "1234567890123456";
        const string cardholderName = "Ada Lovelace";
        var expectedMaskedCardNumber = $"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}";
        await LoadViewModelWithCards(
            1,
            cardType: cardType,
            cardBrand: cardBrand,
            cardNumber: cardNumber,
            cardholderName: cardholderName);

        // Act
        string details = _viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().Contain(cardType.ToString())
            .And.Contain(cardBrand)
            .And.Contain(expectedMaskedCardNumber)
            .And.Contain(cardholderName);
    }

    /// <summary>
    ///     When the second card is selected via navigation, only the second card dot should be active.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CardDots_WhenNavigatedToSecondCard_SecondDotIsActive()
    {
        // Arrange
        await LoadViewModelWithCards(3);
        _viewModel.NavigateNext();

        // Act
        IReadOnlyList<CardPageIndicatorViewModel> dots = _viewModel.CardDots;

        // Assert
        dots.Select(dot => dot.IsActive).Should().Equal(false, true, false);
    }

    private async Task LoadViewModelWithCards(
        int cardCount,
        string cardBrand = "Visa",
        CardType cardType = CardType.Debit,
        string cardholderName = "Test User",
        string cardNumber = "1234567812345678")
    {
        List<CardDataTransferObject> cards = Enumerable.Range(0, cardCount)
            .Select(index => new CardDataTransferObject
            {
                CardBrand = cardBrand,
                CardType = cardType,
                CardholderName = cardholderName,
                CardNumber = cardNumber,
            })
            .ToList();

        var response = new DashboardResponse
        {
            CurrentUser = new UserSummaryDataTransferObject { FullName = "Test User" },
            Cards = cards,
        };

        _apiClient
            .Setup(getsAsync =>
                getsAsync.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _viewModel.LoadDashboard();
    }
}
