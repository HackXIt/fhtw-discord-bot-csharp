using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FHTW.DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FHTW.DiscordBot.Middleware;

public class RoleMiddleware : InteractionMiddlewareBase
{
    private const string AddCommand = "add";
    private const string RemoveCommand = "remove";
    private const string ListCommand = "list";
    private const string RoleOption = "role";
    
    public RoleMiddleware(IServiceProvider serviceProvider) : base(serviceProvider) { }
    
    public override string CommandName => "role";
    public override string Description => "Manage server roles";
    public override List<SlashCommandOptionBuilder> Options => new()
    {
        new SlashCommandOptionBuilder()
            .WithName(AddCommand)
            .WithDescription("Adds a role to manage for the role manager")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(RoleOption, ApplicationCommandOptionType.Role, "The role to add", true),
        new SlashCommandOptionBuilder()
            .WithName(RemoveCommand)
            .WithDescription("Removes a role from the role manager")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(RoleOption, ApplicationCommandOptionType.Role, "The role to remove", true),
        new SlashCommandOptionBuilder()
            .WithName(ListCommand)
            .WithDescription("Lists all roles managed by the role manager")
            .WithType(ApplicationCommandOptionType.SubCommand)
    };
    public override Task<bool> ExecuteCmdAsync(SocketSlashCommand command)
    {
        return command.Data.Options.First().Name switch
        {
            AddCommand => AddRole(command),
            RemoveCommand => RemoveRole(command),
            ListCommand => ListRoles(command),
            _ => Task.FromResult(false)
        };
    }

    private async Task<bool> AddRole(SocketSlashCommand command)
    {
        using var scope = ServiceProvider.CreateScope();
        var providedOptions = RetrieveOptions(command.Data.Options);
        var role = providedOptions[RoleOption] as SocketRole;
        if (role == null)
        {
            await command.FollowupAsync("Invalid role provided", ephemeral: true);
            return false;
        }
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        // TODO code here
        return true;
    }
    
    private async Task<bool> RemoveRole(SocketSlashCommand command)
    {
        using var scope = ServiceProvider.CreateScope();
        var providedOptions = RetrieveOptions(command.Data.Options);
        var role = providedOptions[RoleOption] as SocketRole;
        if (role == null)
        {
            await command.FollowupAsync("Invalid role provided", ephemeral: true);
            return false;
        }
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        // TODO code here
        return true;
    }
    
    private async Task<bool> ListRoles(SocketSlashCommand command)
    {
        using var scope = ServiceProvider.CreateScope();
        //var providedOptions = RetrieveOptions(command.Data.Options);
        // TODO code here
        return true;
    }
}