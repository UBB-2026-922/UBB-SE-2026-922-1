namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class DashboardControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultAccountId = 10;

    private readonly Mock<IDashboardRepository> _dashboardRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetAccountsByUserIdRaw_WhenFound_ReturnsOkWithAccounts()
    {
        var accounts = new List<Account> { new() { Id = DefaultAccountId } };
        _dashboardRepository.Setup(repository => repository.GetAccountsByUser(DefaultUserId)).Returns(accounts);
        DashboardController controller = CreateController();

        IActionResult result = controller.GetAccountsByUserIdRaw(DefaultUserId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(accounts);
    }

    [Fact]
    public void GetAccountsByUserIdRaw_WhenRepositoryFails_ReturnsError()
    {
        _dashboardRepository.Setup(repository => repository.GetAccountsByUser(DefaultUserId)).Returns(Error.NotFound());
        DashboardController controller = CreateController();

        IActionResult result = controller.GetAccountsByUserIdRaw(DefaultUserId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetCardsByUserIdRaw_WhenFound_ReturnsOkWithCards()
    {
        var cards = new List<Card> { new() { Id = 1 } };
        _dashboardRepository.Setup(repository => repository.GetCardsByUser(DefaultUserId)).Returns(cards);
        DashboardController controller = CreateController();

        IActionResult result = controller.GetCardsByUserIdRaw(DefaultUserId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(cards);
    }

    [Fact]
    public void GetRecentTransactionsRaw_WhenFound_ReturnsOkWithTransactions()
    {
        var transactions = new List<Transaction> { new() { Id = 1, Amount = 100m } };
        _dashboardRepository.Setup(repository => repository.GetRecentTransactions(DefaultAccountId, IDashboardRepository.DefaultRecentTransactionLimit))
            .Returns(transactions);
        DashboardController controller = CreateController();

        IActionResult result = controller.GetRecentTransactionsRaw(DefaultAccountId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(transactions);
    }

    [Fact]
    public void GetUnreadNotificationCountRaw_WhenFound_ReturnsOkWithCount()
    {
        _dashboardRepository.Setup(repository => repository.GetUnreadNotificationCount(DefaultUserId)).Returns(3);
        DashboardController controller = CreateController();

        IActionResult result = controller.GetUnreadNotificationCountRaw(DefaultUserId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(3);
    }

    [Fact]
    public void DebitAccountRaw_WhenSuccessful_ReturnsNoContent()
    {
        _dashboardRepository.Setup(repository => repository.DebitAccount(DefaultAccountId, 50m)).Returns(Result.Success);
        DashboardController controller = CreateController();

        IActionResult result = controller.DebitAccountRaw(DefaultAccountId, new DashboardController.DebitAccountRequest { Amount = 50m });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void DebitAccountRaw_WhenRepositoryFails_ReturnsError()
    {
        _dashboardRepository.Setup(repository => repository.DebitAccount(DefaultAccountId, 50m)).Returns(Error.Failure());
        DashboardController controller = CreateController();

        IActionResult result = controller.DebitAccountRaw(DefaultAccountId, new DashboardController.DebitAccountRequest { Amount = 50m });

        ObjectResult obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void AddTransactionRaw_WhenSuccessful_ReturnsOkWithTransaction()
    {
        var transaction = new Transaction { Id = 5, Amount = 200m };
        _dashboardRepository.Setup(repository => repository.AddTransaction(transaction)).Returns(transaction);
        DashboardController controller = CreateController();

        IActionResult result = controller.AddTransactionRaw(transaction);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(transaction);
    }

    private DashboardController CreateController()
    {
        DashboardController controller = new(_dashboardRepository.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Items = { ["UserId"] = DefaultUserId } }
            }
        };
        return controller;
    }
}
