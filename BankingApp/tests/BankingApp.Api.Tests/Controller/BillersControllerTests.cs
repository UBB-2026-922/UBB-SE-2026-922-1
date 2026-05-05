// <copyright file="BillersControllerTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Api.Controllers;
using BankingApp.Application.DTOs.Billers;
using BankingApp.Application.Services.Billers;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Tests.Controller;

/// <summary>
///     Unit tests for <see cref="BillersController" />.
/// </summary>
[Trait("Category", "Unit")]
public sealed class BillersControllerTests
{
    private const int UserId = 1;
    private const int BillerId = 10;
    private const int SavedBillerId = 100;

    private readonly Mock<IBillerService> _billerService = new(MockBehavior.Strict);

    /// <summary>
    ///     Verifies that the directory endpoint returns all billers when no filters are provided.
    /// </summary>
    [Fact]
    public void GetBillers_WhenNoFilters_ReturnsDirectory()
    {
        // Arrange
        var billers = new List<BillerDataTransferObject>
        {
            new() { Id = BillerId, Name = "Water Co", Category = "Utilities" },
        };
        _billerService.Setup(service => service.GetBillerDirectory()).Returns(billers);
        BillersController controller = CreateController();

        // Act
        IActionResult result = controller.GetBillers(null, null);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(billers);
        _billerService.Verify(service => service.GetBillerDirectory(), Times.Once);
    }

    /// <summary>
    ///     Verifies that category-only filtering is passed to the biller search use case.
    /// </summary>
    [Fact]
    public void GetBillers_WhenCategoryOnlyFilterIsProvided_SearchesByCategory()
    {
        // Arrange
        const string category = "Utilities";
        _billerService
            .Setup(service => service.SearchBillers(string.Empty, category))
            .Returns(new List<BillerDataTransferObject>());
        BillersController controller = CreateController();

        // Act
        IActionResult result = controller.GetBillers(null, category);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _billerService.Verify(service => service.SearchBillers(string.Empty, category), Times.Once);
    }

    /// <summary>
    ///     Verifies that saved billers are requested for the authenticated user.
    /// </summary>
    [Fact]
    public void GetSavedBillers_WhenAuthenticated_ReturnsSavedBillers()
    {
        // Arrange
        var savedBillers = new List<SavedBillerDataTransferObject>
        {
            new() { Id = SavedBillerId, BillerId = BillerId, BillerName = "Water Co" },
        };
        _billerService.Setup(service => service.GetSavedBillers(UserId)).Returns(savedBillers);
        BillersController controller = CreateController();

        // Act
        IActionResult result = controller.GetSavedBillers();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(savedBillers);
    }

    /// <summary>
    ///     Verifies that saving a biller uses the authenticated user and returns Created.
    /// </summary>
    [Fact]
    public void SaveBiller_WhenRequestIsValid_ReturnsCreated()
    {
        // Arrange
        var request = new SaveBillerRequest { BillerId = BillerId, Nickname = "Home Water" };
        var savedBiller = new SavedBillerDataTransferObject
        {
            Id = SavedBillerId,
            BillerId = BillerId,
            BillerName = "Water Co",
            Nickname = "Home Water",
        };
        _billerService.Setup(service => service.SaveBiller(UserId, request)).Returns(savedBiller);
        BillersController controller = CreateController();

        // Act
        IActionResult result = controller.SaveBiller(request);

        // Assert
        CreatedAtActionResult createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(BillersController.GetSavedBillers));
        createdResult.Value.Should().BeEquivalentTo(savedBiller);
    }

    /// <summary>
    ///     Verifies that removing a saved biller delegates to the service for the authenticated user.
    /// </summary>
    [Fact]
    public void RemoveSavedBiller_WhenEntryExists_ReturnsNoContent()
    {
        // Arrange
        _billerService.Setup(service => service.RemoveSavedBiller(UserId, SavedBillerId)).Returns(Result.Success);
        BillersController controller = CreateController();

        // Act
        IActionResult result = controller.RemoveSavedBiller(SavedBillerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private BillersController CreateController()
    {
        var controller = new BillersController(_billerService.Object);
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = UserId,
            },
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}
