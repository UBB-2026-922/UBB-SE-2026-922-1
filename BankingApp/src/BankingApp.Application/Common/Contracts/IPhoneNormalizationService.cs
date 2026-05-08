namespace BankingApp.Application.Common.Contracts;

// TODO: move implementation into infrastructure.
using PhoneNumbers;

/// <summary>
/// TODO: add docs.
/// </summary>
public interface IPhoneNormalizationService
{

    /// <summary>
    ///     Determines whether the specified string is a valid phone number.
    /// </summary>
    /// <param name="phone">The phone number string to validate.</param>
    /// <param name="defaultRegion">
    ///     The default country code to fall back to. Defaults to RO until callers can supply a user-specific region.
    /// </param>
    /// <returns><see langword="true" /> if the phone number is valid; otherwise, <see langword="false" />.</returns>
    public static bool IsValidPhoneNumber(string phone, string defaultRegion = "RO")
    {
        return NormalizePhoneNumber(phone, defaultRegion) is not null;
    }

    /// <summary>
    ///     Normalizes the specified phone number to E.164 format.
    /// </summary>
    /// <param name="phone">The phone number string to normalize.</param>
    /// <param name="defaultRegion">
    ///     The default country code to fall back to. Defaults to RO until callers can supply a user-specific region.
    /// </param>
    /// <returns>The normalized E.164 phone number if valid; otherwise, <see langword="null" />.</returns>
    public static string? NormalizePhoneNumber(string phone, string defaultRegion = "RO")
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var phoneUtil = PhoneNumberUtil.GetInstance();
        try
        {
            PhoneNumber parsedNumber = phoneUtil.Parse(phone.Trim(), defaultRegion);
            return phoneUtil.IsValidNumber(parsedNumber)
                ? phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164)
                : null;
        }
        catch (NumberParseException)
        {
            return null;
        }
    }
}