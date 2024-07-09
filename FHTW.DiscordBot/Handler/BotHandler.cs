using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FHTW.DiscordBot.Services;
using FHTW.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FHTW.DiscordBot.Handler;

public class BotHandler : IHandler
{
    private readonly DiscordSocketClient _client;
    private readonly BotSettings _settings;
    private readonly ILogger<BotService> _logger;
    
    private IServiceScope? _scope;
    private EventService _eventService;

    public BotHandler(DiscordSocketClient client, BotSettings settings, ILogger<BotService> logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
    }

    internal void Initialize(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _eventService = _scope.ServiceProvider.GetRequiredService<EventService>();

        BindEvents();
    }

    public void BindEvents()
    {
        _client.Ready += HandleReady;
        _client.Log += LogAsync;
        _eventService.StudentRegistered += HandleStudentRegistered;
    }

    public void UnbindEvents()
    {
        _client.Ready -= HandleReady;
        _client.Log -= LogAsync;
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation("{}", log.Message);
        return Task.CompletedTask;
    }

    private async Task HandleReady()
    {
        await _client.SetActivityAsync(new Game($"BIC-FHTW | Use me via command prefix {_settings.Prefix}"));
    }

    private async void HandleStudentRegistered(object? sender, EventService.StudentRegisteredEventArgs eventArgs)
    {
        var owner = _client.GetUser(_settings.OwnerId);
        var roleService = _scope?.ServiceProvider.GetService<IRoleService>();
        if (roleService == null)
        {
            _logger.LogWarning("RoleService currently not available!");
            return;
        }
        var guild = _client.GetGuild(_settings.GuildId);
        await guild.DownloadUsersAsync();
        //var user = guild.GetUser(eventArgs.DiscordUserId);
        var user = guild.GetUser(eventArgs.DiscordUserId);
        if (user == null)
        {
            _logger.LogWarning("User with id {DiscordUserId} not found in guild {GuildId}", eventArgs.DiscordUserId, _settings.GuildId);
            return;
        }

        var roles = (await roleService.GetDiscordRolesAsync(guild.Id)).ToList();
        var studentRole = roles.FirstOrDefault(role => role.RoleName == "BIC-Student");
        var existingStudentRole = roles.FirstOrDefault(role => role.RoleName == eventArgs.Student.StudentYear);
        if (existingStudentRole == null)
        {
            var newRole = await guild.CreateRoleAsync(eventArgs.Student.StudentYear, GuildPermissions.None, Color.LightOrange, true, true);
            await roleService.AddRoleAsync(newRole.Id, guild.Id, newRole.Name);
            await user.AddRoleAsync(newRole);
        }
        else
        {
            var existingRole = guild.GetRole(existingStudentRole.RoleId);
            await user.AddRoleAsync(existingRole);
        }

        if (studentRole != null)
        {
            await user.AddRoleAsync(studentRole.RoleId);
        }
        else
        {
            _logger.LogWarning("General Student role not found!");
        }
        await user.SendMessageAsync(
            $"User activation successful. New role {eventArgs.Student.StudentYear} has been assigned.");
    }
}