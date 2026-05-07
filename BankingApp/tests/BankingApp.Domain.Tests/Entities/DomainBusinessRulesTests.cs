namespace BankingApp.Domain.Tests.Entities;

using BankingApp.Domain.Entities;
using Enums;
using ErrorOr;

/// <summary>
///     Tests the business rules that are owned by domain entities.
/// </summary>
public class DomainBusinessRulesTests
{
    private static readonly DateTime _testStartDate = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _testCreatedAt = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RecurringPaymentCreate_WhenValid_SetsInitialSchedule()
    {
        ErrorOr<RecurringPayment> result = RecurringPayment.Create(
            userId: 42,
            billerId: 7,
            sourceAccountId: 3,
            amount: 200m,
            isPayInFull: false,
            frequency: RecurringFrequency.Monthly,
            startDate: _testStartDate,
            endDate: null,
            createdAt: _testCreatedAt);

        result.IsError.Should().BeFalse();
        result.Value.NextExecutionDate.Should().Be(_testStartDate.AddMonths(1));
        result.Value.CreatedAt.Should().Be(_testCreatedAt);
        result.Value.Status.Should().Be(RecurringPaymentStatus.Active);
    }

    [Fact]
    public void RecurringPaymentResume_WhenStatusIsNotPaused_ReturnsConflict()
    {
        var payment = new RecurringPayment
        {
            UserId = 42,
            Status = RecurringPaymentStatus.Active
        };

        ErrorOr<Success> result = payment.Resume(42);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void RecurringPaymentAdvanceAfterSuccessfulExecution_WhenPastEndDate_CancelsSchedule()
    {
        var payment = new RecurringPayment
        {
            Frequency = RecurringFrequency.Monthly,
            NextExecutionDate = _testStartDate,
            EndDate = _testStartDate.AddDays(20),
            Status = RecurringPaymentStatus.Active
        };

        payment.AdvanceAfterSuccessfulExecution();

        payment.Status.Should().Be(RecurringPaymentStatus.Cancelled);
    }

    [Fact]
    public void RecurringPaymentAdvanceAfterSuccessfulExecution_WhenWithinSchedule_AdvancesNextExecutionDate()
    {
        var payment = new RecurringPayment
        {
            Frequency = RecurringFrequency.Weekly,
            NextExecutionDate = _testStartDate,
            EndDate = _testStartDate.AddDays(30),
            Status = RecurringPaymentStatus.Active
        };

        payment.AdvanceAfterSuccessfulExecution();

        payment.Status.Should().Be(RecurringPaymentStatus.Active);
        payment.NextExecutionDate.Should().Be(_testStartDate.AddDays(7));
    }

    [Fact]
    public void RateAlertCreate_WhenCurrenciesMatch_ReturnsValidationError()
    {
        ErrorOr<RateAlert> result = RateAlert.Create(
            userId: 7,
            baseCurrency: "EUR",
            targetCurrency: "eur",
            targetRate: 4.9m,
            isBuyAlert: true,
            createdAt: _testCreatedAt);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void RateAlertShouldTrigger_WhenBuyAlertRoundsDownToTarget_ReturnsTrue()
    {
        var alert = new RateAlert
        {
            BaseCurrency = "EUR",
            TargetCurrency = "USD",
            TargetRate = 1.15m,
            IsBuyAlert = true
        };

        alert.ShouldTrigger(1.149m).Should().BeTrue();
    }

    [Fact]
    public void RateAlertShouldTrigger_WhenSellAlertRoundsUpToTarget_ReturnsTrue()
    {
        var alert = new RateAlert
        {
            BaseCurrency = "EUR",
            TargetCurrency = "USD",
            TargetRate = 1.15m,
            IsBuyAlert = false
        };

        alert.ShouldTrigger(1.151m).Should().BeTrue();
    }

    [Theory]
    [InlineData("RO49AAAA1B31007593840000", "Romanian Bank")]
    [InlineData("de89370400440532013000", "German Bank")]
    [InlineData("GB29NWBK60161331926819", "UK Bank")]
    [InlineData("ZZ12TEST123456789", "International Bank")]
    [InlineData("", "Unknown Bank")]
    [InlineData("R", "Unknown Bank")]
    public void TransferInferRecipientBankName_ReturnsExpectedBankName(string iban, string expectedBankName)
    {
        Transfer.InferRecipientBankName(iban).Should().Be(expectedBankName);
    }

    [Theory]
    [InlineData("RO49AAAA1B31007593840000", 100, "EUR", false)]
    [InlineData("", 100, "EUR", true)]
    [InlineData("RO49AAAA1B31007593840000", 0, "EUR", true)]
    [InlineData("RO49AAAA1B31007593840000", 100, "EURO", true)]
    public void TransferValidate_ReturnsExpectedValidationResult(
        string recipientIban,
        decimal amount,
        string currency,
        bool shouldFail)
    {
        ErrorOr<Success> result = Transfer.Validate(recipientIban, amount, currency);

        result.IsError.Should().Be(shouldFail);
    }

    [Theory]
    [InlineData(999.99, false)]
    [InlineData(1000.00, true)]
    [InlineData(1500.00, true)]
    public void TransferRequiresTwoFactorAuthentication_UsesThresholdRule(decimal amount, bool expected)
    {
        Transfer.RequiresTwoFactorAuthentication(amount).Should().Be(expected);
    }
}
