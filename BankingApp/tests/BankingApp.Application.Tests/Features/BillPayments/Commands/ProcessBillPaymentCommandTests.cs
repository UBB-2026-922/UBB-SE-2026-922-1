namespace BankingApp.Application.Tests.Features.BillPayments.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Features.BillPayments.Commands;
using BankingApp.Domain.Aggregates.AccountAggregate;
using BankingApp.Domain.Aggregates.BillPaymentAggregate;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Enums;
using BankingApp.Domain.ReferenceData.Billers;
using BankingApp.Application.Common.Contracts.Security;
using BankingApp.Application.Common.Contracts;
using Microsoft.Extensions.Logging;
using NodaMoney;
using ErrorOr;

public sealed class ProcessBillPaymentCommandTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IBillPaymentRepository> _billPaymentRepositoryMock;
    private readonly Mock<IBillerRepository> _billerRepositoryMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemClock> _clockMock;
    private readonly Mock<ILogger<ProcessBillPaymentCommandHandler>> _loggerMock;
    private readonly ProcessBillPaymentCommandHandler _handler;

    public ProcessBillPaymentCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _billPaymentRepositoryMock = MockFactory.CreateBillPaymentRepositoryMock();
        _billerRepositoryMock = MockFactory.CreateBillerRepositoryMock();
        _otpServiceMock = MockFactory.CreateOtpServiceMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _clockMock = MockFactory.CreateSystemClockMock();
        _loggerMock = new Mock<ILogger<ProcessBillPaymentCommandHandler>>();

        _handler = new ProcessBillPaymentCommandHandler(
            _accountRepositoryMock.Object,
            _billPaymentRepositoryMock.Object,
            _billerRepositoryMock.Object,
            _otpServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            _loggerMock.Object);
    }

    private static Account CreateTestAccount(int userId, AccountStatus status, Currency currency, decimal balanceAmount = 0m)
    {
        Account account = Account.Open(userId, null!, currency, AccountType.Checking, null, DateTime.UtcNow);
        typeof(Account).GetProperty("Status")!.SetValue(account, status);
        typeof(Account).GetProperty("Balance")!.SetValue(account, new Money(balanceAmount, currency));
        return account;
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 10m, null);
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 10m, null);
        Account account = CreateTestAccount(99, AccountStatus.Active, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 10m, null);
        Account account = CreateTestAccount(1, AccountStatus.Closed, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.AccountNotActive);
    }

    [Fact]
    public async Task Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 10m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((Biller?)null);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAmountAboveThresholdAndTwoFaTokenMissing_ShouldReturnTwoFaRequiredError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 1500m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"));
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.TwoFaRequired);
    }

    [Fact]
    public async Task Handle_WhenAmountAboveThresholdAndTwoFaTokenIsInvalid_ShouldReturnInvalidTwoFaTokenError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 1500m, "invalid");
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"));
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);
        _otpServiceMock.Setup(service => service.VerifyTotp(1, "invalid")).Returns(false);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidTwoFaToken);
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 100m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 50m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.InsufficientFunds);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDebitAccountAndCreateBillPaymentTransaction()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 100m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        account.Balance.Amount.Should().Be(99.50m);
        account.Transactions.Should().ContainSingle(transaction => transaction.Type == "BILL_PAYMENT" && transaction.Amount.Amount == 100m);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldMarkBillPaymentAsProcessed()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 50m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        BillPayment? savedPayment = null;
        _billPaymentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<BillPayment>(), It.IsAny<CancellationToken>()))
            .Callback<BillPayment, CancellationToken>((payment, token) => savedPayment = payment)
            .Returns(Task.CompletedTask);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        savedPayment.Should().NotBeNull();
        savedPayment!.Status.Should().Be(BillPaymentStatus.Completed);
        savedPayment.ReceiptNumber.Should().StartWith("RCP-");
        savedPayment.LedgerTransactionId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        var command = new ProcessBillPaymentCommand(1, 2, 3, "REF", 50m, null);
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };
        
        _accountRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repo => repo.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BankingApp.Application.Features.BillPayments.Dtos.BillPayResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
