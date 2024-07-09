using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FHTW.DiscordBot.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Handler;

public class InteractionHandler : IHandler
{
    private readonly DiscordSocketClient _client;
    private readonly BotSettings _settings;
    private readonly ILogger<BotService> _logger;
    private readonly InteractionService _interactions;
    
    private IServiceScope? _scope;
    
    private readonly Dictionary<string, IInteractionMiddleware> _interactionHandlers = new();

    public InteractionHandler(DiscordSocketClient client, InteractionService interactions, BotSettings settings, ILogger<BotService> logger)
    {
        _client = client;
        _interactions = interactions;
        _settings = settings;
        _logger = logger;
    }

    internal void Initialize(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        foreach (var middleware in _scope.ServiceProvider.GetServices<IInteractionMiddleware>())
        {
            _interactionHandlers.Add(middleware.CommandName, middleware);
        }

        BindEvents();
    }
    
    public void BindEvents()
    {
        _client.Ready += HandleReady;
        _client.InteractionCreated += HandleInteractionCreated;
    }
    public void UnbindEvents()
    {
        // nothing to do here yet
        _client.Ready -= HandleReady;
        _client.InteractionCreated -= HandleInteractionCreated;
    }
    
    private async Task HandleReady()
    {
        foreach (var (key, handler) in _interactionHandlers)
        {
            _logger.LogInformation("Setting up command /{}...", key);

            var commandBuilder = new SlashCommandBuilder()
                .WithName(handler.CommandName)
                .WithDescription(handler.Description);

            foreach (var option in handler.Options)
            {
                commandBuilder.AddOption(option);
            }

            await _client.Rest.CreateGlobalCommand(commandBuilder.Build());
        }
    }
    
    private async Task HandleInteractionCreated(SocketInteraction arg)
    {
        if (arg is SocketSlashCommand command)
        {
            // Try to find a handler for the received command
            if (_interactionHandlers.TryGetValue(command.Data.Name, out var handler))
            {
                // Found handler => Acknowledge the command
                await command.DeferAsync();
                _logger.LogInformation("Executing handler for command /{} from {}({}).", handler.CommandName, command.User.Username, command.User.Id);
                bool taskCompletion;
                try
                {
                    taskCompletion = await handler.ExecuteCmdAsync(command);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Command /{} from {}({}) threw exception: {}", handler.CommandName, command.User.Username, command.User.Id, ex);
                    await command.RespondAsync("There was a problem executing this command.", ephemeral: true);
                    taskCompletion = false;
                }
                if (!taskCompletion)
                {
                    _logger.LogInformation("Command /{} from {}({}) failed.", handler.CommandName, command.User.Username, command.User.Id);
                }
            }
            else
            {
                _logger.LogInformation("No handler found for command /{} from {}({}).", command.Data.Name, command.User.Username, command.User.Id);
                await command.RespondAsync("This command is currently unavailable.", ephemeral:true);
            }

            if (!command.HasResponded)
            {
                await command.RespondAsync("There seems to be an unknown problem executing this command.", ephemeral: true);
            }
        }
    }
}