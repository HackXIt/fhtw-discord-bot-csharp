﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Middleware;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot.Handler;

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
            // Acknowledge the command
            await command.DeferAsync();
            // Try to find a handler for the received command
            if (_interactionHandlers.TryGetValue(command.Data.Name, out var handler))
            {
                _logger.LogInformation("Executing handler for command /{} from {}({}).", handler.CommandName, command.User.Username, command.User.Id);
                var taskCompletion = await handler.ExecuteCmdAsync(command);
                if (!taskCompletion)
                {
                    _logger.LogInformation("Command /{} from {}({}) failed.", handler.CommandName, command.User.Username, command.User.Id);
                }
            }
        }
    }
}