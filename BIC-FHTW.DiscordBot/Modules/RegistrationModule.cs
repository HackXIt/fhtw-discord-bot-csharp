using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using Discord.Commands;
using Newtonsoft.Json;

namespace BIC_FHTW.DiscordBot.Modules;

public class RegistrationModule : ModuleBase<SocketCommandContext>
{
    private readonly IDatabaseService _service;
    private const string RequiredDomain = "technikum-wien.at";
    private const string WebAppUrl = "https://localhost:5000";
    
    public RegistrationModule(IDatabaseService databaseService)
    {
        _service = databaseService;
    }
    
    [Command("register")]
    [Summary("Registers a user with their email address.")]
    public async Task HandleRegisterCommandAsync(SocketCommandContext context, [Remainder] string email)
    {
        if (!IsValidDomain(email, RequiredDomain))
        {
            await ReplyAsync($"Invalid email domain. Please use your provided student email address with the domain '{RequiredDomain}'.");
            return;
        }
        // TODO Verify correctness of email address syntax

        var discordId = Context.User.Id.ToString();

        var activationToken = _service.GenerateUniqueToken();
        _service.AddUser(discordId, email, activationToken);

        /*
        string activationLink = $"https://your-webserver-url/activate?token={token}";
        await _service.SendEmailAsync(email, "Discord Bot Activation", $"Please click the following link to activate your account: {activationLink}");
        */
        
        // Send a request to the ASP.NET Core web application to send the activation email
        using var httpClient = new HttpClient();
        var requestData = new JsonObject
        {
            { "EmailAddress", email },
            { "Token", activationToken }
        };
        var content = new StringContent(requestData.ToJsonString(), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"{WebAppUrl}/send-activation-email", content);

        if (response.IsSuccessStatusCode)
        {
            await ReplyAsync("An activation email has been sent to the provided address. Please check your inbox and click the link to complete your registration.");
        }
        else
        {
            await ReplyAsync("There was an error sending the activation email. Please try again later.");
        }
    }
    
    private bool IsValidDomain(string email, string requiredDomain)
    {
        var domainIndex = email.LastIndexOf('@');
        if (domainIndex == -1) return false;

        var domain = email.Substring(domainIndex + 1);
        return domain.Equals(requiredDomain, StringComparison.OrdinalIgnoreCase);
    }
}