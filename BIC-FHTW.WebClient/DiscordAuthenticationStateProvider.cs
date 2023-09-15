using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BIC_FHTW.Shared;
using BIC_FHTW.WebClient.Services.Core;
using Microsoft.AspNetCore.Components.Authorization;

namespace BIC_FHTW.WebClient;

public class DiscordAuthenticationStateProvider : AuthenticationStateProvider
{
    private DiscordUserDTO _discordUserCache;
    private readonly IAuthorizeApi _authorizeApi;

    public bool HasValidInfoCache => _discordUserCache != null && _discordUserCache.IsAuthenticated;
    public string Username => _discordUserCache?.Username;
    public string AvatarHash => _discordUserCache?.AvatarHash;
    public ulong UserId => _discordUserCache?.UserId ?? default;
    public IReadOnlyDictionary<string, string> Claims => _discordUserCache?.Claims;

    public DiscordAuthenticationStateProvider(IAuthorizeApi authorizeApi)
    {
        _authorizeApi = authorizeApi ?? throw new ArgumentNullException(nameof(authorizeApi));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var userInfo = await GetUserInfoAsync();
            if (userInfo.IsAuthenticated)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,_discordUserCache.UserId.ToString()),
                    new Claim(ClaimTypes.Name,_discordUserCache.Username)
                };
                claims.AddRange(userInfo.Claims.Select(c => new Claim(c.Key, c.Value)));
                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request failed: {e}");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private async Task<DiscordUserDTO> GetUserInfoAsync()
    {
        if (_discordUserCache != null && _discordUserCache.IsAuthenticated)
            return _discordUserCache;

        _discordUserCache = await _authorizeApi.GetUserInfo();
        return _discordUserCache;
    }
}