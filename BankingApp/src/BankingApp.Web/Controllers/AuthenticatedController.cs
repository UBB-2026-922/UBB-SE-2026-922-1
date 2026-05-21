namespace BankingApp.Web.Controllers;

using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Http;
using Microsoft.AspNetCore.Mvc;

public abstract class AuthenticatedController : Controller
{
    protected int CurrentUserId
    {
        get
        {
            string? userIdValue = User.FindFirstValue(AuthClaimTypes.UserId)
                                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue)
                || !int.TryParse(userIdValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int userId))
            {
                throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");
            }

            return userId;
        }
    }
}
