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
///     Unit tests for <see cref="TransferService" /> aligned with the current repository split.
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
    private readonly Mock<ITransferRepository> _transferRepository = new(MockBehavior.Strict);
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly TransferService _service;

    public TransferServiceTests()
    {
        _dashboardRepository
            .Setup(repository => repository.GetAccountsByUser(It.IsAny<int>()))
            .Returns(new List<Account> { CreateAccount(DefaultBalance, AccountStatus.Active) });
        _dashboardRepository
            .Setup(repository => repository.DebitAccount(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(Result.Success);
        _dashboardRepository
            .Setup(repository => repository.AddTransaction(It.IsAny<Transaction>()))
            .Returns((Transaction transaction) =>
            {
                transaction.Id = 1;
                transaction.TransactionRef = "TRF-123";
                return transaction;
            });

        _transferRepository
            .Setup(repository => repository.Create(It.IsAny<Transfer>()))
            .Returns((Transfer transfer) =>
            {
                transfer.Id = 1;
                return transfer;
            });
        _transferRepository
            .Setup(repository => repository.GetByUserId(It.IsAny<int>()))
            .Returns(new List<Transfer>());

        _service = new TransferService(
            _dashboardRepository.Object,
            _transferRepository.Object,
            _otpService.Object,
            NullLogger<TransferService>.Instance);
    }

    [Fact]
    public void CreateTransfer_WhenIbanIsInvalid_ReturnsValidationError()
    {
        CreateTransferRequest request = CreateRequest(iban: InvalidIban);

        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_iban");
    }

    [Fact]
    public void CreateTransfer_WhenAmountIsZero_ReturnsValidationError()
    {
        CreateTransferRequest request = CreateRequest(amount: 0m);

        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_amount");
    }

    [Fact]
    public void CreateTransfer_WhenCurrencyIsInvalid_ReturnsValidationError()
    {
        CreateTransferRequest request = CreateRequest(currency: "INVALID");

        ErrorOr<TransferResponse> result = _service.CreateTransfer(request, DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("transfer.invalid_currency");
    }

    [Fact]
    public void CreateTransfer_WhenAccountNotFound_ReturnsNotFoundError()
    {
        _dashboardRepository
            .Setup(repository => repository.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account>());

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("transfer.account_not_found");
    }

    [Fact]
    public void CreateTransfer_WhenAccountIsNotActive_ReturnsForbiddenError()
    {
        _dashboardRepository
            .Setup(repository => repository.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account> { CreateAccount(DefaultBalance, AccountStatus.Suspended) });

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.account_not_active");
    }

    [Fact]
    public void CreateTransfer_WhenInsufficientFunds_ReturnsForbiddenError()
    {
        _dashboardRepository
            .Setup(repository => repository.GetAccountsByUser(DefaultUserId))
            .Returns(new List<Account> { CreateAccount(10m, AccountStatus.Active) });

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.insufficient_funds");
    }

    [Fact]
    public void CreateTransfer_WhenAmountRequires2FaAndTokenMissing_ReturnsForbiddenError()
    {
        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(amount: LargeAmount, twoFaToken: null), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("transfer.2fa_required");
    }

    [Fact]
    public void CreateTransfer_WhenAmountRequires2FaAndTokenInvalid_ReturnsUnauthorizedError()
    {
        _otpService
            .Setup(service => service.VerifyTotp(DefaultUserId, It.IsAny<string>()))
            .Returns(false);

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(amount: LargeAmount, twoFaToken: "wrong"), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("transfer.invalid_2fa_token");
    }

    [Fact]
    public void CreateTransfer_WhenValidSmallAmount_ReturnsTransferResponse()
    {
        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeFalse();
        result.Value.Amount.Should().Be(SmallAmount);
        result.Value.Currency.Should().Be(DefaultCurrency);
        result.Value.RecipientIban.Should().Be(DefaultIban);
        result.Value.RecipientName.Should().Be(DefaultRecipientName);
        result.Value.TransactionId.Should().Be(1);
        result.Value.TransactionRef.Should().Be("TRF-123");
        _transferRepository.Verify(repository => repository.Create(It.IsAny<Transfer>()), Times.Once);
    }

    [Fact]
    public void CreateTransfer_WhenValidLargeAmountWithTwoFaToken_ReturnsTransferResponse()
    {
        ErrorOr<TransferResponse> result = _service.CreateTransfer(
            CreateRequest(amount: LargeAmount, twoFaToken: ValidTwoFaToken),
            DefaultUserId);

        result.IsError.Should().BeFalse();
        result.Value.Amount.Should().Be(LargeAmount);
    }

    [Fact]
    public void GetHistory_WhenNoTransfersExist_ReturnsEmptyList()
    {
        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void GetHistory_WhenTransfersExist_ReturnsMappedList()
    {
        _transferRepository
            .Setup(repository => repository.GetByUserId(DefaultUserId))
            .Returns(new List<Transfer>
            {
                new()
                {
                    Id = 1,
                    User = new User { Id = DefaultUserId },
                    SourceAccount = new Account { Id = DefaultAccountId },
                    Transaction = new Transaction { Id = 99, TransactionRef = "TRF-ABC" },
                    RecipientName = DefaultRecipientName,
                    RecipientIban = DefaultIban,
                    Amount = SmallAmount,
                    Currency = DefaultCurrency,
                    Status = TransferStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                }
            });

        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().RecipientName.Should().Be(DefaultRecipientName);
        result.Value.First().Amount.Should().Be(SmallAmount);
        result.Value.First().TransactionId.Should().Be(99);
        result.Value.First().TransactionRef.Should().BeNull();
    }

    [Fact]
    public void GetHistory_WhenRepositoryFails_ReturnsError()
    {
        _transferRepository
            .Setup(repository => repository.GetByUserId(DefaultUserId))
            .Returns(Error.Failure());

        ErrorOr<List<TransferResponse>> result = _service.GetHistory(DefaultUserId);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void CreateTransfer_WhenDebitFails_ReturnsFailureError()
    {
        _dashboardRepository
            .Setup(repository => repository.DebitAccount(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(Error.Failure());

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("transfer.debit_failed");
    }

    [Fact]
    public void CreateTransfer_WhenPersistenceFails_ReturnsFailureError()
    {
        _transferRepository
            .Setup(repository => repository.Create(It.IsAny<Transfer>()))
            .Returns(Error.Failure());

        ErrorOr<TransferResponse> result = _service.CreateTransfer(CreateRequest(), DefaultUserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("transfer.persistence_failed");
    }

    private static Account CreateAccount(decimal balance, AccountStatus status)
    {
        return new Account
        {
            Id = DefaultAccountId,
            User = new User { Id = DefaultUserId },
            Balance = balance,
            Status = status,
            Currency = DefaultCurrency,
            Iban = DefaultIban,
            AccountName = "Main Account"
        };
    }

    private static CreateTransferRequest CreateRequest(
        string? iban = null,
        decimal amount = SmallAmount,
        string currency = DefaultCurrency,
        string? twoFaToken = null)
    {
        return new CreateTransferRequest
        {
            SourceAccountId = DefaultAccountId,
            RecipientName = DefaultRecipientName,
            RecipientIban = iban ?? DefaultIban,
            Amount = amount,
            Currency = currency,
            TwoFaToken = twoFaToken
        };
    }
}
