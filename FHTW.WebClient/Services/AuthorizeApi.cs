using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class AuthorizeApi : IAuthorizeApi
{
    private readonly HttpClient _httpClient;

    public AuthorizeApi(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<DiscordUserDTO> GetUserInfo() =>
        await _httpClient.GetFromJsonAsync<DiscordUserDTO>($"{Constants.botApiPath}/authentication/userinfo");

    public async Task Login()
    {
        var result = await _httpClient.GetAsync($"{Constants.botApiPath}/authentication/login");
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }

    public async Task Logout()
    {
        var result = await _httpClient.PostAsync($"{Constants.botApiPath}/Authorize/Logout", null);
        result.EnsureSuccessStatusCode();
    }
}