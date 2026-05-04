namespace BankingApp.Application.DataTransferObjects.Beneficiary;

/// <summary>
///     Represents the data required to create a beneficiary.
/// </summary>
public class CreateBeneficiaryRequest
{
	/// <summary>
	///     Gets or sets the beneficiary name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///     Gets or sets the beneficiary IBAN.
	/// </summary>
	public string Iban { get; set; } = string.Empty;

	/// <summary>
	///     Gets or sets the beneficiary bank name.
	/// </summary>
	public string? BankName { get; set; }
}
