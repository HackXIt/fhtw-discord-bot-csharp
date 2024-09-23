using System;
using System.Net.Http;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class RegisterApi : IRegisterApi
{
    public const string ApiSubPath = "register";
    private readonly HttpClient _httpClient;
    
    public RegisterApi(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }


    public async Task Register(string token)
    {
        var result = await _httpClient.PostAsync($"{Constants.botApiPath}/{ApiSubPath}/complete-registration?token={token}", null);
        result.EnsureSuccessStatusCode();
    }
}