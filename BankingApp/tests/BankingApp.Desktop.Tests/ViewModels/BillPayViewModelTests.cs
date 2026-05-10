namespace BankingApp.Desktop.Tests.ViewModels;

using System.Collections.Generic;
using Application.DTOs.Billers;
using Application.DTOs.BillPayments;
using Master;
using Services;
using BankingApp.Desktop.ViewModels;
using ErrorOr;

public class BillPayViewModelTests
{
    private readonly Mock<IBillPaymentClientService> _billPaymentClientService = new();
    private readonly Mock<IAppNavigationService> _navigationService = new();

    [Fact]
    public async Task LoadAsync_WhenSuccess_ShouldPopulateCollections()
    {
        // Arrange
        SetupSuccessfulLoad();
        BillPayViewModel vm = CreateViewModel();

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Billers.Should().HaveCount(2);
        vm.SavedBillers.Should().HaveCount(1);
        vm.Accounts.Should().HaveCount(1);
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_WhenBillersApiFails_ShouldKeepBillersEmpty()
    {
        // Arrange
        _billPaymentClientService
            .Setup(service => service.GetBillersAsync(null, null))
            .ReturnsAsync(Error.Failure(description: "Server error"));
        _billPaymentClientService
            .Setup(service => service.GetSavedBillersAsync())
            .ReturnsAsync(CreateMockSavedBillers());
        _billPaymentClientService
            .Setup(service => service.GetAccountsAsync())
            .ReturnsAsync(CreateMockAccounts());
        BillPayViewModel viewModel = CreateViewModel();

        // Act
        await viewModel.LoadAsync();

        // Assert
        viewModel.Billers.Should().BeEmpty();
        viewModel.SavedBillers.Should().HaveCount(1);
    }

    [Fact]
    public void SelectBillerCommand_WhenBillerDtoSelected_ShouldAdvanceToStep2()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };

        // Act
        vm.SelectBillerCommand.Execute(biller);

        // Assert
        vm.SelectedBiller.Should().Be(biller);
        vm.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void SelectBillerCommand_WhenSavedBillerDtoSelected_ShouldPrefillReferenceAndAdvanceToStep2()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var savedBiller = new SavedBillerDto
        {
            Id = 1,
            BillerId = 1,
            DefaultReference = "REF-123",
            Biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" },
        };

        // Act
        vm.SelectBillerCommand.Execute(savedBiller);

        // Assert
        vm.SelectedBiller.Should().NotBeNull();
        vm.SelectedBiller!.Id.Should().Be(1);
        vm.BillerReference.Should().Be("REF-123");
        vm.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep1AndNoBillerSelected_ShouldSetError()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();

        // Act
        vm.ExecuteNextStep();

