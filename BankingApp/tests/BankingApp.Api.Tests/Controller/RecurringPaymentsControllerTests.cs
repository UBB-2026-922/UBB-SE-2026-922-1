namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.RecurringPayments;
using Application.Services.RecurringPayments;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class RecurringPaymentsControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultPaymentId = 10;

    private readonly Mock<IRecurringPaymentService> _recurringPaymentService = new(MockBehavior.Strict);

    [Fact]
    public void GetAll_WhenServiceReturnsPayments_ReturnsOkWithPayments()
    {
        // Arrange
        var payments = new List<RecurringPaymentResponse>
        {
            new() { Id = DefaultPaymentId, UserId = DefaultUserId, Amount = 50m },
        };
        _recurringPaymentService.Setup(service => service.GetByUser(DefaultUserId)).Returns(payments);
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.GetAll();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(payments);
        _recurringPaymentService.Verify(service => service.GetByUser(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetAll_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.GetByUser(DefaultUserId))
            .Returns(Error.Failure("query_failed", "Could not retrieve payments."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.GetAll();

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Create_WhenRequestIsValid_ReturnsCreatedWithPayment()
    {
        // Arrange
        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 5,
            SourceAccountId = 3,
            Amount = 100m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateTime(2026, 6, 1),
        };
        var created = new RecurringPaymentResponse
        {
            Id = DefaultPaymentId,
            UserId = DefaultUserId,
            BillerId = 5,
            SourceAccountId = 3,
            Amount = 100m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateTime(2026, 6, 1),
            Status = RecurringPaymentStatus.Active,
        };
        _recurringPaymentService.Setup(service => service.Create(DefaultUserId, request)).Returns(created);
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Create(request);

        // Assert
        CreatedAtActionResult createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(RecurringPaymentsController.GetAll));
        createdResult.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public void Create_WhenServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateRecurringPaymentRequest { Amount = -1m };
        _recurringPaymentService
            .Setup(service => service.Create(DefaultUserId, request))
            .Returns(Error.Validation("invalid_amount", "Amount must be positive."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Pause_WhenServiceSucceeds_ReturnsNoContent()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Pause(DefaultUserId, DefaultPaymentId))
            .Returns(Result.Success);
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Pause(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _recurringPaymentService.Verify(service => service.Pause(DefaultUserId, DefaultPaymentId), Times.Once);
    }

    [Fact]
    public void Pause_WhenPaymentNotFound_ReturnsNotFound()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Pause(DefaultUserId, DefaultPaymentId))
            .Returns(Error.NotFound("not_found", "Schedule not found."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Pause(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Pause_WhenUserDoesNotOwnPayment_ReturnsForbidden()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Pause(DefaultUserId, DefaultPaymentId))
            .Returns(Error.Forbidden("forbidden", "You do not own this schedule."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Pause(DefaultPaymentId);

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void Resume_WhenServiceSucceeds_ReturnsNoContent()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.ResumeRecurringPayment(DefaultUserId, DefaultPaymentId))
            .Returns(Result.Success);
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.ResumePayment(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _recurringPaymentService.Verify(
            service => service.ResumeRecurringPayment(DefaultUserId, DefaultPaymentId),
            Times.Once);
    }

    [Fact]
    public void Resume_WhenPaymentNotFound_ReturnsNotFound()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.ResumeRecurringPayment(DefaultUserId, DefaultPaymentId))
            .Returns(Error.NotFound("not_found", "Schedule not found."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.ResumePayment(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Resume_WhenUserDoesNotOwnPayment_ReturnsForbidden()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.ResumeRecurringPayment(DefaultUserId, DefaultPaymentId))
            .Returns(Error.Forbidden("forbidden", "You do not own this schedule."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.ResumePayment(DefaultPaymentId);

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void Cancel_WhenServiceSucceeds_ReturnsNoContent()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Cancel(DefaultUserId, DefaultPaymentId))
            .Returns(Result.Success);
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Cancel(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _recurringPaymentService.Verify(service => service.Cancel(DefaultUserId, DefaultPaymentId), Times.Once);
    }

    [Fact]
    public void Cancel_WhenPaymentNotFound_ReturnsNotFound()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Cancel(DefaultUserId, DefaultPaymentId))
            .Returns(Error.NotFound("not_found", "Schedule not found."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Cancel(DefaultPaymentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Cancel_WhenUserDoesNotOwnPayment_ReturnsForbidden()
    {
        // Arrange
        _recurringPaymentService
            .Setup(service => service.Cancel(DefaultUserId, DefaultPaymentId))
            .Returns(Error.Forbidden("forbidden", "You do not own this schedule."));
        RecurringPaymentsController controller = CreateController();

        // Act
        IActionResult result = controller.Cancel(DefaultPaymentId);

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private RecurringPaymentsController CreateController()
    {
        var controller = new RecurringPaymentsController(_recurringPaymentService.Object);
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = DefaultUserId,
            },
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}
