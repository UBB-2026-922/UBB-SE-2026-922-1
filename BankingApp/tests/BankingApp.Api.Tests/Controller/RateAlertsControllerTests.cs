namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.RateAlerts;
using Application.Services.RateAlerts;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class RateAlertsControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultAlertId = 42;

    private readonly Mock<IRateAlertService> _rateAlertService = new(MockBehavior.Strict);

    [Fact]
    public void GetAlerts_WhenServiceReturnsAlerts_ReturnsOkWithAlerts()
    {
        // Arrange
        var alerts = new List<RateAlertDto>
        {
            new() { Id = DefaultAlertId, UserId = DefaultUserId, BaseCurrency = "EUR", TargetCurrency = "USD", TargetRate = 1.10m },
        };
        _rateAlertService.Setup(service => service.GetAlerts(DefaultUserId)).Returns(alerts);
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.GetAlerts();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(alerts);
        _rateAlertService.Verify(service => service.GetAlerts(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetAlerts_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _rateAlertService
            .Setup(service => service.GetAlerts(DefaultUserId))
            .Returns(Error.NotFound("alerts_not_found", "No alerts found."));
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.GetAlerts();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void CreateAlert_WhenRequestIsValid_ReturnsOkWithCreatedAlert()
    {
        // Arrange
        var request = new RateAlertDto
        {
            BaseCurrency = "EUR",
            TargetCurrency = "USD",
            TargetRate = 1.15m,
            IsBuyAlert = true,
        };
        var createdAlert = new RateAlertDto
        {
            Id = DefaultAlertId,
            UserId = DefaultUserId,
            BaseCurrency = "EUR",
            TargetCurrency = "USD",
            TargetRate = 1.15m,
            IsBuyAlert = true,
        };
        _rateAlertService
            .Setup(service => service.CreateAlert(It.Is<RateAlertDto>(dto => dto.UserId == DefaultUserId)))
            .Returns(createdAlert);
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.CreateAlert(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(createdAlert);
    }

    [Fact]
    public void CreateAlert_WhenCalled_OverwritesUserIdFromAuth()
    {
        // Arrange
        var request = new RateAlertDto
        {
            UserId = 999,
            BaseCurrency = "GBP",
            TargetCurrency = "RON",
            TargetRate = 6.0m,
        };
        _rateAlertService
            .Setup(service => service.CreateAlert(It.Is<RateAlertDto>(dto => dto.UserId == DefaultUserId)))
            .Returns(new RateAlertDto { Id = DefaultAlertId, UserId = DefaultUserId });
        RateAlertsController controller = CreateController();

        // Act
        controller.CreateAlert(request);

        // Assert
        _rateAlertService.Verify(
            service => service.CreateAlert(It.Is<RateAlertDto>(dto => dto.UserId == DefaultUserId)),
            Times.Once);
    }

    [Fact]
    public void CreateAlert_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        var request = new RateAlertDto { BaseCurrency = "EUR", TargetCurrency = "USD", TargetRate = 1.15m };
        _rateAlertService
            .Setup(service => service.CreateAlert(It.IsAny<RateAlertDto>()))
            .Returns(Error.Validation("invalid_alert", "Invalid alert configuration."));
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.CreateAlert(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void DeleteAlert_WhenAlertBelongsToUser_ReturnsNoContent()
    {
        // Arrange
        var alerts = new List<RateAlertDto>
        {
            new() { Id = DefaultAlertId, UserId = DefaultUserId },
        };
        _rateAlertService.Setup(service => service.GetAlerts(DefaultUserId)).Returns(alerts);
        _rateAlertService.Setup(service => service.DeleteAlert(DefaultAlertId)).Returns(Result.Success);
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteAlert(DefaultAlertId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _rateAlertService.Verify(service => service.DeleteAlert(DefaultAlertId), Times.Once);
    }

    [Fact]
    public void DeleteAlert_WhenAlertDoesNotBelongToUser_ReturnsNotFound()
    {
        // Arrange
        const int otherAlertId = 99;
        var alerts = new List<RateAlertDto>
        {
            new() { Id = DefaultAlertId, UserId = DefaultUserId },
        };
        _rateAlertService.Setup(service => service.GetAlerts(DefaultUserId)).Returns(alerts);
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteAlert(otherAlertId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void DeleteAlert_WhenGetAlertsFails_ReturnsMatchingError()
    {
        // Arrange
        _rateAlertService
            .Setup(service => service.GetAlerts(DefaultUserId))
            .Returns(Error.Unexpected("service_error", "Something went wrong."));
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteAlert(DefaultAlertId);

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void DeleteAlert_WhenDeleteServiceFails_ReturnsMatchingError()
    {
        // Arrange
        var alerts = new List<RateAlertDto>
        {
            new() { Id = DefaultAlertId, UserId = DefaultUserId },
        };
        _rateAlertService.Setup(service => service.GetAlerts(DefaultUserId)).Returns(alerts);
        _rateAlertService
            .Setup(service => service.DeleteAlert(DefaultAlertId))
            .Returns(Error.NotFound("alert_not_found", "Alert not found."));
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteAlert(DefaultAlertId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void DeleteAlert_WhenUserHasNoAlerts_ReturnsNotFound()
    {
        // Arrange
        _rateAlertService
            .Setup(service => service.GetAlerts(DefaultUserId))
            .Returns(new List<RateAlertDto>());
        RateAlertsController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteAlert(DefaultAlertId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private RateAlertsController CreateController()
    {
        var controller = new RateAlertsController(_rateAlertService.Object);
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
