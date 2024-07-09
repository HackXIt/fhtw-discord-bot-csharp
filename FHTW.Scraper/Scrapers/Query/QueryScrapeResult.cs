namespace FHTW.Scraper.Scrapers.Query;

public class QueryScrapeResult : IScrapeResult
{
    public bool Success { get; set; }
    public List<string>? Headers { get; set; }
    public string? RequestedColumn { get; set; }
    public int RequestedColumnIndex { get; set; } = -1;
    public List<string>? RequestedData { get; set; }
    public List<Dictionary<string, string>>? Data { get; set; }
}