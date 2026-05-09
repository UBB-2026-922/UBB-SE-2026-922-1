namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Transfers.Commands;
using BankingApp.Application.Features.Transfers.Dtos;
using BankingApp.Application.Features.Transfers.Queries;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ExecuteTransferCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<ITransferRepository> _transferRepo = MockFactory.CreateTransferRepository();
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepo = MockFactory.CreateBeneficiaryRepository();
    private readonly Mock<IOtpService> _otpService = MockFactory.CreateOtpService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ExecuteTransferCommandHandler CreateHandler() => new(
        _accountRepo.Object,
        _transferRepo.Object,
        _beneficiaryRepo.Object,
        _otpService.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<ExecuteTransferCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenIbanInvalid_ReturnsError()
    {
        var command = new ExecuteTransferCommand(1, 10, "John Doe", "INVALID", 100m, "RON", null, null);

        ErrorOr<TransferResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsNotFoundError()
    {
        var command = new ExecuteTransferCommand(1, 10, "John Doe", "RO49AAAA1B31007593840000", 100m, "RON", null, null);

        ErrorOr<TransferResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ReturnsError()
    {
        var sourceIban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, sourceIban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new ExecuteTransferCommand(1, 10, "John Doe", "RO49AAAA1B31007593840001", 100m, "RON", null, null);

        ErrorOr<TransferResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCurrencyMismatch_ReturnsError()
    {
        var sourceIban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, sourceIban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, null, DateTime.UtcNow);
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var command = new ExecuteTransferCommand(1, 10, "John Doe", "RO49AAAA1B31007593840001", 100m, "EUR", null, null);

        ErrorOr<TransferResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }
}

public sealed class GetTransferAccountsQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();

    private GetTransferAccountsQueryHandler CreateHandler() => new(_accountRepo.Object);

    [Fact]
    public async Task Handle_WhenNoActiveAccounts_ReturnsEmptyList()
    {
        var query = new GetTransferAccountsQuery(1);

        ErrorOr<List<TransferAccountSelectionResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenActiveAccountExists_ReturnsMappedList()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(1, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, "My Account", DateTime.UtcNow);
        _accountRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Account>)new[] { account });
        var query = new GetTransferAccountsQuery(1);

        ErrorOr<List<TransferAccountSelectionResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].AccountName.Should().Be("My Account");
    }
}

public sealed class ValidateIbanQueryHandlerTests
{
    private ValidateIbanQueryHandler CreateHandler() => new();

    [Fact]
    public async Task Handle_WhenIbanIsValid_ReturnsValidResult()
    {
        var query = new ValidateIbanQuery("RO49AAAA1B31007593840000");

        ErrorOr<TransferIbanValidationResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ReturnsInvalidResult()
    {
        var query = new ValidateIbanQuery("NOT-AN-IBAN");

        ErrorOr<TransferIbanValidationResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeFalse();
    }
}

public sealed class GetTransferHistoryQueryHandlerTests
{
    private readonly Mock<ITransferRepository> _transferRepo = MockFactory.CreateTransferRepository();

    private GetTransferHistoryQueryHandler CreateHandler() => new(_transferRepo.Object);

    [Fact]
    public async Task Handle_WhenNoTransfers_ReturnsEmptyList()
    {
        var query = new GetTransferHistoryQuery(1);

        ErrorOr<List<TransferResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
