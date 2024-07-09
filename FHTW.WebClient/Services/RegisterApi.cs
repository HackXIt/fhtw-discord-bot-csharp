using System;
using System.Net.Http;
using System.Threading.Tasks;
using FHTW.WebClient.Services.Core;

namespace FHTW.WebClient.Services;

public class RegisterApi : IRegisterApi
{
    private readonly HttpClient _httpClient;
    
    public RegisterApi(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }


    public async Task Register(string token)
    {
        var result = await _httpClient.PostAsync($"api/bic-fhtw/register/complete-registration?token={token}", null);
        result.EnsureSuccessStatusCode();
    }
}