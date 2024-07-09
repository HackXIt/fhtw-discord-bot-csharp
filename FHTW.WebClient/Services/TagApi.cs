using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class TagApi : ITagApi
{
    private readonly HttpClient _client;

    public TagApi(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<TagMetaDataDTO[]> GetTagMetaDataAsync() =>
        await _client.GetFromJsonAsync<TagMetaDataDTO[]>("api/bic-fhtw/tags");

    public async Task<UserTagListDTO> GetUserTaglistAsync() =>
        await _client.GetFromJsonAsync<UserTagListDTO>("api/bic-fhtw/tags/userlist");

    public async Task SubscribeAsync(string[] tags) =>
        await _client.PostAsJsonAsync("api/bic-fhtw/tags/subscribe", tags);

    public async Task UnsubscribeAsync(string[] tags) =>
        await _client.PostAsJsonAsync("api/bic-fhtw/tags/unsubscribe", tags);

    public async Task BlacklistAsync(string[] tags) =>
        await _client.PostAsJsonAsync("api/bic-fhtw/tags/blacklist", tags);

    public async Task UnblacklistAsync(string[] tags) =>
        await _client.PostAsJsonAsync("api/bic-fhtw/tags/unblacklist", tags);

}