namespace FHTW.Scraper.Scrapers.Query;

public class QueryScrapeArguments : BaseScrapeArgument
{
    public string Query { get; }
    public string RequestedTableId { get; }
    public string RequestedDataColumn { get; }
    public string RequestedSearchTarget { get; }

    public QueryScrapeArguments(string requestedTable, string query, string requestedDataColumn, string requestedSearchTarget) : base(ScrapeType.Query)
    {
        RequestedTableId = requestedTable;
        Query = query;
        RequestedDataColumn = requestedDataColumn;
        RequestedSearchTarget = requestedSearchTarget;
    }

    public override string ConstructUri(string baseUrl, string relativeUrl)
    {
        return baseUrl + relativeUrl + $"search={Query}";
    }
}