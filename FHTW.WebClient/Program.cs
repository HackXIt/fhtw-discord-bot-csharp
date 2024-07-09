using System;
using System.Net.Http;
using System.Threading.Tasks;
using FHTW.Shared;
using FHTW.WebClient.Services;
using FHTW.WebClient.Services.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FHTW.WebClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");

        builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddOptions();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<DiscordAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider, DiscordAuthenticationStateProvider>((provider) => provider.GetRequiredService<DiscordAuthenticationStateProvider>());
        builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
        builder.Services.AddScoped<IRoleApi, RoleApi>();
        builder.Services.AddScoped<ITagApi, TagApi>();
        builder.Services.AddScoped<IRegisterApi, RegisterApi>();
        
        builder.Services.AddAuthorizationCore(config =>
        {
            config.AddPolicy(Constants.IsBotOwner, policy => policy.RequireClaim(Constants.IsBotOwner, "true"));
        });

        await builder.Build().RunAsync();
    }
}