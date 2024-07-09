namespace FHTW.Scraper.Scrapers.MailGroups;

public class MailGroupsScrapeArguments : BaseScrapeArgument
{
    public Dictionary<string, string> Arguments { get; set; } = new();
    
    public MailGroupsScrapeArguments(string requestedColumn, string mailGroup) : base(ScrapeType.MailGroups)
    {
    }

    public override string ConstructUri(string baseUrl, string relativeUrl)
    {
        throw new NotImplementedException();
    }
}