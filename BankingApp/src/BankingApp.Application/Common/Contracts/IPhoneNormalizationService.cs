namespace BankingApp.Application.Common.Utilities;

using PhoneNumbers;

public static class PhoneNormalization
{
    public static bool IsValidPhoneNumber(string phone, string defaultRegion = "RO") =>
        NormalizePhoneNumber(phone, defaultRegion) is not null;

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
