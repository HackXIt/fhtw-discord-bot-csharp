using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using FHTW.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FHTW.WebApp.Controllers;

[Route($"{Constants.botApiPath}/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    [HttpGet("login")]
    [HttpPost("login")]
    public IActionResult LogIn()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/");
    }

    [HttpGet("userinfo")]
    public ActionResult<DiscordUserDTO> GetUserInfoAsync()
    {
        if (!User.Identity.IsAuthenticated)
            return Ok(new DiscordUserDTO());

        var userInfo = new DiscordUserDTO()
        {
            IsAuthenticated = User.Identity.IsAuthenticated
        };

        foreach (var claim in User.Claims)
        {
            switch (claim.Type)
            {
                case ClaimTypes.NameIdentifier:
                    userInfo.UserId = ulong.Parse(claim.Value);
                    break;

                case ClaimTypes.Name:
                    userInfo.Username = claim.Value;
                    break;

                case DiscordAuthenticationConstants.Claims.AvatarHash:
                    userInfo.AvatarHash = claim.Value;
                    break;

                case Constants.IsBotOwner:
                    userInfo.Claims.Add(claim.Type, claim.Value);
                    break;
            }
        }

        return Ok(userInfo);
    }
}