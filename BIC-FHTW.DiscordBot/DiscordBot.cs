using System;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;

namespace BIC_FHTW.DiscordBot;

public class DiscordBot
{
    private DiscordSocketClient _client;
    private Logger _logger;

    /*
    public DiscordBot(DatabaseManager databaseManager)
    {
        _client = new DiscordSocketClient();
        _commands = new CommandService();
        _services = new ServiceCollection()
            .AddSingleton(databaseManager)
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();
    }
    */

    /// <summary>
    /// Initializes the discord bot and registers all commands
    /// </summary>
    /// <remarks>Source: https://github.com/HRKings/DiscordNetBotTemplate/blob/master/DiscordNetBotTemplate/Startup.cs</remarks>
    public async Task Initialize()
    {
        _logger = LogManager.GetCurrentClassLogger();
        
        // You should dispose a service provider created using ASP.NET
        // when you are finished using it, at the end of your app's lifetime.
        // If you use another dependency injection framework, you should inspect
        // its documentation for the best way to do this.
        await using var services = ConfigureServices();
        _client = services.GetRequiredService<DiscordSocketClient>();

        _client.Ready += ReadyAsync;
        _client.Log += HandleLogAsync;
        services.GetRequiredService<CommandService>().Log += HandleLogAsync;

        // Tokens should be considered secret data and never hard-coded.
        // We can read from the environment variable to avoid hardcoding.
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        await _client.StartAsync();
        
        // FIXME might not work because not properly initialized?!
        services.GetRequiredService<IDatabaseService>();

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }
    
    private Task HandleLogAsync(LogMessage msg)
    {
        _logger.Log(ConvertLogSeverityToLogLevel(msg.Severity), msg.Exception, msg.Message);
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        // Logs the bot name and all the servers that it's connected to
        Console.WriteLine($"Connected to these servers as '{_client.CurrentUser.Username}': ");
        foreach (var guild in _client.Guilds) 
            Console.WriteLine($"- {guild.Name}");

        // Set the activity from the environment variable or fallback to 'I'm alive!'
        _client.SetGameAsync(Environment.GetEnvironmentVariable("DISCORD_BOT_ACTIVITY") ?? "I'm alive!", 
            type: ActivityType.CustomStatus);
        Console.WriteLine($"Activity set to '{_client.Activity.Name}'");
        Console.WriteLine("Bot is connected and ready.");
        return Task.CompletedTask;
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<IDatabaseService>()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .BuildServiceProvider();
    }
    
    private static LogLevel ConvertLogSeverityToLogLevel(LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Critical => LogLevel.Fatal,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Info => LogLevel.Info,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Warning => LogLevel.Warn,
            _ => LogLevel.Off, // FIXME Might be wrong?!?
        };
    }
}