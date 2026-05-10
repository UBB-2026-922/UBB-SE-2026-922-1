namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public sealed class ExchangeControllerTests
{
    private readonly Mock<IExchangeRepository> _exchangeRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetById_WhenFound_ReturnsOkWithExchange()
    {
        var exchange = new ExchangeTransaction { Id = 1 };
        _exchangeRepository.Setup(r => r.GetById(1)).Returns(exchange);
        ExchangeController controller = CreateController();

        IActionResult result = controller.GetById(1);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(exchange);
    }

    [Fact]
    public void GetById_WhenNotFound_ReturnsNotFound()
    {
        _exchangeRepository.Setup(r => r.GetById(99)).Returns(Error.NotFound());
        ExchangeController controller = CreateController();

        IActionResult result = controller.GetById(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetByUserId_WhenExchangesExist_ReturnsOkWithList()
    {
        var exchanges = new List<ExchangeTransaction> { new() { Id = 1 }, new() { Id = 2 } };
        _exchangeRepository.Setup(r => r.GetByUserId(1)).Returns(exchanges);
        ExchangeController controller = CreateController();

        IActionResult result = controller.GetByUserId(1);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(exchanges);
    }

    [Fact]
    public void GetByUserId_WhenRepositoryFails_ReturnsError()
    {
        _exchangeRepository.Setup(r => r.GetByUserId(1)).Returns(Error.Failure());
        ExchangeController controller = CreateController();

        IActionResult result = controller.GetByUserId(1);

        ObjectResult obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Create_WhenSuccessful_ReturnsOkWithExchange()
    {
        var exchange = new ExchangeTransaction { Id = 5, SourceCurrency = "EUR", TargetCurrency = "RON" };
        _exchangeRepository.Setup(r => r.Create(exchange)).Returns(exchange);
        ExchangeController controller = CreateController();

        IActionResult result = controller.Create(exchange);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(exchange);
    }

    [Fact]
    public void Create_WhenRepositoryFails_ReturnsError()
    {
        var exchange = new ExchangeTransaction { SourceCurrency = "EUR", TargetCurrency = "RON" };
        _exchangeRepository.Setup(r => r.Create(exchange)).Returns(Error.Failure());
        ExchangeController controller = CreateController();

        IActionResult result = controller.Create(exchange);

        ObjectResult obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void UpdateStatus_WhenSuccessful_ReturnsOkWithUpdatedExchange()
    {
        var updated = new ExchangeTransaction { Id = 1, Status = ExchangeTransactionStatus.Completed };
        _exchangeRepository.Setup(r => r.UpdateStatus(1, ExchangeTransactionStatus.Completed)).Returns(updated);
        ExchangeController controller = CreateController();

        IActionResult result = controller.UpdateStatus(1, new ExchangeController.UpdateStatusRequest { Status = ExchangeTransactionStatus.Completed });

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public void UpdateStatus_WhenNotFound_ReturnsNotFound()
    {
        _exchangeRepository.Setup(r => r.UpdateStatus(99, ExchangeTransactionStatus.Completed)).Returns(Error.NotFound());
        ExchangeController controller = CreateController();

        IActionResult result = controller.UpdateStatus(99, new ExchangeController.UpdateStatusRequest { Status = ExchangeTransactionStatus.Completed });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private ExchangeController CreateController()
    {
        ExchangeController controller = new(_exchangeRepository.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }
}
