using FHTW.Scraper.Scrapers;
using FHTW.Scraper.Scrapers.MailGroup;
using FHTW.Scraper.Scrapers.MailGroups;
using FHTW.Scraper.Scrapers.Query;
using FHTW.Scraper.Scrapers.Userprofile;

namespace FHTW.Scraper;

public class ScrapeResult : IScrapeResult
{
    public bool Success { get; set; }
    public MailGroupScrapeResult? MailGroupScrapeResult { get; set; }
    public MailGroupsScrapeResult? MailGroupsScrapeResult { get; set; }
    public QueryScrapeResult? QueryScrapeResult { get; set; }
    public UserprofileScrapeResult? UserprofileScrapeResult { get; set; }
}