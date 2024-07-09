using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FHTW.DiscordBot.Services;
using FHTW.Scraper.Services;
using MailKitSimplified.Sender.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Middleware;

public class RegisterMiddleware : InteractionMiddlewareBase
{
    private const int TokenLength = 32;
    private const string MailAddressOptionName = "email_address";
    private const string RegistrationCompleteApi = "api/fhtw/registration/complete-registration";
    public RegisterMiddleware(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override string CommandName => "register";
    public override string Description => "Registers a student on the server";
    public override List<SlashCommandOptionBuilder> Options => new()
    {
        new SlashCommandOptionBuilder()
            .WithName(MailAddressOptionName)
            .WithDescription("Your student email address")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)
    };
    
    // Any methods required by IMiddleware

    public override async Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        using var scope = ServiceProvider.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
        var mailService = scope.ServiceProvider.GetRequiredService<IEmailWriter>();
        Logger.LogDebug("ExecuteCmdAsync called...");
        
        var providedOptions = RetrieveOptions(command.Data.Options);
        // use services scoped to the command
        if (!IsValidEmail(providedOptions[MailAddressOptionName] as string))
        {
            await command.FollowupAsync($"Invalid email address. Only email addresses from domain {BotSettings.ValidMailDomain} are allowed.", ephemeral:true);
            return false;
            
        }
        
        var email = providedOptions[MailAddressOptionName] as string ?? throw new InvalidOperationException();
        var token = GenerateSecureToken(TokenLength);
        var user = await userService.AddUserAsync(command.User.Id, token, email);
        var registrationUrl = $"{BotSettings.WebApiUrl}/{RegistrationCompleteApi}?token={UrlEncoder.Default.Encode(token)}";
        var mailSuccessful = await mailService.To(command.User.Username, email)
            .From(BotSettings.BotName, BotSettings.BotMail)
            .Subject("BIC-FHTW Discord Server Registration")
            .BodyHtml("Welcome to the Server!" +
                      "<br><br>Please click the following link to complete your registration:"+
                      $"<br><a href=\"{registrationUrl}\">{registrationUrl}</a>" +
                      $"<br><br>If there are any problems during registration, please do not hesitate contacting the server administration on discord or responding to this email.<br><br>" +
                      $"<br><br>Kind regards,<br>{BotSettings.BotName}")
            .TrySendAsync();
        if (mailSuccessful)
        {
            Logger.LogInformation("Mail sent to {email} with registration link {registrationUrl}", email, registrationUrl);
            
        }
        else
        {
            Logger.LogError("Mail could not be sent to {email} with registration link {registrationUrl}", email, registrationUrl);
            await command.FollowupAsync("Sending you an email failed for some reasons. Ask @admins for clarification and please try again later.", ephemeral:true);
            return false;
        }
        await command.FollowupAsync("Registration email sent. Please check your inbox for a verification link. (It can take up to 15 minutes to receive the email)", ephemeral:true);
        return true;
        /* Some debugging code to check the API call directly
        using var httpClient = new HttpClient();
        _logger.LogDebug("Debugging...");
        var result = await httpClient.PostAsync($"{registrationUrl}", null);
        _logger.LogDebug("Received result: {result}", result);
        return result.IsSuccessStatusCode;
        */
    }
    
    private bool IsValidEmail(string? email)
    {
        if (email == null)
            return false;
        var splitMail = email.Split('@');
        return Regex.IsMatch(splitMail[0], @"^[\w\.]+$", RegexOptions.IgnoreCase)
            && Regex.IsMatch(splitMail[1], Regex.Escape(BotSettings.ValidMailDomain), RegexOptions.IgnoreCase);
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
        Logger.LogDebug("Generated token: {token}", randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}