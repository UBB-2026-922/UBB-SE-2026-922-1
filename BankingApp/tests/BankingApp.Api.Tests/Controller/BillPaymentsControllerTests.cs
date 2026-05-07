namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.Billers;
using Application.DTOs.BillPayments;
using Application.Services.BillPayments;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class BillPaymentsControllerTests
{
    private readonly Mock<IBillPaymentService> _mockBillPaymentService;
    private readonly BillPaymentsController _controller;

    public BillPaymentsControllerTests()
    {
        _mockBillPaymentService = new Mock<IBillPaymentService>();
        _controller = new BillPaymentsController(_mockBillPaymentService.Object);

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
    public async Task GetBillers_WhenCalled_ReturnsOkResultWithBillers()
    {
        // Arrange
        var expectedBillers = new List<Biller>
        {
            new Biller { Id = 1, Name = "Biller1" },
            new Biller { Id = 2, Name = "Biller2" },
        };
        _mockBillPaymentService.Setup(service => service.GetAllBillersAsync()).ReturnsAsync(expectedBillers);

        // Act
        IActionResult result = await _controller.GetBillers();

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<Biller> actualBillers = Assert.IsType<IEnumerable<Biller>>(actionResult.Value, exactMatch: false);
        Assert.Equal(expectedBillers, actualBillers);
    }

    [Fact]
    public async Task GetBillers_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _mockBillPaymentService.Setup(service => service.GetAllBillersAsync())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        IActionResult result = await _controller.GetBillers();

        // Assert
        BadRequestObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(errorResult.Value);
    }

    [Fact]
    public async Task GetAccounts_WhenValidUser_ReturnsOkResultWithMappedAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                Id = 1, Iban = "RO123", Currency = "RON", Balance = 100, AccountName = "Test",
                Status = AccountStatus.Active
            },
        };
        _mockBillPaymentService.Setup(service => service.GetAccountsForUserAsync(1)).ReturnsAsync(accounts);

        // Act
        IActionResult result = await _controller.GetAccounts();

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<AccountDto> actualAccounts =
            Assert.IsType<IEnumerable<AccountDto>>(actionResult.Value, exactMatch: false);
        Assert.Single(actualAccounts);
    }

    [Fact]
    public async Task GetAccounts_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _mockBillPaymentService.Setup(service => service.GetAccountsForUserAsync(1))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        IActionResult result = await _controller.GetAccounts();

        // Assert
        BadRequestObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(errorResult.Value);
    }

    [Fact]
    public void CalculateFee_WhenValidAmount_ReturnsOkResultWithFee()
    {
        // Arrange
        const decimal amount = 100m;
        const decimal expectedFee = 2.5m;
        _mockBillPaymentService.Setup(service => service.CalculateFee(amount)).Returns(expectedFee);

        // Act
        IActionResult result = _controller.CalculateFee(amount);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        FeeResponse response = Assert.IsType<FeeResponse>(actionResult.Value);
        Assert.Equal(expectedFee, response.Fee);
    }

    [Fact]
    public void Requires2Fa_WhenAmountRequires_ReturnsOkResultWithExpectedValue()
    {
        // Arrange
        const decimal amount = 5000m;
        _mockBillPaymentService.Setup(service => service.Requires2Fa(amount)).Returns(true);

        // Act
        IActionResult result = _controller.Requires2Fa(amount);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        RequiresTwoFaResponse response = Assert.IsType<RequiresTwoFaResponse>(actionResult.Value);
        Assert.True(response.Required);
    }

    [Fact]
    public async Task ProcessPayment_WhenValidRequest_ReturnsOkResultWithPaymentDetails()
    {
        // Arrange
        var request = new BillPayRequest
        {
            SourceAccountId = 1,
            BillerId = 2,
            BillerReference = "REF123",
            Amount = 100m,
            IsPayInFull = false,
            TwoFaToken = "123456",
        };

        var expectedPayment = new BillPayment
        {
            Id = 10,
            ReceiptNumber = "REC-123",
            Fee = 2.5m,
            Amount = 100m,
            Status = BillPaymentStatus.Completed,
        };

        _mockBillPaymentService
            .Setup(service => service.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(expectedPayment);

        // Act
        IActionResult result = await _controller.ProcessPayment(request);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        BillPayResponse response = Assert.IsType<BillPayResponse>(actionResult.Value);
        Assert.Equal(expectedPayment.Id, response.Id);
        Assert.Equal(expectedPayment.ReceiptNumber, response.ReceiptNumber);
        Assert.Equal(expectedPayment.Fee, response.Fee);
        Assert.Equal(expectedPayment.Amount, response.Amount);
        Assert.Equal(expectedPayment.Status.ToString(), response.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillPayRequest();
        _mockBillPaymentService
            .Setup(service => service.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ThrowsAsync(new InvalidOperationException("Insufficient funds"));

        // Act
        IActionResult result = await _controller.ProcessPayment(request);

        // Assert
        BadRequestObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(errorResult.Value);
    }

    [Fact]
    public async Task SaveBiller_WhenValidRequestAndServiceReturnsTrue_ReturnsOkResult()
    {
        // Arrange
        var request = new SaveBillerRequest
        {
            BillerId = 2,
            Nickname = "My Biller",
        };
        _mockBillPaymentService
            .Setup(service => service.SaveBillerForUserAsync(1, request.BillerId, request.Nickname))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.SaveBiller(request);

        // Assert
        OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(actionResult.Value);
    }

    [Fact]
    public async Task SaveBiller_WhenServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var request = new SaveBillerRequest
        {
            BillerId = 2,
            Nickname = "My Biller",
        };
        _mockBillPaymentService
            .Setup(service => service.SaveBillerForUserAsync(1, request.BillerId, request.Nickname))
            .ReturnsAsync(false);

        // Act
        IActionResult result = await _controller.SaveBiller(request);

        // Assert
        BadRequestObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(errorResult.Value);
    }

    [Fact]
    public async Task SaveBiller_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var request = new SaveBillerRequest { Nickname = "test" };
        _mockBillPaymentService
            .Setup(service => service.SaveBillerForUserAsync(1, request.BillerId, request.Nickname))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        IActionResult result = await _controller.SaveBiller(request);

        // Assert
        BadRequestObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(errorResult.Value);
    }
}
