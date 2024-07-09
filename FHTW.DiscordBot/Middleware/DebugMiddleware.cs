using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FHTW.DiscordBot.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Middleware;

[IsBotOwner]
public class DebugMiddleware : IInteractionMiddleware
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotService> _logger;
    private readonly BotSettings _botSettings;
    
    public DebugMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = _serviceProvider.GetRequiredService<ILogger<BotService>>();
        _botSettings = _serviceProvider.GetRequiredService<BotSettings>();
    }

    public string CommandName => "debug";
    public string Description => "Debugging command";
    public List<SlashCommandOptionBuilder> Options => new();
    
    public async Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        _logger.LogDebug("Debug command called...");
        try
        {
            return await DebugRoutine(scope);
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"Command failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> DebugRoutine(IServiceScope scope)
    {
        /*
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        using var httpClient = new HttpClient();
        var owner = await userService.GetUserByDiscordIdAsync(_botSettings.OwnerId);
        var result = await httpClient.PostAsync($"api/bic-fhtw/register/complete-registration?token={UrlEncoder.Default.Encode(owner.Token)}", null);
        _logger.LogDebug("Received result: {result}", result);
        return result.IsSuccessStatusCode;
        */
        return true;
    }
}