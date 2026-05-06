// <copyright file="BillPaymentsControllerTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BankingApp.Api.Controllers;
using BankingApp.Application.DTOs.BillPayment;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Services.BillPayments;
using BankingApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankingApp.Api.Tests.Controller;

public class BillPaymentsControllerTests
{
    private readonly Mock<IBillPaymentService> _mockBillPaymentService;
    private readonly BillPaymentsController _controller;

    public BillPaymentsControllerTests()
    {
        _mockBillPaymentService = new Mock<IBillPaymentService>();
        _controller = new BillPaymentsController(_mockBillPaymentService.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = 1;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetBillers_WhenCalled_ReturnsOkResultWithBillers()
    {
        // Arrange
        var expectedBillers = new List<Biller>
        {
            new Biller { Id = 1, Name = "Biller1" },
            new Biller { Id = 2, Name = "Biller2" }
        };
        _mockBillPaymentService.Setup(s => s.GetAllBillersAsync()).ReturnsAsync(expectedBillers);

        // Act
        var result = await _controller.GetBillers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualBillers = Assert.IsAssignableFrom<IEnumerable<Biller>>(okResult.Value);
        Assert.Equal(expectedBillers, actualBillers);
    }

    [Fact]
    public async Task GetBillers_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _mockBillPaymentService.Setup(s => s.GetAllBillersAsync()).ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetBillers();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetAccounts_WhenValidUser_ReturnsOkResultWithMappedAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Iban = "RO123", Currency = "RON", Balance = 100, AccountName = "Test", Status = AccountStatus.Active }
        };
        _mockBillPaymentService.Setup(s => s.GetAccountsForUserAsync(1)).ReturnsAsync(accounts);

        // Act
        var result = await _controller.GetAccounts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualAccounts = Assert.IsAssignableFrom<IEnumerable<AccountDto>>(okResult.Value);
        Assert.Single(actualAccounts);
    }

    [Fact]
    public async Task GetAccounts_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _mockBillPaymentService.Setup(s => s.GetAccountsForUserAsync(1)).ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetAccounts();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public void CalculateFee_WhenValidAmount_ReturnsOkResultWithFee()
    {
        // Arrange
        decimal amount = 100m;
        decimal expectedFee = 2.5m;
        _mockBillPaymentService.Setup(s => s.CalculateFee(amount)).Returns(expectedFee);

        // Act
        var result = _controller.CalculateFee(amount);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FeeResponseDto>(okResult.Value);
        Assert.Equal(expectedFee, response.Fee);
    }

    [Fact]
    public void Requires2Fa_WhenAmountRequires_ReturnsOkResultWithExpectedValue()
    {
        // Arrange
        decimal amount = 5000m;
        _mockBillPaymentService.Setup(s => s.Requires2Fa(amount)).Returns(true);

        // Act
        var result = _controller.Requires2Fa(amount);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Requires2FaResponseDto>(okResult.Value);
        Assert.True(response.Required);
    }
}
