namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

using BankingApp.Application.Features.RecurringPayments.Commands;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class ProcessDueRecurringPaymentsCommandTests
{
    private const int TestUserId = 1;
    private const int SourceAccountId = 20;
    private const int BillerId = 10;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _startDate = new(2026, 4, 17, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IBillPaymentRepository> _billPaymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenNoDuePayments_ShouldReturnSuccessWithoutProcessing()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.ListDueAsync(_testNow, cancellationToken))
            .ReturnsAsync([]);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.ListDueAsync(_testNow, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenDuePaymentProcessedSuccessfully_ShouldAdvanceNextExecutionDate()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment recurring = CreateRecurringPayment();
        Account account = CreateAccount(balance: 1000m);
        SetupSuccessfulProcessing(recurring, account, cancellationToken);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        recurring.NextExecutionDate.Should().Be(_startDate.AddMonths(2));

        VerifySuccessfulProcessing(recurring, account, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenDuePaymentHasNoBiller_ShouldSkipPayment()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment recurring = CreateRecurringPayment();
        SetRecurringAmount(recurring, 0m);
        Account account = CreateAccount(balance: 1000m);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.ListDueAsync(_testNow, cancellationToken))
            .ReturnsAsync([recurring]);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
        recurring.NextExecutionDate.Should().Be(_startDate.AddMonths(1));

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.ListDueAsync(_testNow, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenDuePaymentHasNoAccount_ShouldSkipPayment()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment recurring = CreateRecurringPayment();

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.ListDueAsync(_testNow, cancellationToken))
            .ReturnsAsync([recurring]);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync((Account?)null);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.ListDueAsync(_testNow, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenDuePaymentHasInsufficientFunds_ShouldSkipPayment()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment recurring = CreateRecurringPayment();
        Account account = CreateAccount(balance: 10m);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.ListDueAsync(_testNow, cancellationToken))
            .ReturnsAsync([recurring]);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
        account.Balance.Amount.Should().Be(10m);

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.ListDueAsync(_testNow, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPaymentIsProcessed_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment recurring = CreateRecurringPayment();
        Account account = CreateAccount(balance: 1000m);
        SetupSuccessfulProcessing(recurring, account, cancellationToken);

        ProcessDueRecurringPaymentsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessDueRecurringPaymentsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        VerifySuccessfulProcessing(recurring, account, cancellationToken);
    }

    private ProcessDueRecurringPaymentsCommandHandler CreateHandler()
    {
        return new ProcessDueRecurringPaymentsCommandHandler(
            _recurringPaymentRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _billPaymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private void SetupSuccessfulProcessing(RecurringPayment recurring, Account account, CancellationToken cancellationToken)
    {
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.ListDueAsync(_testNow, cancellationToken))
            .ReturnsAsync([recurring]);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(account, cancellationToken))
            .Returns(Task.CompletedTask);

        _billPaymentRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<BillPayment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.UpdateAsync(recurring, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void VerifySuccessfulProcessing(RecurringPayment recurring, Account account, CancellationToken cancellationToken)
    {
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.ListDueAsync(_testNow, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(account, cancellationToken), Times.Once);
        _billPaymentRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<BillPayment>(), cancellationToken), Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.UpdateAsync(recurring, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _recurringPaymentRepositoryMock.VerifyNoOtherCalls();
        _accountRepositoryMock.VerifyNoOtherCalls();
        _billPaymentRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static RecurringPayment CreateRecurringPayment()
    {
        return RecurringPayment.Create(
            TestUserId,
            BillerId,
            SourceAccountId,
            50m,
            false,
            RecurringFrequency.Monthly,
            _startDate,
            null,
            _testNow).Value;
    }

    private static Account CreateAccount(decimal balance)
    {
        var account = Account.Open(
            TestUserId,
            Iban.Create("RO12BANK1234567890123456").Value,
            Currency.FromCode("RON"),
            AccountType.Checking,
            "Main",
            _testNow);

        account.ChangeBalance(new Money(balance, Currency.FromCode("RON")), _testNow);
        return account;
    }

    private static void SetRecurringAmount(RecurringPayment recurring, decimal amount)
    {
        typeof(RecurringPayment)
            .GetProperty(nameof(RecurringPayment.Amount))!
            .SetValue(recurring, amount);
    }
}
