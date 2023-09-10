using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using BIC_FHTW.Database.DatabaseContexts;
using BIC_FHTW.Database.Services;
using BIC_FHTW.DiscordBot;
using BIC_FHTW.DiscordBot.Services;
using BIC_FHTW.Shared;
// using BIC_FHTW.WebApp.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.WebApp;

public class Startup
{
   public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        var botSettings = Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>();
        var authenticationSettings = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();
        var connectionString = Configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty or null");
        }

        services.AddSingleton(botSettings);

        services.AddSingleton(BotUtilities.CreateDicordWebsocketClient(botSettings));
        services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());

        services.AddSingleton(BotUtilities.CreateCommandService(botSettings));
        //services.AddHostedService<BotService>();
        services.AddHostedService(provider => 
        {
            var discordSocketClient = provider.GetRequiredService<DiscordSocketClient>();
            var commandService = provider.GetRequiredService<CommandService>();
            var botSettings = provider.GetRequiredService<BotSettings>();
            var logger = provider.GetRequiredService<ILogger<BotService>>();

            // Explicitly provide other arguments here, if any
            return new BotService(provider, discordSocketClient, commandService, botSettings, logger);
        });

        services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlite(connectionString, b => b.MigrationsAssembly("BIC-FHTW.Database"))
                .EnableDetailedErrors() // Enable detailed errors
                .EnableSensitiveDataLogging() // Enable sensitive data logging
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())); // Add logging
        });
        
        MigrateDatabase(connectionString);

        services.AddScoped<UserRepositoryManager>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<RequestableRoleManager>();
        services.AddScoped<IRoleService, RoleService>();


        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.ExpireTimeSpan = new System.TimeSpan(7, 0, 0, 0);
            })
            .AddDiscord(options =>
            {
                options.ClientId = authenticationSettings.Discord.ClientId;
                options.ClientSecret = authenticationSettings.Discord.ClientSecret;

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = ticketContext =>
                    {
                        if (ulong.TryParse(ticketContext.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
                                out var userId))
                        {
                            if (userId == botSettings.OwnerId)
                            {
                                ticketContext.Principal?.AddIdentity(new ClaimsIdentity(new[]
                                    { new Claim(Constants.IsBotOwner, "true") }));
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.IsBotOwner, policy => policy.RequireClaim(Constants.IsBotOwner, "true"));
        });

        services.AddOptions();
        services.AddMemoryCache();
        services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
        services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
        services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddControllersWithViews();
        services.AddRazorPages();
    }
    
    /*
            .AddOAuth("Discord",
                options =>
                {
                    options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
                    options.Scope.Add("identify");
                    options.CallbackPath = new PathString("/auth/oauth2Callback");
                    options.ClientId = authenticationSettings.Discord.ClientId;
                    options.ClientSecret = authenticationSettings.Discord.ClientSecret;
                    options.TokenEndpoint = "https://discord.com/api/oauth2/token";
                    options.UserInformationEndpoint = "https://discord.com/api/users/@me";
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                    // For nested json objects, use MapJsonSubKey
                    //options.ClaimActions.MapJsonSubKey("...", "someObject", "id");
                    options.AccessDeniedPath = "/Home/DiscordAuthFailed";
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                            context.RunClaimActions(user);
                        }
                    };
            })
            */

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseClientRateLimiting();
        app.UseIpRateLimiting();

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
        });
    }

    private void MigrateDatabase(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ApplicationContext>();
        builder.UseSqlite(connectionString, b => b.MigrationsAssembly("BIC-FHTW.Database"))
            .EnableDetailedErrors() // Enable detailed errors
            .EnableSensitiveDataLogging() // Enable sensitive data logging
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())); // Add logging;

        using var context = new ApplicationContext(builder.Options);
        context.Database.Migrate();
    }
}
/*
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<DatabaseService>();
    services.AddSingleton<DiscordBot.DiscordBot>();
    services.AddMailKit(config =>
    {
        config.UseMailKit(new MailKitOptions
        {
            Server = "smtp.example.com",
            Port = 587,
            SenderName = "your-email@example.com",
            SenderEmail = "your-email@example.com",
            Account = "your-email@example.com",
            Password = "your-email-password",
            Security = true
        });
    });
    
    services.AddControllers();
}

public static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Trace);
        })
        .UseNLog();
}
*/