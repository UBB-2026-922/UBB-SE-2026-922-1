namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.Transfer;
using Application.Repositories.Interfaces;
using Application.Services.Transfers;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;

[Trait("Category", "Unit")]
public sealed class TransferControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultTransferId = 50;

    private readonly Mock<ITransferService> _transferService = new(MockBehavior.Strict);
    private readonly Mock<ITransferRepository> _transferRepository = new(MockBehavior.Strict);
    private readonly Mock<IDashboardRepository> _dashboardRepository = new(MockBehavior.Strict);

    [Fact]
    public void CreateTransfer_WhenRequestIsValid_ReturnsCreatedWithTransfer()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = 1,
            RecipientName = "Jane Doe",
            RecipientIban = "RO49AAAA1B31007593840000",
            Amount = 250m,
            Currency = "RON",
        };
        var transfer = new TransferResponse
        {
            Id = DefaultTransferId,
            SourceAccountId = 1,
            RecipientName = "Jane Doe",
            RecipientIban = "RO49AAAA1B31007593840000",
            Amount = 250m,
            Currency = "RON",
            TransactionRef = "TXN-001",
        };
        _transferRepository.Setup(r => r.Create(It.IsAny<Transfer>())).Returns((Transfer t) => 
        {
            t.Id = DefaultTransferId;
            return t;
        });
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.CreateTransfer(request);

        // Assert
        CreatedAtActionResult createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TransferController.GetHistory));
        Transfer returnedTransfer = createdResult.Value.Should().BeOfType<Transfer>().Subject;
        returnedTransfer.Id.Should().Be(DefaultTransferId);
    }

    [Fact]
    public void CreateTransfer_WhenServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTransferRequest { Amount = -1m };
        _transferRepository
            .Setup(r => r.Create(It.IsAny<Transfer>()))
            .Returns(Error.Validation("invalid_amount", "Amount must be positive."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.CreateTransfer(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ExecuteTransfer_WhenRequestIsValid_ReturnsOkWithTransactionRef()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = 1,
            RecipientName = "Jane Doe",
            RecipientIban = "RO49AAAA1B31007593840000",
            Amount = 500m,
            Currency = "EUR",
        };
        var transfer = new TransferResponse
        {
            Id = DefaultTransferId,
            TransactionRef = "TXN-002",
        };
        _transferRepository.Setup(r => r.Create(It.IsAny<Transfer>())).Returns(new Transfer { Id = DefaultTransferId, Transaction = new Transaction { TransactionRef = "TXN-002" } });
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ExecuteTransfer(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(new TransferExecutionResponse { TransactionRef = "TXN-002" });
    }

    [Fact]
    public void ExecuteTransfer_WhenTransactionRefIsNull_ReturnsEmptyRef()
    {
        // Arrange
        var request = new CreateTransferRequest { SourceAccountId = 1, Amount = 100m, Currency = "RON" };
        var transfer = new TransferResponse { Id = DefaultTransferId, TransactionRef = null };
        _transferRepository.Setup(r => r.Create(It.IsAny<Transfer>())).Returns(new Transfer { Id = DefaultTransferId });
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ExecuteTransfer(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(new TransferExecutionResponse { TransactionRef = string.Empty });
    }

    [Fact]
    public void ExecuteTransfer_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        var request = new CreateTransferRequest { Amount = 500m };
        _transferRepository
            .Setup(r => r.Create(It.IsAny<Transfer>()))
            .Returns(Error.Unauthorized("unauthorized", "2FA required."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ExecuteTransfer(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void GetHistory_WhenServiceReturnsTransfers_ReturnsOkWithHistory()
    {
        // Arrange
        var history = new List<Transfer>
        {
            new() { Id = DefaultTransferId, Amount = 250m, Currency = "RON", Transaction = new Transaction { TransactionRef = "TXN-001" } },
        };
        var expectedResponse = new List<TransferResponse>
        {
            new() { Id = DefaultTransferId, Amount = 250m, Currency = "RON", Status = default, CreatedAt = history[0].CreatedAt, RecipientBankName = null, RecipientIban = string.Empty, RecipientName = string.Empty, Reference = null, SourceAccountId = 0, TransactionId = 0, TransactionRef = "TXN-001" }
        };
        _transferRepository.Setup(r => r.GetByUserId(DefaultUserId)).Returns(history);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetHistory();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(expectedResponse);
        _transferRepository.Verify(r => r.GetByUserId(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetHistory_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _transferRepository
            .Setup(r => r.GetByUserId(DefaultUserId))
            .Returns(Error.Failure("query_failed", "Could not retrieve history."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetHistory();

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void GetAccounts_WhenServiceReturnsAccounts_ReturnsOkWithAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Iban = "RO49AAAA1B31007593840000", Currency = "RON", Balance = 5000m, Status = AccountStatus.Active, AccountName = "Test" },
        };
        var expectedResponses = new List<TransferAccountSelectionResponse>
        {
            new() { Id = 1, Iban = "RO49AAAA1B31007593840000", Currency = "RON", Balance = 5000m, AccountName = "Test" }
        };
        _dashboardRepository.Setup(r => r.GetAccountsByUser(DefaultUserId)).Returns(accounts);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetAccounts();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public void GetAccounts_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _dashboardRepository
            .Setup(r => r.GetAccountsByUser(DefaultUserId))
            .Returns(Error.Failure("query_failed", "Could not load accounts."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetAccounts();

        // Assert
        ObjectResult errorResult = result.Should().BeOfType<ObjectResult>().Subject;
        errorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ValidateIban_WhenIbanIsValid_ReturnsOkWithValidation()
    {
        // Arrange
        var request = new TransferIbanValidationRequest { Iban = "RO49BTRL0000000000000000" };
        var response = new TransferIbanValidationResponse { IsValid = true, BankName = "Romanian Bank" };
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ValidateIban(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void ValidateIban_WhenServiceReturnsError_ReturnsValidButFalse()
    {
        // Arrange
        var request = new TransferIbanValidationRequest { Iban = "INVALID" };
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ValidateIban(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        TransferIbanValidationResponse response = successResult.Value.Should().BeOfType<TransferIbanValidationResponse>().Subject;
        response.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetFxPreview_WhenServiceSucceeds_ReturnsOkWithPreview()
    {
        // Arrange
        var preview = new TransferForexPreviewResponse { ExchangeRate = 4.95m, ConvertedAmount = 495m };
        _transferService
            .Setup(service => service.GetFxPreview("EUR", "RON", 100m))
            .Returns(preview);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetFxPreview("EUR", "RON", 100m);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(preview);
    }

    [Fact]
    public void GetFxPreview_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _transferService
            .Setup(service => service.GetFxPreview("XXX", "YYY", 100m))
            .Returns(Error.Validation("unsupported_currency", "Currency pair not supported."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetFxPreview("XXX", "YYY", 100m);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private TransferController CreateController()
    {
        var controller = new TransferController(_transferService.Object, _transferRepository.Object, _dashboardRepository.Object);
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
