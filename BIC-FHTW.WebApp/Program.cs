using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using BIC_FHTW.WebApp;
using BIC_FHTW.WebApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ...

class Program
{
    static async Task Main(string[] args)
    {
        var databaseManager = new DatabaseService();
        //var emailSender = new EmailSender("smtp.example.com", 587, "your-email@example.com", "your-email-password");
        // DiscordBot needs to be injected ?
        
        // Create and configure the web application host
        var hostBuilder = new HostBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(databaseManager);
                services.AddSingleton(discordBot.Client);
            });

        // Build and run the web application
        await hostBuilder.Build().RunAsync();
    }
}