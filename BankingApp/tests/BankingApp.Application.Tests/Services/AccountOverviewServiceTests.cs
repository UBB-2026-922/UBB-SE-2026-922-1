namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.AccountOverview.Dtos;
using BankingApp.Application.Features.AccountOverview.Queries;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class GetDashboardQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = MockFactory.CreateUserRepository();
    private readonly Mock<IIdentityRepository> _identityRepo = MockFactory.CreateIdentityRepository();
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<ITransactionRepository> _transactionRepo = MockFactory.CreateTransactionRepository();

    private GetDashboardQueryHandler CreateHandler() => new(
        _userRepo.Object,
        _identityRepo.Object,
        _accountRepo.Object,
        _transactionRepo.Object,
        NullLogger<GetDashboardQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        var query = new GetDashboardQuery(1);

        ErrorOr<AccountOverviewDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserFound_ReturnsOverviewWithUserDetails()
    {
        var user = User.Register(Email.Create("ada@test.com").Value, "Ada Lovelace", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var query = new GetDashboardQuery(1);

        ErrorOr<AccountOverviewDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.CurrentUser!.FullName.Should().Be("Ada Lovelace");
        result.Value.CurrentUser.Email.Should().Be("ada@test.com");
    }

    [Fact]
    public async Task Handle_WhenUserFoundWithIdentity_ReturnsOverviewWith2FaStatus()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test User", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, null);
        identity.Enable2Fa(TwoFactorMethod.Email);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);
        var query = new GetDashboardQuery(1);

        ErrorOr<AccountOverviewDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.CurrentUser!.Is2FaEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserHasAccounts_ReturnsOverviewWithAccountData()
    {
        var user = User.Register(Email.Create("test@test.com").Value, "Test User", DateTime.UtcNow);
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var account = Account.Open(user.Id, iban, NodaMoney.Currency.FromCode("RON"), AccountType.Checking, "My Account", DateTime.UtcNow);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _accountRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Account>)new[] { account });
        var query = new GetDashboardQuery(1);

        ErrorOr<AccountOverviewDto> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().BeEmpty();
    }
}
