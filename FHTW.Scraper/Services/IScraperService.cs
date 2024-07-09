using BIC_FHTW.Scraper.Scrapers;

namespace BIC_FHTW.Scraper.Services;

public interface IScraperService
{
    Task<ScrapeResult> Scrape(IScrapeArguments args);
}