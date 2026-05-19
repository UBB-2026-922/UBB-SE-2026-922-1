namespace BankingApp.Contracts.Features.UserProfile.Dtos;

using Domain.Enums;

public class ProfileDto
{
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool Is2FaEnabled { get; set; }
    public TwoFactorMethod? Preferred2FaMethod { get; set; }
}
