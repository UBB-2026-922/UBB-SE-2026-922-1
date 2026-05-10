namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class RecurringPaymentsControllerTests
{
    private const int DefaultPaymentId = 10;

    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetById_WhenFound_ReturnsOkWithPayment()
    {
        var payment = new RecurringPayment { Id = DefaultPaymentId };
        _recurringPaymentRepository.Setup(r => r.GetById(DefaultPaymentId)).Returns(payment);
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.GetById(DefaultPaymentId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(payment);
    }

    [Fact]
    public void GetById_WhenNotFound_ReturnsNotFound()
    {
        _recurringPaymentRepository.Setup(r => r.GetById(99)).Returns(Error.NotFound());
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.GetById(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetByUserId_WhenPaymentsExist_ReturnsOkWithList()
    {
        var payments = new List<RecurringPayment> { new() { Id = 1 }, new() { Id = 2 } };
        _recurringPaymentRepository.Setup(r => r.GetByUserId(1)).Returns(payments);
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.GetByUserId(1);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(payments);
    }

    [Fact]
    public void GetByUserId_WhenRepositoryFails_ReturnsError()
    {
        _recurringPaymentRepository.Setup(r => r.GetByUserId(1)).Returns(Error.Failure());
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.GetByUserId(1);

        ObjectResult obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void GetDuePayments_WhenDuePaymentsExist_ReturnsOkWithList()
    {
        DateTime asOf = new(2026, 6, 1);
        var duePayments = new List<RecurringPayment> { new() { Id = 1 } };
        _recurringPaymentRepository.Setup(r => r.GetDuePayments(asOf)).Returns(duePayments);
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.GetDuePayments(asOf);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(duePayments);
    }

    [Fact]
    public void Create_WhenSuccessful_ReturnsOkWithPayment()
    {
        var payment = new RecurringPayment { Id = DefaultPaymentId, Amount = 100m, Frequency = RecurringFrequency.Monthly };
        _recurringPaymentRepository.Setup(r => r.Create(payment)).Returns(payment);
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.Create(payment);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(payment);
    }

    [Fact]
    public void Create_WhenRepositoryFails_ReturnsError()
    {
        var payment = new RecurringPayment { Amount = -1m };
        _recurringPaymentRepository.Setup(r => r.Create(payment)).Returns(Error.Validation());
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.Create(payment);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Update_WhenSuccessful_ReturnsNoContent()
    {
        var payment = new RecurringPayment { Id = DefaultPaymentId, Amount = 200m };
        _recurringPaymentRepository.Setup(r => r.Update(payment)).Returns(Result.Success);
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.Update(payment);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Update_WhenNotFound_ReturnsNotFound()
    {
        var payment = new RecurringPayment { Id = 99 };
        _recurringPaymentRepository.Setup(r => r.Update(payment)).Returns(Error.NotFound());
        RecurringPaymentsController controller = CreateController();

        IActionResult result = controller.Update(payment);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private RecurringPaymentsController CreateController()
    {
        RecurringPaymentsController controller = new(_recurringPaymentRepository.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }
}
