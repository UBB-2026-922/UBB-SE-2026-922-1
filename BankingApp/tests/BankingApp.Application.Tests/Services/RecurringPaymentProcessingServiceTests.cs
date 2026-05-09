namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.RecurringPayments.Commands;
using ErrorOr;

public sealed class ProcessDueRecurringPaymentsCommandHandlerTests
{
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<IBillPaymentRepository> _billPaymentRepo = MockFactory.CreateBillPaymentRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ProcessDueRecurringPaymentsCommandHandler CreateHandler() => new(
        _recurringPaymentRepo.Object,
        _accountRepo.Object,
        _billPaymentRepo.Object,
        _unitOfWork.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenNoDuePayments_ReturnsZero()
    {
        var command = new ProcessDueRecurringPaymentsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenDuePaymentWithNoAccount_SkipsAndReturnsZero()
    {
        var payment = RecurringPayment.Create(1, 1, 10, 100m, false, RecurringFrequency.Monthly,
            DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        _recurringPaymentRepo.Setup(r => r.ListDueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RecurringPayment>)new[] { payment });
        var command = new ProcessDueRecurringPaymentsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenDuePaymentWithZeroBalanceAccount_SkipsDueToInsufficientFunds()
    {
        var payment = RecurringPayment.Create(1, 1, 10, 100m, false, RecurringFrequency.Monthly,
            DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _recurringPaymentRepo.Setup(r => r.ListDueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RecurringPayment>)new[] { payment });
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new ProcessDueRecurringPaymentsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
