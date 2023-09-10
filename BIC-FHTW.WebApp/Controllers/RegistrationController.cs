using System;
using BIC_FHTW.DiscordBot.Services;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace BIC_FHTW.WebApp.Controllers;

[Route("api/bic-fhtw/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly DiscordSocketClient _client;

    public RegistrationController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet("register")]
    [HttpPost("register")]
    public IActionResult Register(string mailAddress)
    {
        return Forbid();
    }
    
}