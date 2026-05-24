namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using BankingApp.Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using ErrorOr;

public partial class BillPayViewModel
{
    /// <summary>Loads billers, saved billers, and source accounts for the workflow.</summary>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ResetFormStateOnly();

            await LoadBillersAsync();
            await LoadSavedBillersAsync();
            await LoadAccountsAsync();
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    internal void ExecuteSearch()
    {
        _ = SearchBillersAsync();
    }

    internal void ExecuteSelectBiller(object? parameter)
    {
        ErrorMessage = string.Empty;

        switch (parameter)
        {
            case BillerDto biller:
                SelectedBiller = biller;
                CurrentStep = PaymentDetailsStep;
                return;
            case SavedBillerDto savedBiller:
                SelectSavedBiller(savedBiller);
                return;
        }
    }

    internal void ExecuteNextStep()
    {
        ErrorMessage = string.Empty;

        switch (CurrentStep)
        {
            case SelectBillerStep:
                MoveFromBillerSelection();
                break;
            case PaymentDetailsStep:
                MoveFromPaymentDetails();
                break;
            case TwoFactorAuthenticationStep:
                MoveFromTwoFactorStep();
                break;
        }
    }

    internal void ExecuteBack()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep <= SelectBillerStep)
        {
            return;
        }

        CurrentStep = CurrentStep switch
        {
            ReviewAndConfirmStep when Requires2Fa => TwoFactorAuthenticationStep,
            ReviewAndConfirmStep => PaymentDetailsStep,
            _ => CurrentStep - 1,
        };
    }

    internal async Task ExecutePayBillAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!ValidatePaymentRequest())
            {
                return;
            }

            BillPayRequest request = BuildBillPaymentRequest();
            ErrorOr<BillPayResponse> payResult = await _billPaymentService.PayBillAsync(request);

            if (payResult.IsError)
            {
                ErrorMessage = $"Payment failed: {payResult.FirstError.Description}";
                return;
            }

            await SaveSelectedBillerIfNeededAsync();
            ApplyPaymentSuccess(payResult.Value);
        }
        catch (Exception paymentException)
        {
            ErrorMessage = $"Payment failed: {paymentException.Message}";
        }
    }

    internal void ResetForm()
    {
        ErrorMessage = string.Empty;
        ResetFormStateOnly();
    }

    private async Task LoadBillersAsync()
    {
        ErrorOr<List<BillerDto>> result = await _billerService.GetBillersAsync();
        if (!result.IsError)
        {
            Billers = new ObservableCollection<BillerDto>(result.Value);
        }
    }

    private async Task LoadSavedBillersAsync()
    {
        ErrorOr<List<SavedBillerDto>> result = await _billerService.GetSavedBillersAsync();
        if (!result.IsError)
        {
            SavedBillers = new ObservableCollection<SavedBillerDto>(result.Value);
        }
    }

    private async Task LoadAccountsAsync()
    {
        ErrorOr<List<AccountDto>> result = await _billPaymentService.GetAccountsAsync();
        if (!result.IsError)
        {
            Accounts = new ObservableCollection<AccountDto>(result.Value);
        }
    }

    private async Task SearchBillersAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ErrorOr<List<BillerDto>> result = await _billerService.GetBillersAsync(SearchQuery, SelectedCategory);

            if (!result.IsError)
            {
                Billers = new ObservableCollection<BillerDto>(result.Value);
            }
        }
        catch (Exception searchException)
        {
            ErrorMessage = $"Search failed: {searchException.Message}";
        }
    }

    private void SelectSavedBiller(SavedBillerDto savedBiller)
    {
        SelectedBiller = savedBiller.ToBiller();
        if (!string.IsNullOrWhiteSpace(savedBiller.DefaultReference))
        {
            BillerReference = savedBiller.DefaultReference!;
        }

        CurrentStep = PaymentDetailsStep;
    }

    private void MoveFromBillerSelection()
    {
        if (SelectedBiller == null)
        {
            ErrorMessage = "Please select a biller.";
            return;
        }

        CurrentStep = PaymentDetailsStep;
    }

    private void MoveFromPaymentDetails()
    {
        if (!ValidatePaymentRequest())
        {
            return;
        }

        SetFeeAndTwoFactorRequirement();
        CurrentStep = Requires2Fa ? TwoFactorAuthenticationStep : ReviewAndConfirmStep;
    }

    private void MoveFromTwoFactorStep()
    {
        if (!Is2FaConfirmed)
        {
            ErrorMessage = "You must confirm the 2FA step.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TwoFaToken))
        {
            TwoFaToken = GenerateTwoFaToken();
        }

        CurrentStep = ReviewAndConfirmStep;
    }

    private bool ValidatePaymentRequest()
    {
        if (SelectedBiller == null)
        {
            ErrorMessage = "Please select a biller.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(BillerReference))
        {
            ErrorMessage = "Please enter a biller reference.";
            return false;
        }

        if (SelectedAccount == null)
        {
            ErrorMessage = "Please select a source account.";
            return false;
        }

        if (Amount <= MinimumAmount)
        {
            ErrorMessage = "Please enter a valid amount.";
            return false;
        }

        return true;
    }

    private void SetFeeAndTwoFactorRequirement()
    {
        ErrorOr<FeeResponse> feeResult = _billPaymentService.GetFeeAsync(Amount).GetAwaiter().GetResult();
        Fee = !feeResult.IsError ? feeResult.Value.Fee : NoFee;

        ErrorOr<RequiresTwoFaResponse> twoFaResult =
            _billPaymentService.GetRequires2FaAsync(Amount).GetAwaiter().GetResult();
        Requires2Fa = twoFaResult is { IsError: false, Value.Required: true };
    }

    private BillPayRequest BuildBillPaymentRequest() =>
        new()
        {
            SourceAccountId = SelectedAccount!.Id,
            BillerId = SelectedBiller!.Id,
            BillerReference = BillerReference,
            Amount = Amount,
            IsPayInFull = false,
            TwoFaToken = Requires2Fa ? TwoFaToken : null,
        };

    private async Task SaveSelectedBillerIfNeededAsync()
    {
        if (!ShouldSaveBiller || SelectedBiller == null)
        {
            return;
        }

        bool alreadySaved = SavedBillers.Any(savedBiller =>
            savedBiller.BillerId == SelectedBiller.Id &&
            string.Equals(savedBiller.DefaultReference, BillerReference, StringComparison.OrdinalIgnoreCase));

        if (alreadySaved)
        {
            return;
        }

        SaveBillerRequest saveRequest = new()
        {
            BillerId = SelectedBiller.Id,
            Nickname = SelectedBiller.Name,
            DefaultReference = BillerReference,
        };

        ErrorOr<SavedBillerDto> saveResult = await _billerService.SaveBillerAsync(saveRequest);
        if (!saveResult.IsError)
        {
            SavedBillers.Add(saveResult.Value);
        }
    }

    private void ApplyPaymentSuccess(BillPayResponse response)
    {
        ReceiptNumber = response.ReceiptNumber;
        Fee = response.Fee;
        CurrentStep = PaymentResultStep;
    }

    private static string GenerateTwoFaToken()
    {
        Random random = new();
        return random.Next(MinimumTwoFactorToken, MaximumTwoFactorTokenExclusive)
            .ToString(CultureInfo.InvariantCulture);
    }

    private void ResetFormStateOnly()
    {
        CurrentStep = SelectBillerStep;
        SelectedBiller = null;
        SearchQuery = string.Empty;
        SelectedCategory = null;
        BillerReference = string.Empty;
        Amount = MinimumAmount;
        Fee = NoFee;
        ReceiptNumber = string.Empty;
        SelectedAccount = null;
        IsPayInFull = false;
        ShouldSaveBiller = false;
        Requires2Fa = false;
        Is2FaConfirmed = false;
        TwoFaToken = string.Empty;
    }

    private void ApplySavedDefaultsForSelectedBiller()
    {
        if (SelectedBiller == null || SavedBillers.Count == MinimumBillers)
        {
            return;
        }

        SavedBillerDto? matchingSaved = SavedBillers.FirstOrDefault(s => s.BillerId == SelectedBiller.Id);
        if (matchingSaved != null &&
            string.IsNullOrWhiteSpace(BillerReference) &&
            !string.IsNullOrWhiteSpace(matchingSaved.DefaultReference))
        {
            BillerReference = matchingSaved.DefaultReference!;
        }
    }
}
