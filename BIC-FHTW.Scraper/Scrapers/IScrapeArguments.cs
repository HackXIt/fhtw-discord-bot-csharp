namespace BIC_FHTW.Scraper.Scrapers;

public interface IScrapeArguments
{
    ScrapeType ScrapeType { get; }
    string ConstructUri(string baseUrl, string relativeUrl);
}