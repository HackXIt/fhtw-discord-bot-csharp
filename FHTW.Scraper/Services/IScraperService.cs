using FHTW.Scraper.Scrapers;

namespace FHTW.Scraper.Services;

public interface IScraperService
{
    Task<ScrapeResult> Scrape(IScrapeArguments args);
}