namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.Transfer;
using Application.Repositories.Interfaces;
using Application.Services.Transfers;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class TransferControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultTransferId = 50;

    private readonly Mock<ITransferService> _transferService = new(MockBehavior.Strict);
    private readonly Mock<ITransferRepository> _transferRepository = new(MockBehavior.Strict);

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
        _transferService.Setup(service => service.CreateTransfer(request, DefaultUserId)).Returns(transfer);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.CreateTransfer(request);

        // Assert
        CreatedAtActionResult createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TransferController.GetHistory));
        createdResult.Value.Should().BeEquivalentTo(transfer);
    }

    [Fact]
    public void CreateTransfer_WhenServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTransferRequest { Amount = -1m };
        _transferService
            .Setup(service => service.CreateTransfer(request, DefaultUserId))
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
        _transferService.Setup(service => service.CreateTransfer(request, DefaultUserId)).Returns(transfer);
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
        _transferService.Setup(service => service.CreateTransfer(request, DefaultUserId)).Returns(transfer);
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
        _transferService
            .Setup(service => service.CreateTransfer(request, DefaultUserId))
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
        var history = new List<TransferResponse>
        {
            new() { Id = DefaultTransferId, Amount = 250m, Currency = "RON" },
        };
        _transferService.Setup(service => service.GetHistory(DefaultUserId)).Returns(history);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetHistory();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(history);
        _transferService.Verify(service => service.GetHistory(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetHistory_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _transferService
            .Setup(service => service.GetHistory(DefaultUserId))
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
        var accounts = new List<TransferAccountSelectionResponse>
        {
            new() { Id = 1, Iban = "RO49AAAA1B31007593840000", Currency = "RON", Balance = 5000m },
        };
        _transferService.Setup(service => service.GetAvailableAccounts(DefaultUserId)).Returns(accounts);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.GetAccounts();

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public void GetAccounts_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        _transferService
            .Setup(service => service.GetAvailableAccounts(DefaultUserId))
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
        var request = new TransferIbanValidationRequest { Iban = "RO49AAAA1B31007593840000" };
        var response = new TransferIbanValidationResponse { IsValid = true, BankName = "Alpha Bank" };
        _transferService
            .Setup(service => service.ValidateRecipientIban("RO49AAAA1B31007593840000"))
            .Returns(response);
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ValidateIban(request);

        // Assert
        OkObjectResult successResult = result.Should().BeOfType<OkObjectResult>().Subject;
        successResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void ValidateIban_WhenServiceReturnsError_ReturnsMatchingError()
    {
        // Arrange
        var request = new TransferIbanValidationRequest { Iban = "INVALID" };
        _transferService
            .Setup(service => service.ValidateRecipientIban("INVALID"))
            .Returns(Error.Validation("invalid_iban", "The IBAN is not valid."));
        TransferController controller = CreateController();

        // Act
        IActionResult result = controller.ValidateIban(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
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
        var controller = new TransferController(_transferService.Object, _transferRepository.Object);
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
