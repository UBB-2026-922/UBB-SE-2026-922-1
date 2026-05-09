namespace BankingApp.Application.Tests;

using BankingApp.Domain.Enums;
using ErrorOr;
using Currency = NodaMoney.Currency;

internal static class MockFactory
{
    internal static Mock<IUserRepository> CreateUserRepository()
    {
        var mock = new Mock<IUserRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        mock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IIdentityRepository> CreateIdentityRepository()
    {
        var mock = new Mock<IIdentityRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        mock.Setup(r => r.GetBySessionTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        mock.Setup(r => r.GetByResetTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);
        mock.Setup(r => r.AddAsync(It.IsAny<IdentityAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdateAsync(It.IsAny<IdentityAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IHashService> CreateHashService()
    {
        var mock = new Mock<IHashService>(MockBehavior.Loose);
        mock.Setup(s => s.GetHash(It.IsAny<string>()))
            .Returns((string input) => (ErrorOr<string>)$"hashed_{input}");
        mock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        return mock;
    }

    internal static Mock<IJsonWebTokenService> CreateJwtService()
    {
        var mock = new Mock<IJsonWebTokenService>(MockBehavior.Loose);
        mock.Setup(s => s.GenerateToken(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"jwt-token");
        return mock;
    }

    internal static Mock<IOtpService> CreateOtpService()
    {
        var mock = new Mock<IOtpService>(MockBehavior.Loose);
        mock.Setup(s => s.GenerateTotp(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"123456");
        mock.Setup(s => s.GenerateSmsOtp(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"123456");
        mock.Setup(s => s.VerifyTotp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        mock.Setup(s => s.VerifySmsOtp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ErrorOr<bool>)true);
        mock.Setup(s => s.InvalidateOtp(It.IsAny<int>()));
        return mock;
    }

    internal static Mock<IOtpAttemptTracker> CreateOtpAttemptTracker()
    {
        var mock = new Mock<IOtpAttemptTracker>(MockBehavior.Loose);
        mock.Setup(t => t.RecordFailure(It.IsAny<int>())).Returns(1);
        mock.Setup(t => t.Reset(It.IsAny<int>()));
        return mock;
    }

    internal static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>(MockBehavior.Loose);
        mock.Setup(s => s.SendOtpCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.SendLoginAlertAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var mock = new Mock<IUnitOfWork>(MockBehavior.Loose);
        mock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<ISystemClock> CreateSystemClock(DateTime? fixedTime = null)
    {
        var mock = new Mock<ISystemClock>(MockBehavior.Loose);
        mock.SetupGet(c => c.UtcNow).Returns(fixedTime ?? DateTime.UtcNow);
        return mock;
    }

    internal static Mock<IAccountRepository> CreateAccountRepository()
    {
        var mock = new Mock<IAccountRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Account>)Array.Empty<Account>());
        mock.Setup(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IBeneficiaryRepository> CreateBeneficiaryRepository()
    {
        var mock = new Mock<IBeneficiaryRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Beneficiary?)null);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Beneficiary>)Array.Empty<Beneficiary>());
        mock.Setup(r => r.AddAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdateAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.DeleteAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<ITransferRepository> CreateTransferRepository()
    {
        var mock = new Mock<ITransferRepository>(MockBehavior.Loose);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Transfer>)Array.Empty<Transfer>());
        mock.Setup(r => r.AddAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IBillPaymentRepository> CreateBillPaymentRepository()
    {
        var mock = new Mock<IBillPaymentRepository>(MockBehavior.Loose);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BillPayment>)Array.Empty<BillPayment>());
        mock.Setup(r => r.AddAsync(It.IsAny<BillPayment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IRecurringPaymentRepository> CreateRecurringPaymentRepository()
    {
        var mock = new Mock<IRecurringPaymentRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecurringPayment?)null);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RecurringPayment>)Array.Empty<RecurringPayment>());
        mock.Setup(r => r.ListDueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RecurringPayment>)Array.Empty<RecurringPayment>());
        mock.Setup(r => r.AddAsync(It.IsAny<RecurringPayment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdateAsync(It.IsAny<RecurringPayment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IBillerRepository> CreateBillerRepository()
    {
        var mock = new Mock<IBillerRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Biller?)null);
        mock.Setup(r => r.ListActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Biller>)Array.Empty<Biller>());
        return mock;
    }

    internal static Mock<ISavedBillerRepository> CreateSavedBillerRepository()
    {
        var mock = new Mock<ISavedBillerRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SavedBiller?)null);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<SavedBiller>)Array.Empty<SavedBiller>());
        mock.Setup(r => r.AddAsync(It.IsAny<SavedBiller>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.DeleteAsync(It.IsAny<SavedBiller>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IForexRepository> CreateForexRepository()
    {
        var mock = new Mock<IForexRepository>(MockBehavior.Loose);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ForexTransaction>)Array.Empty<ForexTransaction>());
        mock.Setup(r => r.AddAsync(It.IsAny<ForexTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IRateAlertRepository> CreateRateAlertRepository()
    {
        var mock = new Mock<IRateAlertRepository>(MockBehavior.Loose);
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RateAlert?)null);
        mock.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RateAlert>)Array.Empty<RateAlert>());
        mock.Setup(r => r.ListAllUntriggeredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RateAlert>)Array.Empty<RateAlert>());
        mock.Setup(r => r.AddAsync(It.IsAny<RateAlert>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdateAsync(It.IsAny<RateAlert>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(r => r.DeleteAsync(It.IsAny<RateAlert>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<IExchangeRateService> CreateExchangeRateService()
    {
        var mock = new Mock<IExchangeRateService>(MockBehavior.Loose);
        mock.Setup(s => s.GetRate(It.IsAny<Currency>(), It.IsAny<Currency>()))
            .Returns((ErrorOr<decimal>)1.15m);
        return mock;
    }

    internal static Mock<ILockedRateCache> CreateLockedRateCache()
    {
        var mock = new Mock<ILockedRateCache>(MockBehavior.Loose);
        mock.Setup(c => c.TryGet(It.IsAny<int>())).Returns((LockedRate?)null);
        mock.Setup(c => c.Store(It.IsAny<int>(), It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<decimal>(), It.IsAny<DateTime>()));
        mock.Setup(c => c.Remove(It.IsAny<int>()));
        return mock;
    }

    internal static Mock<ITransactionRepository> CreateTransactionRepository()
    {
        var mock = new Mock<ITransactionRepository>(MockBehavior.Loose);
        mock.Setup(r => r.ListByAccountIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Transaction>)Array.Empty<Transaction>());
        return mock;
    }

    internal static (User User, IdentityAccount Identity) CreateAuthenticatedPair(
        string email = "test@test.com",
        string passwordHash = "test-hash",
        bool is2FaEnabled = false,
        TwoFactorMethod? method = null)
    {
        var emailValue = Email.Create(email).Value;
        var user = User.Register(emailValue, "Test User", DateTime.UtcNow);
        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap(passwordHash));
        if (is2FaEnabled && method.HasValue)
        {
            identity.Enable2Fa(method.Value);
        }

        return (user, identity);
    }
}
