using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using BIC_FHTW.WebApp.Services;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NETCore.MailKit;

namespace BIC_FHTW.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivationController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly DiscordSocketClient _client;
    private readonly IMailKitProvider _mailService;

    public ActivationController(
        DatabaseService databaseService, 
        DiscordSocketClient discordSocketClient, 
        IMailKitProvider mailService)
    {
        _databaseService = databaseService;
        _client = discordSocketClient;
        _mailService = mailService;
    }

    [HttpGet("activate")]
    public async Task<IActionResult> Activate([FromQuery] string token)
    {
        var user = _databaseService.GetUserByToken(token).Result;

        if (user != null)
        {
            return BadRequest("Invalid activation token.");
        }

        var guild = _client.GetGuild();
        if (guild == null)
        {
            return StatusCode(500, "Failed to get the Discord server.");
        }

        var member = await guild.GetUserAsync(user!.Value.DiscordId);
        if (member == null)
        {
            return StatusCode(500, "Failed to get the Discord user.");
        }

        var standardRole = guild.Roles.FirstOrDefault(role => role.Name == "Standard Membership");
        if (standardRole == null)
        {
            return StatusCode(500, "Failed to get the 'Standard Membership' role.");
        }

        await member.AddRoleAsync(standardRole);
        
        _databaseService.UpdateUserStatus(token, false);
        return Ok("Your account has been activated successfully!");
    }
    
    [HttpPost]
    public async Task<IActionResult> SendActivationEmail([FromBody] SendActivationEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Replace this with your actual activation email sending logic
        await SendActivationEmailAsync(request.EmailAddress, request.Token);

        return Ok();
    }

    
    private async Task SendActivationEmailAsync(string emailAddress, string token)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("FHTW-BIC Student Server", "no-reply@fhtw-bic.discord.com"));
        message.To.Add(new MailboxAddress("", emailAddress));

        message.Subject = "Account Activation";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"Please click the following link to activate your account: https://your-web-app-url.com/activate?token={token}"
        };

        message.Body = bodyBuilder.ToMessageBody();

        await _mailService.SmtpClient.SendAsync(message);
    }
    
    public class SendActivationEmailRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Token { get; set; }
    }
}