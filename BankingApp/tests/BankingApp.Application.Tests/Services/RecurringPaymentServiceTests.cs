namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.RecurringPayments.Commands;
using BankingApp.Application.Features.RecurringPayments.Dtos;
using BankingApp.Application.Features.RecurringPayments.Queries;
using ErrorOr;

public sealed class CreateRecurringPaymentCommandHandlerTests
{
    private readonly Mock<IBillerRepository> _billerRepo = MockFactory.CreateBillerRepository();
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private CreateRecurringPaymentCommandHandler CreateHandler() => new(
        _billerRepo.Object,
        _accountRepo.Object,
        _recurringPaymentRepo.Object,
        _unitOfWork.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenBillerNotFound_ReturnsError()
    {
        var command = new CreateRecurringPaymentCommand(1, 99, 10, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow, null);

        ErrorOr<RecurringPaymentResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsError()
    {
        var biller = new Biller { Id = 1, Name = "Test Biller", Category = BillerCategory.Utilities };
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        var command = new CreateRecurringPaymentCommand(1, 1, 99, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow, null);

        ErrorOr<RecurringPaymentResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAmountIsInvalid_ReturnsValidationError()
    {
        var biller = new Biller { Id = 1, Name = "Test Biller", Category = BillerCategory.Utilities };
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new CreateRecurringPaymentCommand(1, 1, 10, 0m, false, RecurringFrequency.Monthly, DateTime.UtcNow, null);

        ErrorOr<RecurringPaymentResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_WhenEndDateBeforeStartDate_ReturnsValidationError()
    {
        var biller = new Biller { Id = 1, Name = "Test Biller", Category = BillerCategory.Utilities };
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var startDate = DateTime.UtcNow;
        var command = new CreateRecurringPaymentCommand(1, 1, 10, 100m, false, RecurringFrequency.Monthly, startDate, startDate.AddDays(-1));

        ErrorOr<RecurringPaymentResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesPaymentAndSaves()
    {
        var biller = new Biller { Id = 1, Name = "Test Biller", Category = BillerCategory.Utilities };
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new CreateRecurringPaymentCommand(1, 1, 10, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null);

        ErrorOr<RecurringPaymentResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _recurringPaymentRepo.Verify(r => r.AddAsync(It.IsAny<RecurringPayment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class GetRecurringPaymentsQueryHandlerTests
{
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();

    private GetRecurringPaymentsQueryHandler CreateHandler() => new(_recurringPaymentRepo.Object);

    [Fact]
    public async Task Handle_WhenNoPayments_ReturnsEmptyList()
    {
        var query = new GetRecurringPaymentsQuery(1);

        ErrorOr<List<RecurringPaymentResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPaymentsExist_ReturnsMappedList()
    {
        var payment = RecurringPayment.Create(1, 1, 1, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        _recurringPaymentRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RecurringPayment>)new[] { payment });
        var query = new GetRecurringPaymentsQuery(1);

        ErrorOr<List<RecurringPaymentResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
    }
}

public sealed class PauseRecurringPaymentCommandHandlerTests
{
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private PauseRecurringPaymentCommandHandler CreateHandler() => new(
        _recurringPaymentRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsNotFoundError()
    {
        var command = new PauseRecurringPaymentCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenPaymentBelongsToDifferentUser_ReturnsError()
    {
        var payment = RecurringPayment.Create(2, 1, 1, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        _recurringPaymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        var command = new PauseRecurringPaymentCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_PausesAndSaves()
    {
        var payment = RecurringPayment.Create(1, 1, 1, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        _recurringPaymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        var command = new PauseRecurringPaymentCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _recurringPaymentRepo.Verify(r => r.UpdateAsync(It.IsAny<RecurringPayment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class ResumeRecurringPaymentCommandHandlerTests
{
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private ResumeRecurringPaymentCommandHandler CreateHandler() => new(
        _recurringPaymentRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsError()
    {
        var command = new ResumeRecurringPaymentCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_ResumesAndSaves()
    {
        var payment = RecurringPayment.Create(1, 1, 1, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        payment.Pause(1);
        _recurringPaymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        var command = new ResumeRecurringPaymentCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class CancelRecurringPaymentCommandHandlerTests
{
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepo = MockFactory.CreateRecurringPaymentRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private CancelRecurringPaymentCommandHandler CreateHandler() => new(
        _recurringPaymentRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsError()
    {
        var command = new CancelRecurringPaymentCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_CancelsAndSaves()
    {
        var payment = RecurringPayment.Create(1, 1, 1, 100m, false, RecurringFrequency.Monthly, DateTime.UtcNow.AddDays(-1), null, DateTime.UtcNow.AddDays(-1)).Value;
        _recurringPaymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        var command = new CancelRecurringPaymentCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
