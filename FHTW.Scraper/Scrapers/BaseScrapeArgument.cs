namespace FHTW.Scraper.Scrapers;

public abstract class BaseScrapeArgument : IScrapeArguments
{
    public ScrapeType ScrapeType { get; }
    public abstract string ConstructUri(string baseUrl, string relativeUrl);

    protected BaseScrapeArgument(ScrapeType scrapeType)
    {
        ScrapeType = scrapeType;
    }
}