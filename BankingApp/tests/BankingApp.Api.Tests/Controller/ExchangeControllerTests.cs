namespace BankingApp.Api.Tests.Controller;

using Controllers;
using BankingApp.Application.Common.Dtos;
using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Application.Features.UserProfile.Repositories;
using BankingApp.Application.Features.Forex.Services;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class ExchangeControllerTests
{
    private readonly Mock<IForexService> _mockForexService;
    private readonly Mock<IBillPaymentRepository> _mockBillPaymentRepository;
    private readonly ExchangeController _controller;

    public ExchangeControllerTests()
    {
        _mockForexService = new Mock<IForexService>();
        _mockBillPaymentRepository = new Mock<IBillPaymentRepository>();
        _controller = new ExchangeController(_mockForexService.Object, _mockBillPaymentRepository.Object);

        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = 1
            }
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Fact]
    public void GetPreview_WhenPreviewAndLockBothSucceed_ReturnsOkWithPreviewDto()
    {
        // Arrange
        var previewDto = new ForexTransactionResponse { ExchangeRate = 1.2m };
        var lockedRate = new LockedRate { Rate = 1.2m };

        _mockForexService
            .Setup(service => service.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockForexService
            .Setup(service => service.LockRate(1, "EUR", "USD"))
            .Returns(lockedRate);

        // Act
        IActionResult result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        ForexTransactionResponse actualDto = Assert.IsType<ForexTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public void GetPreview_WhenGetRatePreviewFails_ReturnsMappedError()
    {
        // Arrange
        var error = Error.Validation("Code", "Description");
        _mockForexService
            .Setup(exchangeService => exchangeService.GetRatePreview("EUR", "USD", 100m))
            .Returns(error);

        // Act
        IActionResult result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        BadRequestObjectResult actionResult = Assert.IsType<BadRequestObjectResult>(result);
        ApplicationErrorResponse errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public void GetPreview_WhenLockRateFails_ReturnsMappedError()
    {
        // Arrange
        var previewDto = new ForexTransactionResponse { ExchangeRate = 1.2m };
        var error = Error.Validation("Code", "Description");

        _mockForexService
            .Setup(service => service.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockForexService
            .Setup(service => service.LockRate(1, "EUR", "USD"))
            .Returns(error);

        // Act
        IActionResult result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        BadRequestObjectResult actionResult = Assert.IsType<BadRequestObjectResult>(result);
        ApplicationErrorResponse errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task Execute_WhenBothAccountIdsAreProvidedAndValid_ReturnsOkWithExchangeDto()
    {
        // Arrange
        var request = new ForexTransactionRequest
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

        var responseDto = new ForexTransactionResponse { ExchangeRate = 1.2m };

        _mockBillPaymentRepository
            .Setup(repository => repository.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockForexService
            .Setup(service => service.ExecuteExchange(request))
            .Returns(responseDto);

        // Act
        IActionResult result = await _controller.Execute(request);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        ForexTransactionResponse actualDto = Assert.IsType<ForexTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public async Task Execute_WhenAccountIdsAreZeroAndCurrencyMatchFound_ResolvesAccountsAndReturnsOk()
    {
        // Arrange
        var request = new ForexTransactionRequest
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

        var responseDto = new ForexTransactionResponse { ExchangeRate = 1.2m };

        _mockBillPaymentRepository
            .Setup(repository => repository.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockForexService
            .Setup(service => service.ExecuteExchange(It.Is<ForexTransactionRequest>(req => req.SourceAccountId == 10 && req.TargetAccountId == 20)))
            .Returns(responseDto);

        // Act
        IActionResult result = await _controller.Execute(request);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        ForexTransactionResponse actualDto = Assert.IsType<ForexTransactionResponse>(actionResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public async Task Execute_WhenAccountIdsAreZeroAndNoCurrencyMatchFound_ReturnsNotFound()
    {
        // Arrange
        var request = new ForexTransactionRequest
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
            .Setup(billPaymentRepository => billPaymentRepository.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        // Act
        IActionResult result = await _controller.Execute(request);

        // Assert
        NotFoundObjectResult actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(actionResult.Value);
    }

    [Fact]
    public async Task Execute_WhenProvidedAccountsDoNotBelongToUser_ReturnsNotFound()
    {
        // Arrange
        var request = new ForexTransactionRequest
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
            .Setup(billPaymentRepository => billPaymentRepository.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        // Act
        IActionResult result = await _controller.Execute(request);

        // Assert
        NotFoundObjectResult actionResult = Assert.IsType<NotFoundObjectResult>(result);
        actionResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_WhenExecuteExchangeFails_ReturnsMappedError()
    {
        // Arrange
        var request = new ForexTransactionRequest
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
            .Setup(repository => repository.GetAccountsByUserIdAsync(1))
            .ReturnsAsync(userAccounts);

        _mockForexService
            .Setup(service => service.ExecuteExchange(request))
            .Returns(error);

        // Act
        IActionResult result = await _controller.Execute(request);

        // Assert
        BadRequestObjectResult actionResult = Assert.IsType<BadRequestObjectResult>(result);
        ApplicationErrorResponse errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        errorResponse.ErrorCode.Should().Be("Code");
    }

    [Fact]
    public void GetHistory_WhenHistoryExistsForUser_ReturnsOkWithList()
    {
        // Arrange
        var historyList = new List<ForexTransactionResponse>
        {
            new() { Id = 1, SourceCurrency = "EUR", TargetCurrency = "USD" },
            new() { Id = 2, SourceCurrency = "GBP", TargetCurrency = "EUR" },
        };

        _mockForexService
            .Setup(service => service.GetExchangeHistory(1))
            .Returns(historyList);

        // Act
        IActionResult result = _controller.GetHistory();

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        List<ForexTransactionResponse> actualList = Assert.IsType<List<ForexTransactionResponse>>(actionResult.Value);
        actualList.Count.Should().Be(2);
    }

    [Fact]
    public void GetHistory_WhenServiceFails_ReturnsMappedError()
    {
        // Arrange
        var error = Error.Validation("Code", "Description");
        _mockForexService
            .Setup(service => service.GetExchangeHistory(1))
            .Returns(error);

        // Act
        IActionResult result = _controller.GetHistory();

        // Assert
        BadRequestObjectResult actionResult = Assert.IsType<BadRequestObjectResult>(result);
        ApplicationErrorResponse errorResponse = Assert.IsType<ApplicationErrorResponse>(actionResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }
}
