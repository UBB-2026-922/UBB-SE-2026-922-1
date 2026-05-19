namespace BankingApp.Desktop.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Desktop.ViewModels;
using BankingApp.Domain.Enums;
using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using ErrorOr;
using FluentAssertions;
using Microsoft.UI.Xaml;
using Moq;
using Xunit;

public class RecurringPaymentViewModelTests
{
    private readonly Mock<IBillPaymentService> _billPaymentClientService;
    private readonly Mock<IBillerService> _billerClientService;
    private readonly Mock<IRecurringPaymentService> _recurringPaymentClientService;
    private readonly RecurringPaymentViewModel _viewModel;

    public RecurringPaymentViewModelTests()
    {
        _billPaymentClientService = new Mock<IBillPaymentService>(MockBehavior.Strict);
        _billerClientService = new Mock<IBillerService>(MockBehavior.Strict);
        _recurringPaymentClientService = new Mock<IRecurringPaymentService>(MockBehavior.Strict);
        _viewModel = new RecurringPaymentViewModel(
            _billPaymentClientService.Object,
            _billerClientService.Object,
            _recurringPaymentClientService.Object);
    }

    [Fact]
    public async Task LoadAsync_WhenResponsesAreValid_PopulatesCollections()
    {
        // Arrange
        var accounts = new List<AccountDto>
        {
            new AccountDto { Id = 1, AccountName = "Checking", Balance = 1000m },
        };
        var payments = new List<RecurringPaymentResponse>
        {
            new RecurringPaymentResponse { Id = 1, Amount = 50m },
        };
        var billers = new List<BillerDto>
        {
            new() { Id = 1, Name = "Electric Co" },
        };

        _billPaymentClientService.Setup(service => service.GetAccountsAsync())
            .ReturnsAsync(accounts);
        _recurringPaymentClientService.Setup(service => service.GetAllAsync())
            .ReturnsAsync(payments);
        _billerClientService.Setup(service => service.GetBillersAsync(null, null, default))
            .ReturnsAsync(billers);

        // Act
        await _viewModel.LoadAsync();

        // Assert
        _viewModel.Accounts.Should().BeEquivalentTo(accounts);
        _viewModel.Payments.Should().BeEquivalentTo(payments);
        _viewModel.Billers.Should().BeEquivalentTo(billers);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WhenInputsAreValid_CreatesPaymentAndClearsForm()
    {
        // Arrange
        var biller = new BillerDto { Id = 1, Name = "Water Co" };
        var account = new AccountDto { Id = 1, AccountName = "Checking" };
        DateTime startDate = DateTime.Today;
        const decimal amount = 100m;

        _viewModel.SelectedBiller = biller;
        _viewModel.SelectedAccount = account;
        _viewModel.Amount = amount;
        _viewModel.Frequency = RecurringFrequency.Monthly;
        _viewModel.StartDate = startDate;

        var expectedResult = new RecurringPaymentResponse
        {
            Id = 123,
            BillerId = biller.Id,
            SourceAccountId = account.Id,
            Amount = amount,
            Frequency = RecurringFrequency.Monthly,
            StartDate = startDate,
        };

        _recurringPaymentClientService.Setup(billPaymentClientService => billPaymentClientService.CreateAsync(
                It.Is<CreateRecurringPaymentRequest>(dto =>
                    dto.BillerId == biller.Id &&
                    dto.SourceAccountId == account.Id &&
                    dto.Amount == amount &&
                    dto.Frequency == RecurringFrequency.Monthly &&
                    dto.StartDate == startDate)))
            .ReturnsAsync(expectedResult);

        // Act
        await _viewModel.CreateAsync();

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments.Should().Contain(expectedResult);
        _viewModel.SelectedBiller.Should().BeNull();
        _viewModel.SelectedAccount.Should().BeNull();
        _viewModel.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task PauseAsync_WhenPaymentIsSelected_PausesPayment()
    {
        // Arrange
        var payment = new RecurringPaymentResponse { Id = 1, Status = RecurringPaymentStatus.Active };
        _viewModel.Payments.Add(payment);

        _recurringPaymentClientService.Setup(billPaymentClientService => billPaymentClientService.PauseAsync(payment.Id, default))
            .ReturnsAsync(Result.Success);

        // Act
        await _viewModel.PauseAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Paused);
    }

    [Fact]
    public async Task ResumeAsync_WhenPaymentIsSelected_ResumesPayment()
    {
        // Arrange
        var payment = new RecurringPaymentResponse { Id = 1, Status = RecurringPaymentStatus.Paused };
        _viewModel.Payments.Add(payment);

        _recurringPaymentClientService.Setup(billPaymentClientService => billPaymentClientService.ResumeAsync(payment.Id, default))
            .ReturnsAsync(Result.Success);

        // Act
        await _viewModel.ResumeAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Active);
    }

    [Fact]
    public async Task CancelAsync_WhenPaymentIsSelected_CancelsPayment()
    {
        // Arrange
        var payment = new RecurringPaymentResponse { Id = 1, Status = RecurringPaymentStatus.Active };
        _viewModel.Payments.Add(payment);

        _recurringPaymentClientService.Setup(billPaymentClientService => billPaymentClientService.CancelAsync(payment.Id, default))
            .ReturnsAsync(Result.Success);

        // Act
        await _viewModel.CancelAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Cancelled);
    }

    [Fact]
    public void ErrorMessage_WhenSet_ShouldExposeVisibleErrorState()
    {
        // Act
        _viewModel.ErrorMessage = "Failure";

        // Assert
        _viewModel.ErrorMessageVisibility.Should().Be(Visibility.Visible);
    }
}
