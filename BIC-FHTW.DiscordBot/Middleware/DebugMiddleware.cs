using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot.Middleware;

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

    public string CommandName => "debugMe";
    public string Description => "Debugging command";
    public List<SlashCommandOptionBuilder> Options => new();
    
    public async Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        using var httpClient = new HttpClient();
        _logger.LogDebug("Debug command called...");
        var owner = await userService.GetUserByDiscordIdAsync(_botSettings.OwnerId);
        var result = await httpClient.PostAsync($"api/bic-fhtw/register/complete-registration?token={owner.Token}", null);
        _logger.LogDebug("Received result: {result}", result);
        return result.IsSuccessStatusCode;
    }
}