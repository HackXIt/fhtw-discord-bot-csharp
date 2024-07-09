using BIC_FHTW.Scraper.Services;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.Scraper;

public static class ScraperUtilities
{
    public static HttpClient CreateHttpClient(ScraperSettings scraperSettings)
    {
        var handler = new HttpClientHandler();
        handler.UseCookies = scraperSettings.UseCookies; // Enable cookie management
        handler.AllowAutoRedirect = scraperSettings.AllowAutoRedirect; // Don't automatically follow redirects
        handler.AutomaticDecompression = scraperSettings.AutomaticDecompression;
        return new HttpClient(handler);
    }
    public static ScraperService CreateScraperService(IServiceProvider provider, ILogger<ScraperService> logger,
        HttpClient httpClient, ScraperSettings scraperSettings)
    {
        return new ScraperService(provider, logger, httpClient, scraperSettings);
    }
}