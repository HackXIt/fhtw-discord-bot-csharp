using BIC_FHTW.DiscordBot.Services;
using BIC_FHTW.WebApp.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using NLog.Web;

namespace BIC_FHTW.WebApp;

public class Startup
{
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
}