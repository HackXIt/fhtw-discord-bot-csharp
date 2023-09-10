using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIC_FHTW.Database.Models;
using BIC_FHTW.DiscordBot.Services;
using Discord;
using Discord.Commands;

namespace BIC_FHTW.DiscordBot.Modules;

[RequireBotPermission(GuildPermission.ManageRoles)]
public class RegistrationModule : ModuleBase<SocketCommandContext>
{
    private const int TokenLength = 32;
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    public RegistrationModule(IRoleService roleService, IUserService userService)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }
    
    [Command("register")]
    [Summary("Registers a user with their student email address.")]
    public async Task RegisterAsync([Remainder][Summary("The student email address to register with.")] string email)
    {
        if (!IsValidEmail(email))
        {
            await ReplyAsync("Invalid email address. Please make sure you're using the correct domain.");
            return;
        }

        var existingUser = await _userService.GetUserByDiscordIdAsync(Context.User.Id);
        if (existingUser.Status == DiscordUser.UserStatus.Active)
        {
            await ReplyAsync("You are already registered.");
            return;
        }

        var token = GenerateSecureToken(TokenLength);
        await _userService.AddUserAsync(Context.User.Id, token);
        
        /*
        var response = await ForwardRegistrationRequestToWebApi(
            $"{Environment.GetEnvironmentVariable("API-URL")}/register",
            Context.User.Id, email, Environment.GetEnvironmentVariable("API-KEY") ?? string.Empty);



        if (response.IsSuccessStatusCode)
        {
            await ReplyAsync("Registration successful! Please check your email for an activation link.");
        }
        else
        {
            await ReplyAsync("Registration failed. Please try again later.");
        }
         */
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email && IsValidDomain(email);
        }
        catch (Exception ex) when (ex is ArgumentNullException ||
                                   ex is ArgumentException ||
                                   ex is FormatException)
        {
            return false;
        }
    }

    private bool IsValidDomain(string email)
    {
        var domainPattern = $@"^[\w\.-]+@{Environment.GetEnvironmentVariable("ALLOWED_DOMAIN")}$";
        return Regex.IsMatch(email, domainPattern, RegexOptions.IgnoreCase);
    }

    private async Task<HttpResponseMessage> ForwardRegistrationRequestToWebApi(string webApiUrl, ulong discordUserId, string email, string apikey)
    {
        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("discordUserId", discordUserId.ToString()),
            new KeyValuePair<string, string>("email", email),
            new KeyValuePair<string, string>("apikey", apikey)
        });

        return await httpClient.PostAsync(webApiUrl, content);
    }
    
    private string GenerateSecureToken(int length)
    {
        using (var randomNumberGenerator = new RNGCryptoServiceProvider())
        {
            var randomNumber = new byte[length];
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}