using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class TagApi : ITagApi
{
    public const string ApiSubPath = "tags";
    private readonly HttpClient _client;

    public TagApi(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<TagMetaDataDTO[]> GetTagMetaDataAsync() =>
        await _client.GetFromJsonAsync<TagMetaDataDTO[]>($"{Constants.botApiPath}/{ApiSubPath}");

    public async Task<UserTagListDTO> GetUserTaglistAsync() =>
        await _client.GetFromJsonAsync<UserTagListDTO>($"{Constants.botApiPath}/{ApiSubPath}/userlist");

    public async Task SubscribeAsync(string[] tags) =>
        await _client.PostAsJsonAsync($"{Constants.botApiPath}/{ApiSubPath}/subscribe", tags);

    public async Task UnsubscribeAsync(string[] tags) =>
        await _client.PostAsJsonAsync($"{Constants.botApiPath}/{ApiSubPath}/unsubscribe", tags);

    public async Task BlacklistAsync(string[] tags) =>
        await _client.PostAsJsonAsync($"{Constants.botApiPath}/{ApiSubPath}/blacklist", tags);

    public async Task UnblacklistAsync(string[] tags) =>
        await _client.PostAsJsonAsync($"{Constants.botApiPath}/{ApiSubPath}/unblacklist", tags);

}