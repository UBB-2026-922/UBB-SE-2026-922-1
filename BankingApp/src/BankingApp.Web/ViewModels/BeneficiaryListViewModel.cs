namespace BankingApp.Web.ViewModels;

public sealed class BeneficiaryListViewModel
{
    public IReadOnlyList<BeneficiaryRow> Beneficiaries { get; init; } = [];

    public sealed class BeneficiaryRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Iban { get; init; } = string.Empty;
        public string? BankName { get; init; }
        public int TransferCount { get; init; }
        public decimal TotalAmountSent { get; init; }
        public DateTime? LastTransferDate { get; init; }
    }
}