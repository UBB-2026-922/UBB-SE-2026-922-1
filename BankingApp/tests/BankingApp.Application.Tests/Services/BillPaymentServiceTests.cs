namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.BillPayments.Commands;
using BankingApp.Application.Features.BillPayments.Dtos;
using BankingApp.Application.Features.BillPayments.Queries;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ProcessBillPaymentCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<IBillPaymentRepository> _billPaymentRepo = MockFactory.CreateBillPaymentRepository();
    private readonly Mock<IBillerRepository> _billerRepo = MockFactory.CreateBillerRepository();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ProcessBillPaymentCommandHandler CreateHandler() => new(
        _accountRepo.Object,
        _billPaymentRepo.Object,
        _billerRepo.Object,
        _otpService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<ProcessBillPaymentCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsNotFoundError()
    {
        var command = new ProcessBillPaymentCommand(1, 10, 1, "REF123", 100m, null);

        ErrorOr<BillPayResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBillerNotFound_ReturnsError()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new ProcessBillPaymentCommand(1, 10, 99, "REF123", 100m, null);

        ErrorOr<BillPayResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ReturnsError()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        var biller = new Biller { Id = 1, Name = "Test Biller", Category = BillerCategory.Utilities };
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        var command = new ProcessBillPaymentCommand(1, 10, 1, "REF123", 1000m, null);

        ErrorOr<BillPayResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }
}

public sealed class GetBillPaymentHistoryQueryHandlerTests
{
    private readonly Mock<IBillPaymentRepository> _billPaymentRepo = MockFactory.CreateBillPaymentRepository();

    private GetBillPaymentHistoryQueryHandler CreateHandler() => new(_billPaymentRepo.Object);

    [Fact]
    public async Task Handle_WhenNoBillPayments_ReturnsEmptyList()
    {
        var query = new GetBillPaymentHistoryQuery(1);

        ErrorOr<List<BillPayResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
