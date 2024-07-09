using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Middleware;

public abstract class InteractionMiddlewareBase : IInteractionMiddleware
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger<BotService> Logger;
    protected readonly BotSettings BotSettings;
    
    public InteractionMiddlewareBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = ServiceProvider.GetRequiredService<ILogger<BotService>>();
        BotSettings = ServiceProvider.GetRequiredService<BotSettings>();
    }
    
    public abstract string CommandName { get; }
    public abstract string Description { get; }
    public abstract List<SlashCommandOptionBuilder> Options { get; }
    public abstract Task<bool> ExecuteCmdAsync(SocketSlashCommand command);

    protected static Dictionary<string, object> RetrieveOptions(IEnumerable<SocketSlashCommandDataOption> options)
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
}