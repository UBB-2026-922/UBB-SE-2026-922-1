// <copyright file="DashboardServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Dashboard;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using ApplicationCardType = BankingApp.Application.Enums.CardType;

namespace BankingApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="DashboardService" />.
/// </summary>
public class DashboardServiceTests
{
    private const int NonExistentUserId = 99;
    private const int FirstAccountTransactionCount = 8;
    private const int SecondAccountTransactionCount = 5;
    private const int AmountMultiplier = 10;
    private const int MergedTransactionLimit = 5;

    private readonly Mock<IDashboardRepository> _dashboardRepository = new(MockBehavior.Strict);

    private readonly DashboardService _service;
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardServiceTests" /> class.
    /// </summary>
    public DashboardServiceTests()
    {
        _userRepository
            .Setup(findsById => findsById.FindById(It.IsAny<int>()))
            .Returns(Error.NotFound());
        _dashboardRepository
            .Setup(getsCardsByUser => getsCardsByUser.GetCardsByUser(It.IsAny<int>()))
            .Returns(new List<Card>());
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(It.IsAny<int>()))
            .Returns(new List<Account>());
        _dashboardRepository
            .Setup(getsRecentTransactions =>
                getsRecentTransactions.GetRecentTransactions(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<Transaction>());
        _dashboardRepository
            .Setup(getsUnreadNotificationCount =>
                getsUnreadNotificationCount.GetUnreadNotificationCount(It.IsAny<int>()))
            .Returns(0);

        _service = new DashboardService(
            _dashboardRepository.Object,
            _userRepository.Object,
            NullLogger<DashboardService>.Instance);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(NonExistentUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenUserExists_ReturnsResponseWithUserSummary scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenUserExists_ReturnsResponseWithUserSummary()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = fullName, Email = email });

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CurrentUser!.FullName.Should().Be(fullName);
        result.Value.CurrentUser!.Email.Should().Be(email);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenCardsExist_ReturnsMappedCards scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenCardsExist_ReturnsMappedCards()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsCardsByUser => getsCardsByUser.GetCardsByUser(userId))
            .Returns(
                new List<Card>
                {
                    new()
                    {
                        Id = 1,
                        UserId = userId,
                        CardNumber = "1234567890123456",
                        CardholderName = "Ada Lovelace",
                        CardType = CardType.Debit,
                        ExpiryDate = new DateTime(2027, 12, 1),
                        Status = CardStatus.Active,
                    },
                });
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(userId))
            .Returns(
                new List<Account>
                {
                    new() { Id = 0, AccountName = "Checking", Balance = 2500 },
                });

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Cards.Should().ContainSingle();
        result.Value.Cards.First().CardholderName.Should().Be("Ada Lovelace");
        result.Value.Cards.First().CardType.Should().Be(ApplicationCardType.Debit);
        result.Value.Cards.First().CardNumber.Should().Be("**** **** **** 3456");
        result.Value.Cards.First().AccountName.Should().Be("Checking");
        result.Value.Cards.First().AccountBalance.Should().Be(2500);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenCardsQueryFails_ReturnsEmptyCardList scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenCardsQueryFails_ReturnsEmptyCardList()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsCardsByUser => getsCardsByUser.GetCardsByUser(userId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Cards.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenTransactionsExist_ReturnsMappedTransactions scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenTransactionsExist_ReturnsMappedTransactions()
    {
        // Arrange
        const int userId = 1;
        const int accountId = 10;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(userId))
            .Returns(
                new List<Account>
                {
                    new() { Id = accountId, UserId = userId },
                });
        _dashboardRepository
            .Setup(getsRecentTransactions => getsRecentTransactions.GetRecentTransactions(accountId, It.IsAny<int>()))
            .Returns(
                new List<Transaction>
                {
                    new()
                    {
                        Id = 1,
                        AccountId = accountId,
                        Direction = TransactionDirection.Out,
                        Amount = 100,
                        Currency = "RON",
                        Status = TransactionStatus.Completed,
                        MerchantName = "Shop",
                        CreatedAt = DateTime.UtcNow,
                    },
                });

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().ContainSingle();
        result.Value.RecentTransactions.First().MerchantName.Should().Be("Shop");
        result.Value.RecentTransactions.First().Amount.Should().Be(100);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenAccountsQueryFails_ReturnsEmptyTransactionList scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenAccountsQueryFails_ReturnsEmptyTransactionList()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(userId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenNotificationCountQueryFails_ReturnsZeroCount scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenNotificationCountQueryFails_ReturnsZeroCount()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsUnreadNotificationCount => getsUnreadNotificationCount.GetUnreadNotificationCount(userId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UnreadNotificationCount.Should().Be(0);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenMultipleAccountsExist_MergesAndLimitsTransactions scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenMultipleAccountsExist_MergesAndLimitsTransactions()
    {
        // Arrange
        const int userId = 1;
        const int accountId1 = 10;
        const int accountId2 = 11;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(userId))
            .Returns(
                new List<Account>
                {
                    new() { Id = accountId1, UserId = userId },
                    new() { Id = accountId2, UserId = userId },
                });

        List<Transaction> transactions1 = Enumerable.Range(1, FirstAccountTransactionCount).Select(index =>
            new Transaction
            {
                Id = index,
                AccountId = accountId1,
                Direction = TransactionDirection.In,
                Amount = index * AmountMultiplier,
                Currency = "RON",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
            }).ToList();
        List<Transaction> transactions2 = Enumerable.Range(9, SecondAccountTransactionCount).Select(index =>
            new Transaction
            {
                Id = index,
                AccountId = accountId2,
                Direction = TransactionDirection.Out,
                Amount = index * AmountMultiplier,
                Currency = "RON",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
            }).ToList();
        _dashboardRepository
            .Setup(getsRecentTransactions => getsRecentTransactions.GetRecentTransactions(accountId1, It.IsAny<int>()))
            .Returns(transactions1);
        _dashboardRepository
            .Setup(getsRecentTransactions => getsRecentTransactions.GetRecentTransactions(accountId2, It.IsAny<int>()))
            .Returns(transactions2);

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().HaveCount(MergedTransactionLimit);
    }

    /// <summary>
    ///     Verifies the GetDashboardData_WhenUnreadNotificationsExist_ReturnsCorrectCount scenario.
    /// </summary>
    [Fact]
    public void GetDashboardData_WhenUnreadNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        const int userId = 1;
        const int unreadCount = 7;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        _dashboardRepository
            .Setup(getsUnreadNotificationCount => getsUnreadNotificationCount.GetUnreadNotificationCount(userId))
            .Returns(unreadCount);

        // Act
        ErrorOr<DashboardResponse> result = _service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UnreadNotificationCount.Should().Be(unreadCount);
    }
}
