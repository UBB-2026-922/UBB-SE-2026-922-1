namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public sealed class BillPaymentsControllerTests
{
    private readonly Mock<IBillPaymentRepository> _billPaymentRepository = new(MockBehavior.Strict);

    [Fact]
    public async Task GetBillersAsync_WhenCalled_ReturnsOkWithList()
    {
        var billers = new List<Biller> { new() { Id = 1, Name = "Biller1" } };
        _billPaymentRepository.Setup(r => r.GetBillersAsync()).ReturnsAsync(billers);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetBillersAsync();

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(billers);
    }

    [Fact]
    public async Task GetBillerByIdAsync_WhenFound_ReturnsOkWithBiller()
    {
        var biller = new Biller { Id = 2, Name = "Biller2" };
        _billPaymentRepository.Setup(r => r.GetBillerByIdAsync(2)).ReturnsAsync(biller);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetBillerByIdAsync(2);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(biller);
    }

    [Fact]
    public async Task GetBillerByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        _billPaymentRepository.Setup(r => r.GetBillerByIdAsync(99)).ReturnsAsync((Biller?)null);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetBillerByIdAsync(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AddPaymentAsync_WhenCalled_ReturnsOkWithPayment()
    {
        var payment = new BillPayment { Id = 5, Amount = 100m };
        _billPaymentRepository.Setup(r => r.AddPaymentAsync(payment)).Returns(Task.CompletedTask);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.AddPaymentAsync(payment);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(payment);
    }

    [Fact]
    public async Task GetUserPaymentHistoryAsync_WhenCalled_ReturnsOkWithList()
    {
        var payments = new List<BillPayment> { new() { Id = 1 }, new() { Id = 2 } };
        _billPaymentRepository.Setup(r => r.GetUserPaymentHistoryAsync(1)).ReturnsAsync(payments);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetUserPaymentHistoryAsync(1);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(payments);
    }

    [Fact]
    public async Task GetSavedBillersAsync_WhenCalled_ReturnsOkWithList()
    {
        var saved = new List<SavedBiller> { new() { Id = 1 } };
        _billPaymentRepository.Setup(r => r.GetSavedBillersAsync(1)).ReturnsAsync(saved);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetSavedBillersAsync(1);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(saved);
    }

    [Fact]
    public async Task GetAccountByIdAsync_WhenFound_ReturnsOkWithAccount()
    {
        var account = new Account { Id = 3 };
        _billPaymentRepository.Setup(r => r.GetAccountByIdAsync(3)).ReturnsAsync(account);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetAccountByIdAsync(3);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(account);
    }

    [Fact]
    public async Task GetAccountByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        _billPaymentRepository.Setup(r => r.GetAccountByIdAsync(99)).ReturnsAsync((Account?)null);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.GetAccountByIdAsync(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenCalled_ReturnsNoContent()
    {
        var account = new Account { Id = 1 };
        _billPaymentRepository.Setup(r => r.UpdateAccountAsync(account)).Returns(Task.CompletedTask);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.UpdateAccountAsync(account);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddTransactionAsync_WhenCalled_ReturnsOkWithTransaction()
    {
        var transaction = new Transaction { Id = 7, Amount = 50m };
        _billPaymentRepository.Setup(r => r.AddTransactionAsync(transaction)).Returns(Task.CompletedTask);
        BillPaymentsController controller = CreateController();

        IActionResult result = await controller.AddTransactionAsync(transaction);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(transaction);
    }

    private BillPaymentsController CreateController()
    {
        BillPaymentsController controller = new(_billPaymentRepository.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }
}
