using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BIC_FHTW.Database.Services;
using BIC_FHTW.Shared;
using Discord;
using Discord.WebSocket;

namespace BIC_FHTW.DiscordBot.Services;

public class RoleService : IRoleService
{
    private readonly DiscordSocketClient _client;
    private readonly RoleManager _roleManager;

    public RoleService(DiscordSocketClient client, RoleManager roleManager)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public Task<IEnumerable<RoleDTO>> GetDiscordRolesAsync(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null)
            return Task.FromResult(Enumerable.Empty<RoleDTO>());
        return Task.FromResult(guild.Roles.Select(r => new RoleDTO
        {
            RoleId = r.Id,
            RoleName = r.Name,
            RoleColour = r.Color.ToString()
        }));
    }

    public Task<ulong[]> GetRequestableRoleIdsAsync(ulong guildId) =>
        _roleManager.GetRequestableRoleIdsAsync(guildId);

    public async Task<IEnumerable<RoleDTO>> GetRequestableRolesAsync(ulong guildId)
    {
        var requestableIds = await _roleManager.GetRequestableRoleIdsAsync(guildId);
        var guild = _client.GetGuild(guildId);

        List<RoleDTO> roles = new List<RoleDTO>();

        for (int i = 0; i < requestableIds.Length; i++)
        {
            var role = guild.GetRole(requestableIds[i]);

            if (role == null)
                continue;

            roles.Add(new RoleDTO { 
                RoleId = role.Id,
                RoleColour = role.Color.ToString(),
                RoleName = role.Name
            });
        }

        return roles;
    }

    public Task<IEnumerable<RoleDTO>> GetUserDiscordRolesAsync(ulong guildId, ulong userId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null)
            return Task.FromResult(Enumerable.Empty<RoleDTO>());
        var user = guild.GetUser(userId);
        if (user == null)
            return Task.FromResult(Enumerable.Empty<RoleDTO>());
        return Task.FromResult(user.Roles.Select(r => new RoleDTO
        {
            RoleId = r.Id,
            RoleName = r.Name,
            RoleColour = r.Color.ToString()
        }));
    }

    public async Task<IRoleService.Result> AddRoleAsync(ulong newRoleId, ulong guildId, string newRoleName)
    {
        return await _roleManager.AddRoleAsync(newRoleId, guildId, newRoleName) 
            ? IRoleService.Result.RoleNotFound 
            : IRoleService.Result.Success;
    }

    public async Task<IRoleService.Result> GiveRoleToUserAsync(IGuildUser user, IRole role)
    {
        if (!await _roleManager.IsRoleRequestableAsync(role.Id))
            return IRoleService.Result.RoleNotAllowed;

        if (user.RoleIds.Contains(role.Id))
            return IRoleService.Result.AlreadyHasRole;

        await user.AddRoleAsync(role);
        return IRoleService.Result.Success;
    }

    public async Task<IRoleService.Result> GiveRoleToUserAsync(ulong guildId, ulong userId, ulong roleId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null)
            return IRoleService.Result.GuildNotFound;
        var user = guild.GetUser(userId);
        if (user == null)
            return IRoleService.Result.UserNotFound;
        var role = guild.GetRole(roleId);
        if (role == null)
            return IRoleService.Result.RoleNotFound;

        if (!await _roleManager.IsRoleRequestableAsync(roleId))
            return IRoleService.Result.RoleNotAllowed;

        if (user.Roles.Any(r => r.Id == roleId))
            return IRoleService.Result.AlreadyHasRole;

        await user.AddRoleAsync(role);
        return IRoleService.Result.Success;
    }

    public async Task<IRoleService.Result> SetRoleRequestableAsync(IRole role)
    {
        if (await _roleManager.AddRoleAsync(role.Id, role.Guild.Id, role.Name))
            return IRoleService.Result.Success;

        return IRoleService.Result.RoleAlreadyRequestable;
    }

    public async Task<IRoleService.Result> SetRoleRequestableAsync(ulong guildId, ulong roleId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null)
            return IRoleService.Result.GuildNotFound;
        var role = guild.GetRole(roleId);
        if (role == null)
            return IRoleService.Result.RoleNotFound;

        if (await _roleManager.AddRoleAsync(roleId, guildId, role.Name))
            return IRoleService.Result.Success;

        return IRoleService.Result.RoleAlreadyRequestable;
    }

    public async Task<IRoleService.Result> TakeRoleFromUserAsync(IGuildUser user, IRole role)
    {
        if (!await _roleManager.IsRoleRequestableAsync(role.Id))
            return IRoleService.Result.RoleNotAllowed;

        if (!user.RoleIds.Contains(role.Id))
            return IRoleService.Result.DoesNotHaveRole;

        await user.RemoveRoleAsync(role);
        return IRoleService.Result.Success;
    }

    public async Task<IRoleService.Result> TakeRoleFromUserAsync(ulong guildId, ulong userId, ulong roleId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null)
            return IRoleService.Result.GuildNotFound;
        var user = guild.GetUser(userId);
        if (user == null)
            return IRoleService.Result.UserNotFound;
        var role = guild.GetRole(roleId);
        if (role == null)
            return IRoleService.Result.RoleNotFound;

        if (!await _roleManager.IsRoleRequestableAsync(roleId))
            return IRoleService.Result.RoleNotAllowed;

        if (!user.Roles.Any(r => r.Id == roleId))
            return IRoleService.Result.DoesNotHaveRole;

        await user.RemoveRoleAsync(role);
        return IRoleService.Result.Success;
    }

    public async Task<IRoleService.Result> UnsetRoleRequestableAsync(IRole role)
    {
        if (await _roleManager.RemoveRoleAsync(role.Id))
            return IRoleService.Result.Success;

        return IRoleService.Result.RoleNotRequestable;
    }

    public async Task<IRoleService.Result> UnsetRoleRequestableAsync(ulong roleId)
    {
        if (await _roleManager.RemoveRoleAsync(roleId))
            return IRoleService.Result.Success;

        return IRoleService.Result.RoleNotRequestable;
    }
}