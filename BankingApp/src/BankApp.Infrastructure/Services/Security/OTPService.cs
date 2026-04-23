// <copyright file="OTPService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OTPService class.
// </summary>

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using BankApp.Application.Services.Security;
using ErrorOr;

namespace BankApp.Infrastructure.Services.Security;

/// <summary>
///     Provides HMAC-based TOTP and in-memory SMS OTP generation and verification.
/// </summary>
public class OtpService : IOtpService
{
    /// <summary>Number of seconds in one TOTP time window.</summary>
    public const int TotpWindowSeconds = 60;

    private const int SmsOtpExpiryMinutes = 5;
    private const int OtpRangeMinimum = 100000;
    private const int OtpRangeMaximum = 999999;
    private const int OtpModulus = 1000000;
    private const int OtpDigitCount = 6;
    private const int TruncationOffsetMask = 0x0F;
    private const int SignBitMask = 0x7F;
    private const int ByteMask = 0xFF;
    private const int PreviousTotpWindowOffset = 1;
    private const int FirstDynamicTruncationByteOffset = 0;
    private const int SecondDynamicTruncationByteOffset = 1;
    private const int ThirdDynamicTruncationByteOffset = 2;
    private const int FourthDynamicTruncationByteOffset = 3;
    private const int FirstDynamicTruncationByteShift = 24;
    private const int SecondDynamicTruncationByteShift = 16;
    private const int ThirdDynamicTruncationByteShift = 8;

    private static readonly ConcurrentDictionary<int, (string Code, DateTime ExpiryTime)> _temporarySmsStorage = [];

    private readonly string _otpServerSecret;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OtpService" /> class.
    /// </summary>
    /// <param name="otpServerSecret">The server otp secret used by the <see cref="OtpService"/>.</param>
    public OtpService(string otpServerSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(otpServerSecret);
        _otpServerSecret = otpServerSecret;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<string> GenerateSmsOtp(int userId)
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(OtpRangeMinimum, OtpRangeMaximum).ToString();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(SmsOtpExpiryMinutes);
            _temporarySmsStorage[userId] = (code, expiryTime);
            return code;
        }
        catch (Exception exception)
        {
            return Error.Failure("otp.generate_sms_failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<string> GenerateTotp(int userId)
    {
        try
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            return GenerateHmacCode(userId, currentWindow);
        }
        catch (Exception exception)
        {
            return Error.Failure("otp.generate_totp_failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    public void InvalidateOtp(int userId)
    {
        _temporarySmsStorage.TryRemove(userId, out _);
    }

    /// <inheritdoc />
    /// <param name="expiredAt">The expiredAt value.</param>
    /// <returns>The result of the operation.</returns>
    public bool IsExpired(DateTime expiredAt)
    {
        return DateTime.UtcNow > expiredAt;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="code">The code value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> VerifySmsOtp(int userId, string code)
    {
        try
        {
            if (!_temporarySmsStorage.TryGetValue(userId, out (string Code, DateTime ExpiryTime) storedOtpData))
            {
                return false;
            }

            if (DateTime.UtcNow > storedOtpData.ExpiryTime)
            {
                InvalidateOtp(userId);
                return false;
            }

            if (storedOtpData.Code != code)
            {
                return false;
            }

            InvalidateOtp(userId);
            return true;

        }
        catch (Exception exception)
        {
            return Error.Failure("otp.verify_sms_failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="code">The code value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> VerifyTotp(int userId, string code)
    {
        try
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            if (code == GenerateHmacCode(userId, currentWindow))
            {
                return true;
            }

            if (code == GenerateHmacCode(userId, currentWindow - PreviousTotpWindowOffset))
            {
                return true;
            }

            return false;
        }
        catch (Exception exception)
        {
            return Error.Failure("otp.verify_totp_failed", exception.Message);
        }
    }

    private string GenerateHmacCode(int userId, long timeWindow)
    {
        var secret = $"{_otpServerSecret}_{userId}";
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(BitConverter.GetBytes(timeWindow));
        int offset = hash.Last() & TruncationOffsetMask;
        int binary = ((hash.ElementAt(offset + FirstDynamicTruncationByteOffset) & SignBitMask) <<
                      FirstDynamicTruncationByteShift) |
                     ((hash.ElementAt(offset + SecondDynamicTruncationByteOffset) & ByteMask) <<
                      SecondDynamicTruncationByteShift) |
                     ((hash.ElementAt(offset + ThirdDynamicTruncationByteOffset) & ByteMask) <<
                      ThirdDynamicTruncationByteShift) |
                     (hash.ElementAt(offset + FourthDynamicTruncationByteOffset) & ByteMask);
        return (binary % OtpModulus).ToString($"D{OtpDigitCount}");
    }
}