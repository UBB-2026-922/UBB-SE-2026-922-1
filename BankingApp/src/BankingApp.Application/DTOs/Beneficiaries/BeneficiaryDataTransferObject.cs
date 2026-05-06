namespace BankingApp.Application.DTOs.Beneficiaries;

/// <summary>
///     Represents a beneficiary saved by a user for future transfers.
/// </summary>
public class BeneficiaryDataTransferObject
{
    /// <summary>
    ///     Gets or sets the beneficiary identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }
}