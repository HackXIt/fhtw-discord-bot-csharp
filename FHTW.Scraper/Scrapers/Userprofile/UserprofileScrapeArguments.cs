namespace BIC_FHTW.Scraper.Scrapers.Userprofile;

public class UserprofileScrapeArguments : BaseScrapeArgument
{
    public string RequestedUserId { get; }
    
    public UserprofileScrapeArguments(string requestedUserId) : base(ScrapeType.Userprofile)
    {
        RequestedUserId = requestedUserId;
    }

    public override string ConstructUri(string baseUrl, string relativeUrl)
    {
        return baseUrl + relativeUrl + $"uid={RequestedUserId}";
    }
}