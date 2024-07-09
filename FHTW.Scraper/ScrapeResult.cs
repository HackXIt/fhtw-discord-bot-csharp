using BIC_FHTW.Scraper.Scrapers;
using BIC_FHTW.Scraper.Scrapers.MailGroup;
using BIC_FHTW.Scraper.Scrapers.MailGroups;
using BIC_FHTW.Scraper.Scrapers.Query;
using BIC_FHTW.Scraper.Scrapers.Userprofile;

namespace BIC_FHTW.Scraper;

public class ScrapeResult : IScrapeResult
{
    public bool Success { get; set; }
    public MailGroupScrapeResult? MailGroupScrapeResult { get; set; }
    public MailGroupsScrapeResult? MailGroupsScrapeResult { get; set; }
    public QueryScrapeResult? QueryScrapeResult { get; set; }
    public UserprofileScrapeResult? UserprofileScrapeResult { get; set; }
}