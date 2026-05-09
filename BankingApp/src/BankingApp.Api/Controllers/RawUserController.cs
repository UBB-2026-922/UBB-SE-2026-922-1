#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/users")]
public class RawUserController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;

    public RawUserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("{userId:int}")]
    public IActionResult FindById(int userId)
        => ToActionResult(_userRepository.FindById(userId), Ok);

    [HttpPut]
    public IActionResult UpdateUser([FromBody] User user)
        => ToActionResult(_userRepository.UpdateUser(user));

    [HttpPut("{userId:int}/password")]
    public IActionResult UpdatePassword(int userId, [FromBody] UpdatePasswordRequest request)
        => ToActionResult(_userRepository.UpdatePassword(userId, request.NewPasswordHash));

    [HttpGet("{userId:int}/sessions")]
    public IActionResult GetActiveSessions(int userId)
        => ToActionResult(_userRepository.GetActiveSessions(userId), Ok);

    [HttpDelete("{userId:int}/sessions/{sessionId:int}")]
    public IActionResult RevokeSession(int userId, int sessionId)
        => ToActionResult(_userRepository.RevokeSession(userId, sessionId));

    [HttpGet("{userId:int}/notification-preferences")]
    public IActionResult GetNotificationPreferences(int userId)
        => ToActionResult(_userRepository.GetNotificationPreferences(userId), Ok);

    [HttpPut("{userId:int}/notification-preferences")]
    public IActionResult UpdateNotificationPreferences(int userId, [FromBody] List<NotificationPreference> preferences)
        => ToActionResult(_userRepository.UpdateNotificationPreferences(userId, preferences));

    public sealed class UpdatePasswordRequest
    {
        public string NewPasswordHash { get; set; } = string.Empty;
    }
}
#pragma warning restore CS1591
