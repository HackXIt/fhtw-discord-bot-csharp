using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FHTW.DiscordBot.Services;
using FHTW.Scraper.Scrapers.Userprofile;
using FHTW.Scraper.Services;
using FHTW.Shared;
using MailKitSimplified.Sender.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Middleware;

public class UpdateMiddleware : InteractionMiddlewareBase
{
    public UpdateMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override string CommandName => "update";
    public override string Description => "Retrieve your user information and assign roles based on CIS";
    public override List<SlashCommandOptionBuilder> Options => new();
    public override async Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        using var scope = ServiceProvider.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
        var mailService = scope.ServiceProvider.GetRequiredService<IEmailWriter>();
        Logger.LogDebug("ExecuteCmdAsync called...");

        try
        {
            var user = await userService.GetUserByDiscordIdAsync(command.User.Id);
            var scrape = await scraperService.Scrape(new UserprofileScrapeArguments(user.MailUsername));
            if (scrape is not { Success: true, UserprofileScrapeResult: not null }) return false;
            if (string.Compare(user.MailUsername, scrape.UserprofileScrapeResult.Username, StringComparison.Ordinal) !=
                0)
            {
                Logger.LogInformation("Scraped username does not match user!");
                await command.FollowupAsync("Your user information could not be validated.", ephemeral:true);
                return false;
            }

            var activatedUser =
                await userService.ActivateUserWithStudentInformation(user.UserId, scrape.UserprofileScrapeResult);
            if (activatedUser == null)
            {
                Logger.LogInformation("User activation failed!");
                await command.FollowupAsync("User update failed, please contact @admin", ephemeral:true);
                return false;
            }

            var student = await userService.GetStudentByDiscordIdAsync(activatedUser.UserId);
            if (student == null)
            {
                Logger.LogInformation("Newly activated student not found!");
                await command.FollowupAsync("Failed to store student information, please contact @admin", ephemeral: true);
                return false;
            }
            
            // TODO Update roles based on user information
        }
        catch (NotFoundException ex)
        {
            Logger.LogError("Error during retrieval of user: Not found");
            await command.FollowupAsync("You are not registered with the server. Please use `/register` before updating information. Contact @admin if this issue persists.", ephemeral:true);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Error during user information update: {}", ex.Message);
            await command.FollowupAsync("An unknown error occured, please contact @admin", ephemeral: true);
            return false;
        }
        Logger.LogInformation("User information update successful.");
        await command.FollowupAsync("User information updated. You have been assigned the following roles: ", ephemeral: true);
        return true;
    }
}