namespace BIC_FHTW.Scraper.Scrapers.MailGroup;

public class MailGroupScrapeArguments : BaseScrapeArgument
{
    public MailGroupScrapeType MailGroupScrapeType { get; }
    public string RequestedDataColumn { get; }
    public Dictionary<string, string> QueryParameters { get; } = new();
    
    public MailGroupScrapeArguments(MailGroupScrapeType mailGroupScrapeType, string requestedDataColumn) : base(ScrapeType.MailGroup)
    {
        MailGroupScrapeType = mailGroupScrapeType;
        RequestedDataColumn = requestedDataColumn;
    }

    public override string ConstructUri(string baseUrl, string relativeUrl)
    {
        return baseUrl + relativeUrl + string.Join("&", QueryParameters.Select(x => $"{x.Key}={x.Value}"));
    }
}