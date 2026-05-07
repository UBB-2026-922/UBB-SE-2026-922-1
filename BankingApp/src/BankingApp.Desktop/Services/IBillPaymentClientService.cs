using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.DTOs.Billers;
using BankingApp.Application.DTOs.RecurringPayments;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IBillPaymentClientService
{
    Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null);

    Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync();

    Task<ErrorOr<List<AccountDto>>> GetAccountsAsync();

    Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount);

    Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount);

    Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request);

    Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request);

    Task<ErrorOr<List<RecurringPaymentResponse>>> GetRecurringPaymentsAsync();

    Task<ErrorOr<RecurringPaymentResponse>> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request);

    Task<ErrorOr<Success>> PauseRecurringPaymentAsync(int paymentId);

    Task<ErrorOr<Success>> ResumeRecurringPaymentAsync(int paymentId);

    Task<ErrorOr<Success>> CancelRecurringPaymentAsync(int paymentId);
}
