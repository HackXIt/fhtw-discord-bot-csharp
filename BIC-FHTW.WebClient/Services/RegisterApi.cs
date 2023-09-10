using System;
using System.Net.Http;
using System.Threading.Tasks;
using BIC_FHTW.WebClient.Services.Core;

namespace BIC_FHTW.WebClient.Services;

public class RegisterApi : IRegisterApi
{
    private readonly HttpClient _httpClient;
    
    public RegisterApi(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task Register(string mailAddress)
    {
        throw new NotImplementedException();
    }
}