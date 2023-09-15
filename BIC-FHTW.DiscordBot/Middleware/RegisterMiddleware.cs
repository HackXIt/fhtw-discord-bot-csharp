using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using BIC_FHTW.Scraper.Services;
using Discord;
using Discord.WebSocket;
using MailKitSimplified.Sender.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot.Middleware;

public class RegisterMiddleware : IInteractionMiddleware
{
    private const int TokenLength = 32;
    private const string MailAddressOptionName = "email_address";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotService> _logger;
    private readonly BotSettings _botSettings;
    
    public RegisterMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = _serviceProvider.GetRequiredService<ILogger<BotService>>();
        _botSettings = _serviceProvider.GetRequiredService<BotSettings>();
    }

    public string CommandName => "register";
    public string Description => "Registers a student on the server";
    public List<SlashCommandOptionBuilder> Options => new()
    {
        new SlashCommandOptionBuilder()
            .WithName(MailAddressOptionName)
            .WithDescription("Your student email address")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)
    };
    
    // Any methods required by IMiddleware

    public async Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
            var mailService = scope.ServiceProvider.GetRequiredService<IEmailWriter>();
            _logger.LogDebug("ExecuteCmdAsync called...");

            var providedOptions = RetrieveOptions(command.Data.Options);
            // use services scoped to the command
            if (!IsValidEmail(providedOptions[MailAddressOptionName] as string))
            {
                await command.FollowupAsync($"Invalid email address. Only email addresses from domain {_botSettings.ValidMailDomain} are allowed.", ephemeral:true);
                return false;
            }

            var email = providedOptions[MailAddressOptionName] as string ?? throw new InvalidOperationException();
            var token = GenerateSecureToken(TokenLength);
            var user = await userService.AddUserAsync(command.User.Id, token, email);
            var registrationUrl = $"{_botSettings.WebApiUrl}/api/bic-fhtw/registration/complete-registration?token={UrlEncoder.Default.Encode(token)}";
            var mailSuccessful = await mailService.To(command.User.Username, email)
                .From(_botSettings.BotName, _botSettings.BotMail)
                .Subject("BIC-FHTW Discord Server Registration")
                .BodyHtml($"Please click the following link to complete your registration: <a href=\"{registrationUrl}\">{registrationUrl}</a>")
                .TrySendAsync();
            if (mailSuccessful)
            {
                _logger.LogInformation("Mail sent to {email} with registration link {registrationUrl}", email, registrationUrl);
            }
            else
            {
                _logger.LogError("Mail could not be sent to {email} with registration link {registrationUrl}", email, registrationUrl);
                await command.FollowupAsync("Sending you an email failed for some reasons. Ask @Administration for clarification and please try again later.", ephemeral:true);
                return false;
            }
            await command.FollowupAsync("User registered. Please check your email inbox for a verification link.", ephemeral:true);
            return true;
            /* Some debugging code to check the API call directly
            using var httpClient = new HttpClient();
            _logger.LogDebug("Debugging...");
            var result = await httpClient.PostAsync($"{registrationUrl}", null);
            _logger.LogDebug("Received result: {result}", result);
            return result.IsSuccessStatusCode;
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during /{} command execution", CommandName);
            await command.FollowupAsync("An unexpected error occured. Please try again later.", ephemeral:true);
        }
        return false;
    }
    
    private Dictionary<string, object> RetrieveOptions(IEnumerable<SocketSlashCommandDataOption> options)
    {
        var providedOptions = new Dictionary<string, object>();
        foreach (var option in options)
        {
            if (option.Type == ApplicationCommandOptionType.SubCommand)
            {
                providedOptions = RetrieveOptions(option.Options);
            }
            else
            {
                providedOptions.Add(option.Name, option.Value);
            }
        }

        return providedOptions;
    }
    
    private bool IsValidEmail(string? email)
    {
        if (email == null)
            return false;
        var splitMail = email.Split('@');
        return Regex.IsMatch(splitMail[0], @"^[\w\.]+$", RegexOptions.IgnoreCase)
            && Regex.IsMatch(splitMail[1], Regex.Escape(_botSettings.ValidMailDomain), RegexOptions.IgnoreCase);
    }
    
    // Generating a secure token can be done outside the registration command (e.g. in the registration controller)
    // ReSharper disable once MemberCanBePrivate.Global
    public string GenerateSecureToken(int length)
    {
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        if (randomNumberGenerator == null) 
        {
            throw new InvalidOperationException("Unable to create a random number generator.");
        }

        var randomNumber = new byte[length];
        randomNumberGenerator.GetBytes(randomNumber);
        _logger.LogDebug("Generated token: {token}", randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}