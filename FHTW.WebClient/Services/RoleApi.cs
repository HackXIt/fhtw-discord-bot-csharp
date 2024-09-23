using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class RoleApi : IRoleApi
{
    private readonly HttpClient _client;

    public RoleApi(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task<IEnumerable<RoleDTO>> GetDiscordRolesAsync(ulong guildId) =>
        _client.GetFromJsonAsync<IEnumerable<RoleDTO>>($"{Constants.botApiPath}/guild/{guildId}");

    public Task<IEnumerable<ulong>> GetRequestableRoleIdsAsync(ulong guildId) =>
        _client.GetFromJsonAsync<IEnumerable<ulong>>($"{Constants.botApiPath}/requestable/{guildId}");

    public Task<IEnumerable<GuildWithRolesDTO>> GetUserGuildsWithRequestableRoles(ulong userID) =>
        _client.GetFromJsonAsync<IEnumerable<GuildWithRolesDTO>>($"{Constants.botApiPath}/userguilds/{userID}");

    public Task<IEnumerable<RoleDTO>> GetUserDiscordRolesAsync(ulong guildId, ulong userId) =>
        _client.GetFromJsonAsync<IEnumerable<RoleDTO>>($"{Constants.botApiPath}/user/{guildId}/{userId}");

    public async Task GiveRoleAsync(ulong guildId, ulong roleId)
    {
        var mesage = await _client.PostAsJsonAsync($"{Constants.botApiPath}/give/", new { guildId, roleId });
        mesage.EnsureSuccessStatusCode();
    }

    public async Task SetRequestableAsync(ulong guildId, ulong roleId)
    {
        var mesage = await _client.PostAsJsonAsync("setrequestable", new { guildId, roleId });
        mesage.EnsureSuccessStatusCode();
    }

    public async Task TakeRoleAsync(ulong guildId, ulong roleId)
    {
        var mesage = await _client.PostAsJsonAsync("take", new { guildId, roleId });
        mesage.EnsureSuccessStatusCode();
    }

    public async Task UnsetRequestableAsync(ulong guildId, ulong roleId)
    {
        using StringContent content = new StringContent(JsonSerializer.Serialize(new { guildId, roleId }));

        using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, "unsetrequestable");
        message.Content = content;

        var response = await _client.SendAsync(message);
        response.EnsureSuccessStatusCode();
    }
}