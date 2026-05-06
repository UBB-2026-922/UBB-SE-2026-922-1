using System.Collections.Generic;
using BankingApp.Desktop.Models;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;

namespace BankingApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for the <see cref="TransferViewModel" />.
/// </summary>
public class TransferViewModelTests
{
    private const int AccountSelectionStep = 1;
    private const int RecipientDetailsStep = 2;
    private const int AmountDetailsStep = 3;
    private const int TwoFactorAuthenticationStep = 4;
    private const int ReviewAndConfirmationStep = 5;
    private const int TransferCompletedStep = 6;
    private const int TransferErrorStep = 7;

    private readonly Mock<IApiClient> _apiClient;
    private readonly TransferViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferViewModelTests" /> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public TransferViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Loose);
        _viewModel = new TransferViewModel(_apiClient.Object);
    }

    /// <summary>
    ///     In LoadAccountsAsync, when the API returns accounts, the collection
    ///     should be populated and the first account selected.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAccountsAsync_WhenApiReturnsAccounts_PopulatesAccountsAndSelectsFirst()
    {
        // Arrange
        var accounts = new List<TransferAccountDto>
        {
            new TransferAccountDto { Id = 1, AccountName = "Main", Currency = "EUR", Balance = 1000m },
            new TransferAccountDto { Id = 2, AccountName = "Savings", Currency = "USD", Balance = 500m }
        };

        _apiClient
            .Setup(client => client.GetAsync<List<TransferAccountDto>>(
                ApiEndpoints.TransferAccounts, default))
            .ReturnsAsync(accounts);

        // Act
        await _viewModel.LoadAccountsAsync();

        // Assert
        _viewModel.Accounts.Should().HaveCount(2);
        _viewModel.SelectedAccount.Should().NotBeNull();
        _viewModel.SelectedAccount!.AccountName.Should().Be("Main");
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     In LoadAccountsAsync, when the API returns an error, the error message
    ///     should be set and the accounts collection should remain empty.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAccountsAsync_WhenApiFails_SetsErrorMessage()
    {
        // Arrange
        _apiClient
            .Setup(client => client.GetAsync<List<TransferAccountDto>>(
                ApiEndpoints.TransferAccounts, default))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadAccountsAsync();

        // Assert
        _viewModel.Accounts.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.AccountLoadFailed);
    }

    /// <summary>
    ///     ExecuteNextStep from step 1 should advance to step 2.
    /// </summary>
    [Fact]
    public void ExecuteNextStep_FromStep1_AdvancesToStep2()
    {
        // Arrange
        _viewModel.CurrentStep.Should().Be(AccountSelectionStep);

        // Act
        _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(RecipientDetailsStep);
    }

    /// <summary>
    ///     ExecuteNextStep at the recipient step when IBAN is invalid should set
    ///     the error step and the invalid IBAN error message.
    /// </summary>
    [Fact]
    public void ExecuteNextStep_AtRecipientStep_WhenIBANInvalid_SetsErrorStep()
    {
        // Arrange
        _viewModel.CurrentStep = RecipientDetailsStep;
        _viewModel.IsIbanValid = false;

        // Act
        _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.InvalidIban);
    }

    /// <summary>
    ///     ExecuteNextStep at the amount step when amount is zero should set
    ///     the error step and the amount error message.
    /// </summary>
    [Fact]
    public void ExecuteNextStep_AtAmountStep_WhenAmountZero_SetsErrorStep()
    {
        // Arrange
        _viewModel.CurrentStep = AmountDetailsStep;
        _viewModel.Amount = 0m;

        // Act
        _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.AmountMustBePositive);
    }

    /// <summary>
    ///     ExecuteNextStep at the amount step when 2FA is not required should skip
    ///     to the review step.
    /// </summary>
    [Fact]
    public void ExecuteNextStep_AtAmountStep_When2FANotRequired_SkipsToReview()
    {
        // Arrange
        _viewModel.CurrentStep = AmountDetailsStep;
        _viewModel.Amount = 100m;

        // Act
        _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(ReviewAndConfirmationStep);
        _viewModel.Requires2Fa.Should().BeFalse();
    }

    /// <summary>
    ///     ExecuteNextStep at the amount step when 2FA is required should go
    ///     to the 2FA step.
    /// </summary>
    [Fact]
    public void ExecuteNextStep_AtAmountStep_When2FARequired_GoesToTwoFAStep()
    {
        // Arrange
        _viewModel.CurrentStep = AmountDetailsStep;
        _viewModel.Amount = 1500m;

        // Act
        _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TwoFactorAuthenticationStep);
        _viewModel.Requires2Fa.Should().BeTrue();
    }

    /// <summary>
    ///     ExecuteTransferAsync when the API succeeds should set the completed step
    ///     and populate the transaction reference.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenApiSucceeds_SetsCompletedStep()
    {
        // Arrange
        const string expectedRef = "TXN-20260504-0001";
        _viewModel.SelectedAccount = new TransferAccountDto { Id = 1, AccountName = "Main", Currency = "EUR" };
        _viewModel.RecipientName = "Jane Doe";
        _viewModel.Amount = 250m;
        _viewModel.Currency = "EUR";

        _apiClient
            .Setup(client => client.PostAsync<TransferRequestDto, TransferResultDto>(
                ApiEndpoints.TransferExecute,
                It.IsAny<object?>()))
            .ReturnsAsync(new TransferResultDto { TransactionRef = expectedRef });

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferCompletedStep);
        _viewModel.TransactionRef.Should().Be(expectedRef);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     ExecuteTransferAsync when the API returns an error should set the error step
    ///     and populate the error message.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenApiFails_SetsErrorStep()
    {
        // Arrange
        const string errorDescription = "Insufficient funds.";
        _viewModel.SelectedAccount = new TransferAccountDto { Id = 1, AccountName = "Main", Currency = "EUR" };

        _apiClient
            .Setup(client => client.PostAsync<TransferRequestDto, TransferResultDto>(
                ApiEndpoints.TransferExecute,
                It.IsAny<object?>()))
            .ReturnsAsync(Error.Failure(description: errorDescription));

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(errorDescription);
    }

    /// <summary>
    ///     ExecuteSendAgain should reset all form fields and return to step 1.
    /// </summary>
    [Fact]
    public async Task ExecuteSendAgain_ResetsAllFieldsAndReturnsToStep1()
    {
        // Arrange - set up accounts and dirty state
        var accounts = new List<TransferAccountDto>
        {
            new TransferAccountDto { Id = 1, AccountName = "Main", Currency = "EUR" }
        };

        _apiClient
            .Setup(client => client.GetAsync<List<TransferAccountDto>>(
                ApiEndpoints.TransferAccounts, default))
            .ReturnsAsync(accounts);

        await _viewModel.LoadAccountsAsync();
        _viewModel.RecipientName = "Jane Doe";
        _viewModel.RecipientIban = "RO49AAAA1B31007593840000";
        _viewModel.AmountText = "500";
        _viewModel.Currency = "USD";
        _viewModel.Is2FaConfirmed = true;
        _viewModel.TransactionRef = "TXN-123";
        _viewModel.ErrorMessage = "Some error";
        _viewModel.CurrentStep = TransferCompletedStep;

        // Act
        _viewModel.ExecuteSendAgain();

        // Assert
        _viewModel.CurrentStep.Should().Be(AccountSelectionStep);
        _viewModel.RecipientName.Should().BeEmpty();
        _viewModel.RecipientIban.Should().BeEmpty();
        _viewModel.AmountText.Should().BeEmpty();
        _viewModel.Currency.Should().Be("EUR");
        _viewModel.Is2FaConfirmed.Should().BeFalse();
        _viewModel.TransactionRef.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.SelectedAccount.Should().NotBeNull();
    }

    /// <summary>
    ///     ExecuteTransferAsync when no account is selected should set the error step.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenNoAccountSelected_SetsErrorStep()
    {
        // Arrange
        _viewModel.SelectedAccount = null;

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.NoAccountSelected);
    }
}
