#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Services.Notifications;
using Application.Services.Security;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/security")]
public class SecurityController(
    IHashService hashService,
    IJsonWebTokenService jsonWebTokenService,
    IOtpService otpService,
    IEmailService emailService)
    : ApiController
{
    [HttpPost("hash")]
    public IActionResult Hash([FromBody] HashRequest request)
        => ToActionResult(hashService.GetHash(request.Input), Ok);

    [HttpPost("verify-hash")]
    public IActionResult VerifyHash([FromBody] VerifyHashRequest request)
        => ToActionResult(hashService.Verify(request.Input, request.Hash), matches => Ok(matches));

    [HttpPost("jwt/generate")]
    public IActionResult GenerateToken([FromBody] GenerateTokenRequest request)
        => ToActionResult(jsonWebTokenService.GenerateToken(request.UserId), Ok);

    [HttpPost("otp/generate-totp")]
    public IActionResult GenerateTotp([FromBody] OtpUserRequest request)
        => ToActionResult(otpService.GenerateTotp(request.UserId), Ok);

    [HttpPost("otp/verify-totp")]
    public IActionResult VerifyTotp([FromBody] VerifyOtpRequest request)
        => ToActionResult(otpService.VerifyTotp(request.UserId, request.Code), isValid => Ok(isValid));

    [HttpPost("otp/generate-sms")]
    public IActionResult GenerateSmsOtp([FromBody] OtpUserRequest request)
        => ToActionResult(otpService.GenerateSmsOtp(request.UserId), Ok);

    [HttpPost("otp/verify-sms")]
    public IActionResult VerifySmsOtp([FromBody] VerifyOtpRequest request)
        => ToActionResult(otpService.VerifySmsOtp(request.UserId, request.Code), isValid => Ok(isValid));

    [HttpPost("otp/invalidate")]
    public IActionResult InvalidateOtp([FromBody] OtpUserRequest request)
    {
        otpService.InvalidateOtp(request.UserId);
        return Ok();
    }

    [HttpPost("email/send-password-reset-link")]
    public IActionResult SendPasswordResetLink([FromBody] PasswordResetEmailRequest request)
    {
        emailService.SendPasswordResetLink(request.Email, request.Token);
        return Ok();
    }

    [HttpPost("email/send-otp")]
    public IActionResult SendOtpCode([FromBody] OtpEmailRequest request)
    {
        emailService.SendOtpCode(request.Email, request.Code);
        return Ok();
    }

    [HttpPost("email/send-login-alert")]
    public IActionResult SendLoginAlert([FromBody] EmailRequest request)
    {
        emailService.SendLoginAlert(request.Email);
        return Ok();
    }

    [HttpPost("email/send-lock-notification")]
    public IActionResult SendLockNotification([FromBody] EmailRequest request)
    {
        emailService.SendLockNotification(request.Email);
        return Ok();
    }

    public sealed class HashRequest
    {
        public string Input { get; set; } = string.Empty;
    }

    public sealed class VerifyHashRequest
    {
        public string Input { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }

    public sealed class GenerateTokenRequest
    {
        public int UserId { get; set; }
    }

    public sealed class OtpUserRequest
    {
        public int UserId { get; set; }
    }

    public sealed class VerifyOtpRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public sealed class EmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public sealed class OtpEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public sealed class PasswordResetEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
#pragma warning restore CS1591
