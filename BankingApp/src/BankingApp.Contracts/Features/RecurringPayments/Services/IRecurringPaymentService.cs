namespace BankingApp.Contracts.Features.RecurringPayments.Services;

using Dtos;
using ErrorOr;

public interface IRecurringPaymentService
{
    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetAllAsync(CancellationToken ct = default);
    public Task<ErrorOr<RecurringPaymentResponse>> CreateAsync(CreateRecurringPaymentRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> PauseAsync(int id, CancellationToken ct = default);
    public Task<ErrorOr<Success>> ResumeAsync(int id, CancellationToken ct = default);
    public Task<ErrorOr<Success>> CancelAsync(int id, CancellationToken ct = default);
}
