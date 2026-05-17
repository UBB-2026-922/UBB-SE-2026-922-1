namespace BankingApp.Web.Services;

using BankingApp.Web.ViewModels;

public interface IBeneficiaryService
{
    public Task<IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow>> GetAllAsync(string token);
    public Task<EditBeneficiaryViewModel?> GetByIdAsync(int id, string token);
    public Task<bool> CreateAsync(CreateBeneficiaryViewModel model, string token);
    public Task<bool> UpdateAsync(EditBeneficiaryViewModel model, string token);
    public Task<bool> DeleteAsync(int id, string token);
}