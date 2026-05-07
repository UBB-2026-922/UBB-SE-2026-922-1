namespace BankingApp.Api.Tests.Controller;

using Controllers;
using BankingApp.Application.Features.AccountOverview.Services;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Features.AccountOverview.Dtos;

/// <summary>
///     Unit tests for <see cref="DashboardController" /> verifying route contracts
///     and protected endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class DashboardControllerTests
{
    private readonly Mock<IAccountOverviewService> _dashboardService = MockFactory.CreateAccountOverviewService();

    /// <summary>
    ///     Verifies the GetDashboard_WhenSuccess_ReturnsOkWithData scenario.
    /// </summary>
    [Fact]
    public void GetDashboard_WhenSuccess_ReturnsOkWithData()
    {
        // Arrange
        const int validUserId = 1;
        var response = new AccountOverviewDto();
        _dashboardService.Setup(getsDashboardData => getsDashboardData.GetAccountOverview(validUserId)).Returns(response);
        DashboardController controller = CreateController(validUserId);

        // Act
        IActionResult result = controller.GetDashboard();

        // Assert
        OkObjectResult? successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().Be(response);
    }

    /// <summary>
    ///     Verifies the GetDashboard_WhenUserNotFound_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void GetDashboard_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        const int nonExistentUserId = 99;
        _dashboardService
            .Setup(getsDashboardData => getsDashboardData.GetAccountOverview(nonExistentUserId))
            .Returns(Error.NotFound("user_not_found", "User not found."));

        DashboardController controller = CreateController(nonExistentUserId);

        // Act
        IActionResult result = controller.GetDashboard();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private DashboardController CreateController(int authenticatedUserId)
    {
        var controller = new DashboardController(_dashboardService.Object);
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = authenticatedUserId
            }
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}