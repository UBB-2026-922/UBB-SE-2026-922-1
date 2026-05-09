namespace BankingApp.Application.Tests.Services;

using DTOs.Transfer;
using Repositories.Interfaces;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Services.Transfers;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
///     Unit tests for <see cref="TransferService" />.
/// </summary>
public class TransferServiceTests
{
    private const int DefaultUserId = 1;
    private const int DefaultAccountId = 10;
    private const decimal DefaultBalance = 5000m;
    private const decimal SmallAmount = 100m;
    private const decimal LargeAmount = 1500m;
    private const string DefaultCurrency = "RON";
    private const string DefaultIban = "RO49AAAA1B31007593840000";
    private const string DefaultRecipientName = "John Doe";
    private const string ValidTwoFaToken = "123456";
    private const string InvalidIban = "INVALID";

    private readonly Mock<IDashboardRepository> _dashboardRepository = new(MockBehavior.Strict);
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly TransferService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferServiceTests" /> class.
    /// </summary>
    public TransferServiceTests()
    {
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(It.IsAny<int>()))
            .Returns(new List<Account>
            {
                new()
                {
                    Id = DefaultAccountId,
                    User = new User { Id = DefaultUserId },
                    Balance = DefaultBalance,
                    Status = AccountStatus.Active,
                    Currency = DefaultCurrency
                }
            });
        _dashboardRepository
            .Setup(debitsAccount => debitsAccount.DebitAccount(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(Result.Success);
        _dashboardRepository
            .Setup(addsTransaction => addsTransaction.AddTransaction(It.IsAny<Transaction>()))
            .Returns((Transaction transaction) =>
            {
                transaction.Id = 1;
                return transaction;
            });
        _dashboardRepository
            .Setup(addsTransfer => addsTransfer.AddTransfer(It.IsAny<Transfer>()))
            .Returns((Transfer transfer) => transfer);
        _dashboardRepository
            .Setup(getsByUserId => getsByUserId.GetTransfersByUserId(It.IsAny<int>()))
            .Returns(new List<Transfer>());

        _service = new TransferService(
            _dashboardRepository.Object,
            _otpService.Object,
            NullLogger<TransferService>.Instance);
    }

    /// <summary>
    ///     Verifies that an invalid IBAN returns a validation error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenIbanIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = InvalidIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_iban");
    }

    /// <summary>
    ///     Verifies that a zero amount returns a validation error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenAmountIsZero_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = 0m,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_amount");
    }

    /// <summary>
    ///     Verifies that an invalid currency code returns a validation error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenCurrencyIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = "INVALID"
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_currency");
    }

    /// <summary>
    ///     Verifies that a non-existent account returns a not found error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenAccountNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account>());

        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("transfer.account_not_found");
    }

    /// <summary>
    ///     Verifies that an inactive account returns a forbidden error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenAccountIsNotActive_ReturnsForbiddenError()
    {
        // Arrange
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account>
            {
                new()
                {
                    Id = DefaultAccountId,
                    User = new User { Id = DefaultUserId },
                    Balance = DefaultBalance,
                    Status = AccountStatus.Suspended,
                    Currency = DefaultCurrency
                }
            });

        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.account_not_active");
    }

    /// <summary>
    ///     Verifies that insufficient funds returns a forbidden error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenInsufficientFunds_ReturnsForbiddenError()
    {
        // Arrange
        _dashboardRepository
            .Setup(getsAccountsByUser => getsAccountsByUser.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account>
            {
                new()
                {
                    Id = DefaultAccountId,
                    User = new User { Id = DefaultUserId },
                    Balance = 10m,
                    Status = AccountStatus.Active,
                    Currency = DefaultCurrency
                }
            });

        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.insufficient_funds");
    }

    /// <summary>
    ///     Verifies that a large transfer without a 2FA token returns a forbidden error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenAmountRequires2FaAndTokenMissing_ReturnsForbiddenError()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = LargeAmount,
            Currency = DefaultCurrency,
            TwoFaToken = null
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.2fa_required");
    }

    /// <summary>
    ///     Verifies that a large transfer with an invalid 2FA token returns an unauthorized error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenAmountRequires2FaAndTokenInvalid_ReturnsUnauthorizedError()
    {
        // Arrange
        _otpService
            .Setup(verifiesTotp => verifiesTotp.VerifyTotp(DefaultUserId, It.IsAny<string>()))
            .Returns(false);

        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = LargeAmount,
            Currency = DefaultCurrency,
            TwoFaToken = "wrong"
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("transfer.invalid_2fa_token");
    }

    /// <summary>
    ///     Verifies that a valid small transfer succeeds and returns the transfer response.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenValidSmallAmount_ReturnsTransferResponse()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Amount.Should().Be(SmallAmount);
        result.Value.Currency.Should().Be(DefaultCurrency);
        result.Value.RecipientIban.Should().Be(DefaultIban);
        result.Value.RecipientName.Should().Be(DefaultRecipientName);
    }

    /// <summary>
    ///     Verifies that a valid large transfer with a valid 2FA token succeeds.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenValidLargeAmountWithTwoFaToken_ReturnsTransferResponse()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = LargeAmount,
            Currency = DefaultCurrency,
            TwoFaToken = ValidTwoFaToken
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Amount.Should().Be(LargeAmount);
    }

    /// <summary>
    ///     Verifies that GetHistory returns an empty list when no transfers exist.
    /// </summary>
    [Fact]
    public void GetHistory_WhenNoTransfersExist_ReturnsEmptyList()
    {
        // Act
        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetHistory returns mapped transfers when they exist.
    /// </summary>
    [Fact]
    public void GetHistory_WhenTransfersExist_ReturnsMappedList()
    {
        // Arrange
        _dashboardRepository
            .Setup(getsByUserId => getsByUserId.GetTransfersByUserId(DefaultUserId))
            .Returns(new List<Transfer>
            {
                new()
                {
                    Id = 1,
                    User = new User { Id = DefaultUserId },
                    SourceAccount = new Account { Id = DefaultAccountId },
                    RecipientName = DefaultRecipientName,
                    RecipientIban = DefaultIban,
                    Amount = SmallAmount,
                    Currency = DefaultCurrency,
                    Status = TransferStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                }
            });

        // Act
        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().RecipientName.Should().Be(DefaultRecipientName);
        result.Value.First().Amount.Should().Be(SmallAmount);
    }

    /// <summary>
    ///     Verifies that GetHistory propagates repository errors.
    /// </summary>
    [Fact]
    public void GetHistory_WhenRepositoryFails_ReturnsError()
    {
        // Arrange
        _dashboardRepository
            .Setup(getsByUserId => getsByUserId.GetTransfersByUserId(DefaultUserId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies that a debit failure propagates as a failure error.
    /// </summary>
    [Fact]
    public void CreateTransfer_WhenDebitFails_ReturnsFailureError()
    {
        // Arrange
        _dashboardRepository
            .Setup(debitsAccount => debitsAccount.DebitAccount(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(Error.Failure());

        var request = new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = DefaultIban,
            Amount = SmallAmount,
            Currency = DefaultCurrency
        };

        // Act
        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("transfer.debit_failed");
    }
}