        // Assert
        vm.ErrorMessage.Should().Contain("select a biller");
        vm.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep2AndNoReferenceProvided_ShouldSetError()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };
        vm.ExecuteSelectBiller(biller);

        // Act
        vm.ExecuteNextStep();

        // Assert
        vm.ErrorMessage.Should().Contain("biller reference");
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep2AndNoAccountSelected_ShouldSetError()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };
        vm.ExecuteSelectBiller(biller);
        vm.BillerReference = "REF-001";

        // Act
        vm.ExecuteNextStep();

        // Assert
        vm.ErrorMessage.Should().Contain("source account");
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep2AndZeroAmount_ShouldSetError()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };
        vm.ExecuteSelectBiller(biller);
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 0;

        // Act
        vm.ExecuteNextStep();

        // Assert
        vm.ErrorMessage.Should().Contain("valid amount");
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep2AndValidLowAmount_ShouldSkipTwoFactorAuthenticationAndGoToStep4()
    {
        _billPaymentClientService
            .Setup(service => service.GetFeeAsync(It.IsAny<decimal>()))
            .ReturnsAsync(new FeeResponse { Fee = 0.50m });
        _billPaymentClientService
            .Setup(service => service.GetRequires2FaAsync(It.IsAny<decimal>()))
            .ReturnsAsync(new RequiresTwoFaResponse { Required = false });

        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };
        vm.ExecuteSelectBiller(biller);
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 50m;

        vm.ExecuteNextStep();

        vm.CurrentStep.Should().Be(4);
        vm.Fee.Should().Be(0.50m);
        vm.Requires2Fa.Should().BeFalse();
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep2AndHighAmount_ShouldGoToTwoFactorAuthenticationStep3()
    {
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.GetFeeAsync(It.IsAny<decimal>()))
            .ReturnsAsync(new FeeResponse { Fee = 1.00m });
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.GetRequires2FaAsync(It.IsAny<decimal>()))
            .ReturnsAsync(new RequiresTwoFaResponse { Required = true });

        BillPayViewModel vm = CreateViewModel();
        var biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" };
        vm.ExecuteSelectBiller(biller);
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 1500m;

        vm.ExecuteNextStep();

        vm.CurrentStep.Should().Be(3);
        vm.Requires2Fa.Should().BeTrue();
    }

    [Fact]
    public void ExecuteNextStep_WhenOnStep3AndNotConfirmed_ShouldSetError()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        vm.CurrentStep = 3;

        // Act
        vm.ExecuteNextStep();

        // Assert
        vm.ErrorMessage.Should().Contain("2FA");
    }

    [Fact]
    public void ExecuteBack_WhenFromReviewWithoutTwoFactorAuthentication_ShouldGoToStep2()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        vm.CurrentStep = 4;
        vm.Requires2Fa = false;

        // Act
        vm.ExecuteBack();

        // Assert
        vm.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void ExecuteBack_WhenFromReviewWithTwoFactorAuthentication_ShouldGoToStep3()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        vm.CurrentStep = 4;
        vm.Requires2Fa = true;

        // Act
        vm.ExecuteBack();

        // Assert
        vm.CurrentStep.Should().Be(3);
    }

    [Fact]
    public void ExecuteBack_WhenFromStep1_ShouldStayAtStep1()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();

        // Act
        vm.ExecuteBack();

        // Assert
        vm.CurrentStep.Should().Be(1);
    }

    [Fact]
    public async Task ExecutePayBillAsync_WhenSuccess_ShouldSetReceiptAndGoToStep5()
    {
        _billPaymentClientService
            .Setup(service => service.PayBillAsync(It.IsAny<BillPayRequest>()))
            .ReturnsAsync(new BillPayResponse
            {
                Id = 1,
                ReceiptNumber = "RCP-20260504-ABC123",
                Fee = 0.50m,
                Amount = 200m,
                Status = "Completed",
            });

        BillPayViewModel vm = CreateViewModel();
        vm.ExecuteSelectBiller(new BillerDto { Id = 1, Name = "Test", Category = "Utilities" });
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 200m;

        await vm.ExecutePayBillAsync();

        vm.ReceiptNumber.Should().Be("RCP-20260504-ABC123");
        vm.CurrentStep.Should().Be(5);
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutePayBillAsync_WhenApiFails_ShouldSetErrorMessage()
    {
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.PayBillAsync(It.IsAny<BillPayRequest>()))
            .ReturnsAsync(Error.Failure(description: "Insufficient funds"));

        BillPayViewModel vm = CreateViewModel();
        vm.ExecuteSelectBiller(new BillerDto { Id = 1, Name = "Test", Category = "Utilities" });
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 200m;

        await vm.ExecutePayBillAsync();

        vm.ErrorMessage.Should().Contain("Payment failed");
        vm.CurrentStep.Should().NotBe(5);
    }

    [Fact]
    public async Task ExecutePayBillAsync_WhenNoBiller_ShouldSetError()
    {
        BillPayViewModel vm = CreateViewModel();

        await vm.ExecutePayBillAsync();

        vm.ErrorMessage.Should().Contain("select a biller");
    }

    [Fact]
    public async Task ExecutePayBillAsync_WhenWithSaveBiller_ShouldCallSaveEndpoint()
    {
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.PayBillAsync(It.IsAny<BillPayRequest>()))
            .ReturnsAsync(new BillPayResponse
            {
                Id = 1,
                ReceiptNumber = "RCP-TEST",
                Fee = 0.50m,
                Amount = 200m,
                Status = "Completed",
            });
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.SaveBillerAsync(It.IsAny<SaveBillerRequest>()))
            .ReturnsAsync(new SavedBillerDto
            {
                Id = 99,
                BillerId = 1,
                Nickname = "Test",
                Biller = new BillerDto { Id = 1, Name = "Test", Category = "Utilities" },
            });

        BillPayViewModel vm = CreateViewModel();
        vm.ExecuteSelectBiller(new BillerDto { Id = 1, Name = "Test", Category = "Utilities" });
        vm.BillerReference = "REF-001";
        vm.SelectedAccount = new AccountDto { Id = 1, AccountName = "Test" };
        vm.Amount = 200m;
        vm.ShouldSaveBiller = true;

        await vm.ExecutePayBillAsync();

        _billPaymentClientService.Verify(
            service => service.SaveBillerAsync(It.IsAny<SaveBillerRequest>()),
            Times.Once);
        vm.SavedBillers.Should().HaveCount(1);
    }

    [Fact]
    public void ResetForm_WhenCalled_ShouldClearAllState()
    {
        BillPayViewModel vm = CreateViewModel();
        vm.ExecuteSelectBiller(new BillerDto { Id = 1, Name = "Test", Category = "Utilities" });
        vm.BillerReference = "REF-001";
        vm.Amount = 500m;
        vm.Fee = 1.0m;
        vm.ReceiptNumber = "RCP-TEST";
        vm.Is2FaConfirmed = true;

        vm.ResetForm();

        vm.CurrentStep.Should().Be(1);
        vm.SelectedBiller.Should().BeNull();
        vm.BillerReference.Should().BeEmpty();
        vm.Amount.Should().Be(0);
        vm.Fee.Should().Be(0);
        vm.ReceiptNumber.Should().BeEmpty();
        vm.ErrorMessage.Should().BeEmpty();
        vm.Is2FaConfirmed.Should().BeFalse();
        vm.ShouldSaveBiller.Should().BeFalse();
    }

    [Fact]
    public void SelectedBillerName_WhenBillerSet_ShouldReturnName()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();

        // Act
        vm.ExecuteSelectBiller(new BillerDto { Id = 1, Name = "Enel Energie", Category = "Utilities" });

        // Assert
        vm.SelectedBillerName.Should().Be("Enel Energie");
    }

    [Fact]
    public void SelectedBillerName_WhenNoBiller_ShouldReturnFallback()
    {
        // Arrange & Act
        BillPayViewModel vm = CreateViewModel();

        // Assert
        vm.SelectedBillerName.Should().Be("No biller selected");
    }

    [Fact]
    public void Total_WhenCalled_ShouldReturnAmountPlusFee()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        vm.Amount = 100m;
        vm.Fee = 0.50m;

        // Assert
        vm.Total.Should().Be(100.50m);
    }

    [Fact]
    public void ReviewAmountText_WhenPositive_ShouldShowFormattedAmount()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        vm.Amount = 250.75m;

        // Assert
        vm.ReviewAmountText.Should().Be("250.75 RON");
    }

    [Fact]
    public void ReviewAmountText_WhenZero_ShouldShowPlaceholder()
    {
        // Arrange & Act
        BillPayViewModel vm = CreateViewModel();

        // Assert
        vm.ReviewAmountText.Should().Be("No amount entered");
    }

    [Fact]
    public void Amount_WhenPropertyChanged_ShouldFireMultipleNotifications()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var changedProperties = new List<string>();
        vm.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName!);

        // Act
        vm.Amount = 100m;

        // Assert
        changedProperties.Should().Contain("Amount");
        changedProperties.Should().Contain("ReviewAmountText");
        changedProperties.Should().Contain("Total");
        changedProperties.Should().Contain("TotalText");
    }

    [Fact]
    public void ErrorMessage_WhenPropertyChanged_ShouldFireVisibilityNotification()
    {
        // Arrange
        BillPayViewModel vm = CreateViewModel();
        var changedProperties = new List<string>();
        vm.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName!);

        // Act
        vm.ErrorMessage = "Something went wrong";

        // Assert
        changedProperties.Should().Contain("ErrorMessage");
        changedProperties.Should().Contain("ErrorMessageVisibility");
    }

    private static List<BillerDto> CreateMockBillers()
    {
        return
        [
            new BillerDto { Id = 1, Name = "Enel Energie", Category = "Utilities", IsActive = true },
            new BillerDto { Id = 2, Name = "Digi RCS-RDS", Category = "Internet", IsActive = true },
        ];
    }

    private static List<SavedBillerDto> CreateMockSavedBillers()
    {
        return
        [
            new SavedBillerDto
            {
                Id = 1,
                UserId = 1,
                BillerId = 1,
                Nickname = "Enel Home",
                DefaultReference = "EL-001",
                Biller = new BillerDto { Id = 1, Name = "Enel Energie", Category = "Utilities", IsActive = true },
            },
        ];
    }

    private static List<AccountDto> CreateMockAccounts()
    {
        return
        [
            new AccountDto
            {
                Id = 1,
                Iban = "RO49AAAA1B31007593840000",
                Currency = "RON",
                Balance = 8500m,
                AccountName = "RON Account",
            },
        ];
    }

    private BillPayViewModel CreateViewModel()
    {
        return new BillPayViewModel(_billPaymentClientService.Object, _navigationService.Object);
    }

    private void SetupSuccessfulLoad()
    {
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.GetBillersAsync(null, null))
            .ReturnsAsync(CreateMockBillers());
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.GetSavedBillersAsync())
            .ReturnsAsync(CreateMockSavedBillers());
        _billPaymentClientService
            .Setup(billPaymentClientService => billPaymentClientService.GetAccountsAsync())
            .ReturnsAsync(CreateMockAccounts());
    }
}
