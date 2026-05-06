using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Api.Controllers;
using BankingApp.Application.DTOs;
using BankingApp.Application.DTOs.Exchange;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Exchange;
using BankingApp.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankingApp.Api.Tests.Controller;

public class ExchangeControllerTests
{
    private readonly Mock<IExchangeService> _mockExchangeService;
    private readonly Mock<IBillPaymentRepository> _mockBillPaymentRepository;
    private readonly ExchangeController _controller;

    public ExchangeControllerTests()
    {
        _mockExchangeService = new Mock<IExchangeService>();
        _mockBillPaymentRepository = new Mock<IBillPaymentRepository>();
        _controller = new ExchangeController(_mockExchangeService.Object, _mockBillPaymentRepository.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = 1;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Fact]
    public void GetPreview_WhenPreviewAndLockBothSucceed_ReturnsOkWithPreviewDto()
    {
        // Arrange
        var previewDto = new ExchangeTransactionResponse { ExchangeRate = 1.2m };
        var lockedRate = new LockedRate { Rate = 1.2m };

        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockExchangeService
            .Setup(s => s.LockRate(1, "EUR", "USD"))
            .Returns(lockedRate);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<ExchangeTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public void GetPreview_WhenGetRatePreviewFails_ReturnsMappedError()
    {
        // Arrange
        var error = Error.Validation("Code", "Description");
        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(error);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public void GetPreview_WhenLockRateFails_ReturnsMappedError()
    {
        // Arrange
        var previewDto = new ExchangeTransactionResponse { ExchangeRate = 1.2m };
        var error = Error.Validation("Code", "Description");

        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockExchangeService
            .Setup(s => s.LockRate(1, "EUR", "USD"))
            .Returns(error);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task Execute_WhenBothAccountIdsAreProvidedAndValid_ReturnsOkWithExchangeDto()
    {
        // Arrange
        var request = new ExchangeTransactionRequest
        {
            SourceAccountId = 10,
            TargetAccountId = 20,
            SourceCurrency = "EUR",
            TargetCurrency = "USD",
            SourceAmount = 100m,
        };

        var userAccounts = new List<Account>
        {
            new Account { Id = 10, Currency = "EUR" },
            new Account { Id = 20, Currency = "USD" },
        };

        var responseDto = new ExchangeTransactionResponse { ExchangeRate = 1.2m };

        _mockBillPaymentRepository
            .Setup(r => r.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockExchangeService
            .Setup(s => s.ExecuteExchange(request))
            .Returns(responseDto);

        // Act
        var result = await _controller.Execute(request);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<ExchangeTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public async Task Execute_WhenAccountIdsAreZeroAndCurrencyMatchFound_ResolvesAccountsAndReturnsOk()
    {
        // Arrange
        var request = new ExchangeTransactionRequest
        {
            SourceAccountId = 0,
            TargetAccountId = 0,
            SourceCurrency = "EUR",
            TargetCurrency = "USD",
            SourceAmount = 100m,
        };

        var userAccounts = new List<Account>
        {
            new Account { Id = 10, Currency = "EUR" },
            new Account { Id = 20, Currency = "USD" },
        };

        var responseDto = new ExchangeTransactionResponse { ExchangeRate = 1.2m };

        _mockBillPaymentRepository
            .Setup(r => r.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockExchangeService
            .Setup(s => s.ExecuteExchange(It.Is<ExchangeTransactionRequest>(req => req.SourceAccountId == 10 && req.TargetAccountId == 20)))
            .Returns(responseDto);

        // Act
        var result = await _controller.Execute(request);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<ExchangeTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public async Task Execute_WhenAccountIdsAreZeroAndNoCurrencyMatchFound_ReturnsNotFound()
    {
        // Arrange
        var request = new ExchangeTransactionRequest
        {
            SourceAccountId = 0,
            TargetAccountId = 0,
            SourceCurrency = "GBP",
            TargetCurrency = "JPY",
            SourceAmount = 100m,
        };

        var userAccounts = new List<Account>
        {
            new Account { Id = 10, Currency = "EUR" },
        };

        _mockBillPaymentRepository
            .Setup(r => r.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        // Act
        var result = await _controller.Execute(request);

        // Assert
        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(actionResult.Value);
    }

    [Fact]
    public async Task Execute_WhenProvidedAccountsDoNotBelongToUser_ReturnsNotFound()
    {
        // Arrange
        var request = new ExchangeTransactionRequest
        {
            SourceAccountId = 10,
            TargetAccountId = 20,
            SourceCurrency = "EUR",
            TargetCurrency = "USD",
            SourceAmount = 100m,
        };

        var userAccounts = new List<Account>
        {
            new Account { Id = 10, Currency = "EUR" },
        };

        _mockBillPaymentRepository
            .Setup(r => r.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        // Act
        var result = await _controller.Execute(request);

        // Assert
        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(actionResult.Value);
    }

    [Fact]
    public async Task Execute_WhenExecuteExchangeFails_ReturnsMappedError()
    {
        // Arrange
        var request = new ExchangeTransactionRequest
        {
            SourceAccountId = 10,
            TargetAccountId = 20,
            SourceCurrency = "EUR",
            TargetCurrency = "USD",
            SourceAmount = 100m,
        };

        var userAccounts = new List<Account>
        {
            new Account { Id = 10, Currency = "EUR" },
            new Account { Id = 20, Currency = "USD" },
        };

        var error = Error.Validation("Code", "Description");

        _mockBillPaymentRepository
            .Setup(r => r.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockExchangeService
            .Setup(s => s.ExecuteExchange(request))
            .Returns(error);

        // Act
        var result = await _controller.Execute(request);

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public void GetHistory_WhenHistoryExistsForUser_ReturnsOkWithList()
    {
        // Arrange
        var historyList = new List<ExchangeTransactionResponse>
        {
            new ExchangeTransactionResponse { Id = 1, SourceCurrency = "EUR", TargetCurrency = "USD" },
            new ExchangeTransactionResponse { Id = 2, SourceCurrency = "GBP", TargetCurrency = "EUR" },
        };

        _mockExchangeService
            .Setup(s => s.GetExchangeHistory(1))
            .Returns(historyList);

        // Act
        var result = _controller.GetHistory();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var actualList = Assert.IsType<List<ExchangeTransactionResponse>>(actionResult.Value);
        Assert.Equal(2, actualList.Count);
    }

    [Fact]
    public void GetHistory_WhenServiceFails_ReturnsMappedError()
    {
        // Arrange
        var error = Error.Validation("Code", "Description");
        _mockExchangeService
            .Setup(s => s.GetExchangeHistory(1))
            .Returns(error);

        // Act
        var result = _controller.GetHistory();

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }
}
