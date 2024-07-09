using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using FHTW.Database.DatabaseContexts;
using FHTW.Database.Services;
using FHTW.DiscordBot;
using FHTW.DiscordBot.Middleware;
using FHTW.DiscordBot.Services;
using FHTW.Scraper;
using FHTW.Scraper.Services;
using FHTW.Shared;
using FHTW.Shared.Services;
using FHTW.ThirdParty.Mail;
using MailKit.Security;
using MailKitSimplified.Sender;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// using BIC_FHTW.WebApp.Services;

namespace FHTW.WebApp;

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
        if(botSettings == null)
            throw new ArgumentNullException(nameof(botSettings), "settings cannot be null");
        var appUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(";").First();
        if(appUrl != null)
        {
            botSettings.WebApiUrl = appUrl;
            botSettings.RegistrationSubUrl = "api/bic-fhtw/registration/complete-registration";
        }
        var scraperSettings = Configuration.GetSection(nameof(ScraperSettings)).Get<ScraperSettings>();
        if(scraperSettings == null)
            throw new ArgumentNullException(nameof(scraperSettings), "settings cannot be null");
        var authenticationSettings = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();
        if(authenticationSettings == null)
            throw new ArgumentNullException(nameof(authenticationSettings), "settings cannot be null");
        var mailSettings = Configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
        if(mailSettings == null)
            throw new ArgumentNullException(nameof(mailSettings), "settings cannot be null");
        var connectionString = Configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty or null");
        }

        // Add settings to DI
        services.AddSingleton(botSettings);
        services.AddSingleton(scraperSettings);
        services.AddSingleton(mailSettings);

        // Add services to DI
        services.AddSingleton<EventService>();
        services.AddSingleton(ScraperUtilities.CreateHttpClient(scraperSettings));

        services.AddSingleton(BotUtilities.CreateDicordWebsocketClient(botSettings));
        services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());

        services.AddSingleton(BotUtilities.CreateCommandService(botSettings));
        services.AddSingleton<InteractionService>(provider =>
        {
            var discordSocketClient = provider.GetRequiredService<DiscordSocketClient>();
            return BotUtilities.CreateInteractionService(discordSocketClient, botSettings);
        });
        services.AddSingleton<IScraperService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ScraperService>>();
            var client = provider.GetRequiredService<HttpClient>();
            return ScraperUtilities.CreateScraperService(provider, logger, client, scraperSettings);
        });
        services.AddHostedService(provider => 
        {
            var logger = provider.GetRequiredService<ILogger<BotService>>();
            var scraperService = provider.GetRequiredService<IScraperService>();
            var discordSocketClient = provider.GetRequiredService<DiscordSocketClient>();
            var commandService = provider.GetRequiredService<CommandService>();
            var interactionService = provider.GetRequiredService<InteractionService>();
            
            // Explicitly provide other arguments here, if any
            return new BotService(provider, logger, scraperService, discordSocketClient, commandService, interactionService, botSettings);
        });
        
        // Simplified Mailkit setup for MailService
        services.AddScopedMailKitSimplifiedEmailSender(Configuration);
        // Overwrite default options of Simplified Mailkit
        services.AddMailKitSimplifiedEmailSender(options =>
        {
            options.SmtpCredential = new NetworkCredential(mailSettings.Account, mailSettings.AppPassword);
            options.SmtpHost = mailSettings.Host;
            options.SmtpPort = (ushort)mailSettings.Port;
            options.SocketOptions = SecureSocketOptions.StartTls;
            options.ProtocolLog = "console";
            options.CreateProtocolLogger();
        });
        
        // Database
        services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlite(connectionString, b => b.MigrationsAssembly("FHTW.Database"))
                .EnableDetailedErrors() // Enable detailed errors
#if DEBUG
                .EnableSensitiveDataLogging() // Enable sensitive data logging
#endif
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())); // Add logging
        });
        MigrateDatabase(connectionString);
        services.AddScoped<UserRepositoryManager>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<RoleManager>();
        services.AddScoped<IRoleService, RoleService>();

        // Interaction middleware for DI
        services.AddSingleton<IInteractionMiddleware, RegisterMiddleware>();
        services.AddSingleton<IInteractionMiddleware, DebugMiddleware>();
        
        // Discord Authentication mechanisms for the web client
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
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (env.Equals("Development"))
                {
                    options.ClientId = authenticationSettings.Discord.ClientId;
                    options.ClientSecret = authenticationSettings.Discord.ClientSecret;
                }
                else
                {
                    options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENTID") ?? string.Empty;
                    options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENTSECRET") ?? string.Empty;
                }
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
    
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // This should be the first middleware
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500; // or whatever status code you want to return
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error; // This will get the exception

                // Log the exception or do something with it here
                // This is a good place to set a breakpoint to see what the exception is
                await context.Response.WriteAsync("A server error occurred."); // You can customize this message
            });
        });
        
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
        builder.UseSqlite(connectionString, b => b.MigrationsAssembly("FHTW.Database"))
            .EnableDetailedErrors() // Enable detailed errors
#if DEBUG
            .EnableSensitiveDataLogging() // Enable sensitive data logging
#endif
            .UseLoggerFactory(LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole())); // Add logging;

        using var context = new ApplicationContext(builder.Options);
        context.Database.Migrate();
    }
}