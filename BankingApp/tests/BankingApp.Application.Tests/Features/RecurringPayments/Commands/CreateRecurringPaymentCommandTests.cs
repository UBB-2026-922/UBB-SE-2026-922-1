namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

using BankingApp.Application.Features.RecurringPayments.Commands;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.RecurringPayments.Dtos;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class CreateRecurringPaymentCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestBillerId = 10;
    private const int SourceAccountId = 20;
    private const decimal Amount = 50m;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _startDate = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBillerRepository> _billerRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync((Biller?)null);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(CreateCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.BillerNotFound);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync(biller);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync((Account?)null);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(CreateCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        Account account = CreateAccount(OtherUserId);
        SetupBillerAndAccount(biller, account, cancellationToken);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(CreateCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        Account account = CreateAccount(TestUserId);
        SetAccountStatus(account, AccountStatus.Closed);
        SetupBillerAndAccount(biller, account, cancellationToken);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(CreateCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotActive);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ShouldReturnDomainError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        Account account = CreateAccount(TestUserId);
        SetupBillerAndAccount(biller, account, cancellationToken);
        CreateRecurringPaymentCommand command = CreateCommand(amount: 0m);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RecurringPaymentErrors.InvalidAmount);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateAndPersistRecurringPayment()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        Account account = CreateAccount(TestUserId);
        RecurringPayment? persistedPayment = null;
        SetupValidFlow(biller, account, cancellationToken, payment => persistedPayment = payment);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();
        CreateRecurringPaymentCommand command = CreateCommand();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedPayment.Should().NotBeNull();
        persistedPayment!.UserId.Should().Be(TestUserId);
        persistedPayment.BillerId.Should().Be(TestBillerId);
        persistedPayment.SourceAccountId.Should().Be(SourceAccountId);
        persistedPayment.Amount.Should().Be(Amount);
        persistedPayment.Frequency.Should().Be(RecurringFrequency.Monthly);
        persistedPayment.NextExecutionDate.Should().Be(_startDate.AddMonths(1));

        result.Value.UserId.Should().Be(TestUserId);
        result.Value.BillerId.Should().Be(TestBillerId);
        result.Value.SourceAccountId.Should().Be(SourceAccountId);
        result.Value.Amount.Should().Be(Amount);
        result.Value.Status.Should().Be(RecurringPaymentStatus.Active);

        VerifyValidFlow(biller, account, persistedPayment, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        Account account = CreateAccount(TestUserId);
        SetupValidFlow(biller, account, cancellationToken);

        CreateRecurringPaymentCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<RecurringPaymentResponse> result = await handler.Handle(CreateCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RecurringPayment>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private CreateRecurringPaymentCommandHandler CreateHandler()
    {
        return new CreateRecurringPaymentCommandHandler(
            _billerRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _recurringPaymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private static CreateRecurringPaymentCommand CreateCommand(decimal amount = Amount)
    {
        return new CreateRecurringPaymentCommand(
            TestUserId,
            TestBillerId,
            SourceAccountId,
            amount,
            false,
            RecurringFrequency.Monthly,
            _startDate,
            null);
    }

    private void SetupBillerAndAccount(Biller biller, Account account, CancellationToken cancellationToken)
    {
        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync(biller);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);
    }

    private void SetupValidFlow(
        Biller biller,
        Account account,
        CancellationToken cancellationToken,
        Action<RecurringPayment>? onPersist = null)
    {
        SetupBillerAndAccount(biller, account, cancellationToken);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        if (onPersist is null)
        {
            _recurringPaymentRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<RecurringPayment>(), cancellationToken))
                .Returns(Task.CompletedTask);
        }
        else
        {
            _recurringPaymentRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<RecurringPayment>(), cancellationToken))
                .Callback<RecurringPayment, CancellationToken>((payment, _) => onPersist(payment))
                .Returns(Task.CompletedTask);
        }

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void VerifyValidFlow(Biller biller, Account account, RecurringPayment payment, CancellationToken cancellationToken)
    {
        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(biller.Id, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.AddAsync(payment, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _billerRepositoryMock.VerifyNoOtherCalls();
        _accountRepositoryMock.VerifyNoOtherCalls();
        _recurringPaymentRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static Biller CreateBiller()
    {
        return new Biller
        {
            Id = TestBillerId,
            Name = "Electric Company",
            Category = BillerCategory.Utilities,
            IsActive = true
        };
    }

    private static Account CreateAccount(int userId)
    {
        var account = Account.Open(
            userId,
            Iban.Create("RO12BANK1234567890123456").Value,
            Currency.FromCode("RON"),
            AccountType.Checking,
            "Main",
            _testNow);

        account.ChangeBalance(new Money(1000m, Currency.FromCode("RON")), _testNow);
        return account;
    }

    private static void SetAccountStatus(Account account, AccountStatus status)
    {
        typeof(Account)
            .GetProperty(nameof(Account.Status))!
            .SetValue(account, status);
    }
}
