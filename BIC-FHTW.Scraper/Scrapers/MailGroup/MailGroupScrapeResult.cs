namespace BIC_FHTW.Scraper.Scrapers.MailGroup;

public class MailGroupScrapeResult : IScrapeResult
{
    public bool Success { get; set; } = false;
    public List<string>? Headers { get; set; }
    public string? RequestedColumn { get; set; }
    public int RequestedColumnIndex { get; set; } = -1;
    public List<string>? RequestedData { get; set; }
    public List<Dictionary<string, string>>? Data { get; set; }
}