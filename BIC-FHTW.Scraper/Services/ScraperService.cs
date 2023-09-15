using System.Text.RegularExpressions;
using BIC_FHTW.Scraper.Scrapers;
using BIC_FHTW.Scraper.Scrapers.MailGroup;
using BIC_FHTW.Scraper.Scrapers.MailGroups;
using BIC_FHTW.Scraper.Scrapers.Query;
using BIC_FHTW.Scraper.Scrapers.Userprofile;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.Scraper.Services;

public class ScraperService : IScraperService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ScraperService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ScraperSettings _scraperSettings;

    public ScraperService(IServiceProvider? provider, ILogger<ScraperService> logger, HttpClient? httpClient, ScraperSettings? scraperSettings)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _scraperSettings = scraperSettings ?? throw new ArgumentNullException(nameof(scraperSettings));
        
        ConfigureClient();
    }
    
    private void ConfigureClient()
    {
        var random = new Random(); // random number generator for random selection of user agent
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _scraperSettings.UserAgents[random.Next(0, _scraperSettings.UserAgents.Length)]);
        _httpClient.DefaultRequestHeaders.Add("Accept", _scraperSettings.AcceptHeader);
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", _scraperSettings.AcceptLanguageHeader);
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", _scraperSettings.AcceptEncodingHeader);
        _httpClient.DefaultRequestHeaders.Add("Connection", _scraperSettings.ConnectionHeader);
        _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", _scraperSettings.UpgradeInsecureRequestsHeader);
        _httpClient.DefaultRequestHeaders.Referrer = new Uri(_scraperSettings.BaseUrl);
        // Add client credentials for login and authorization
        var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(_scraperSettings.Username + ":" + _scraperSettings.Password));
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
    }

    public async Task<ScrapeResult> Scrape(IScrapeArguments args)
    {
        var result = new ScrapeResult();
        try
        {
            switch (args.ScrapeType)
            {
                case ScrapeType.MailGroup:
                    result.MailGroupScrapeResult = await ScrapeMailGroup(args as MailGroupScrapeArguments);
                    result.Success = result.MailGroupScrapeResult.Success;
                    return result;
                case ScrapeType.Query:
                    result.QueryScrapeResult = await ScrapeQuery(args as QueryScrapeArguments);
                    result.Success = result.QueryScrapeResult.Success;
                    return result;
                case ScrapeType.Userprofile:
                    result.UserprofileScrapeResult = await ScrapeUserprofile(args as UserprofileScrapeArguments);
                    result.Success = result.UserprofileScrapeResult.Success;
                    return result;
                case ScrapeType.MailGroups:
                    result.MailGroupsScrapeResult = await ScrapeMailGroups(args as MailGroupsScrapeArguments);
                    result.Success = result.MailGroupsScrapeResult.Success;
                    return result;
                default:
                    throw new NotImplementedException($"Scrape type {args.ScrapeType} not implemented.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Scraping failed: {}", ex.Message);
            throw;
        }
    }
    
    private async Task<MailGroupScrapeResult> ScrapeMailGroup(MailGroupScrapeArguments? args)
    {
        if (args == null)
            throw new ArgumentException($"Scrape arguments {nameof(args)} must not be null.");
        var result = new MailGroupScrapeResult();
        var targetUri = "";
        // Make the GET request to the target URL
        if (args.MailGroupScrapeType == MailGroupScrapeType.StudentGroup)
        {
            targetUri = args.ConstructUri(_scraperSettings.BaseUrl, _scraperSettings.StudentMailGroupRelativeUrl);
        } else if (args.MailGroupScrapeType == MailGroupScrapeType.PersonGroup)
        {
            targetUri = args.ConstructUri(_scraperSettings.BaseUrl, _scraperSettings.PersonMailGroupRelativeUrl);
        }

        var response = GetResponse(targetUri);
        
        // Verify the response (return on failure)
        if (response.Result is not { success: true, document: not null }) return await Task.FromResult(result);
        
        // Find the table containing the mailgroup data (return on failure)
        var table = response.Result.document.DocumentNode.SelectSingleNode("//table[@id='table']");
        if (table == null) return await Task.FromResult(result);
        
        // Find the headers of the table
        result.Headers = table.SelectNodes(".//thead//th").Select(header => header.InnerText.Trim()).ToList();
        _logger.LogDebug("Mailgroup headers: {}", 
            string.Join(",", result.Headers.Select((header, index) => $"{index}={header}")));
        
        // Extract the data
        result.Data = GetTableData(result.Headers, table.SelectNodes(".//tbody//tr"));
        
        // Find the index of the requested column by name (throws exception when args[0] not found)
        var foundColumn = result.Headers.FindIndex(header => string.Equals(header, args.RequestedDataColumn, StringComparison.OrdinalIgnoreCase));
        if (foundColumn == -1)
            throw new KeyNotFoundException($"Requested column {args.RequestedDataColumn} not found.");
        result.RequestedColumn = result.Headers[foundColumn];
        result.RequestedColumnIndex = foundColumn;
        _logger.LogDebug("Requested column: {}={}", result.RequestedColumnIndex, result.RequestedColumn);
        
        // Store requested result
        result.RequestedData = result.Data
            .Where(row => row.ContainsKey(result.RequestedColumn))
            .Select(row => row[result.RequestedColumn])
            .ToList();
        
        result.Success = true;
        return await Task.FromResult(result);
    }

    private async Task<MailGroupsScrapeResult> ScrapeMailGroups(MailGroupsScrapeArguments? args)
    {
        if (args == null)
            throw new ArgumentException($"Scrape arguments {nameof(args)} must not be null.");
        throw new NotImplementedException();
    }

    private async Task<QueryScrapeResult> ScrapeQuery(QueryScrapeArguments? args)
    {
        if (args == null)
            throw new ArgumentException($"Scrape arguments {nameof(args)} must not be null.");
        var result = new QueryScrapeResult();
        // Make the GET request to the target URL
        var response = GetResponse(args.ConstructUri(_scraperSettings.BaseUrl, _scraperSettings.QueryRelativeUrl));

        // Verify the response (return on failure)
        if (response.Result is not { success: true, document: not null }) return await Task.FromResult(result);
        
        // Find the requested table containing the query data (return on failure)
        var table = response.Result.document.DocumentNode.SelectSingleNode($"//table[@id='{args.RequestedTableId}']");
        if (table == null) return await Task.FromResult(result);
        
        // Find the headers of the table
        result.Headers = table.SelectNodes(".//thead//th").Select(header => header.InnerText.Trim()).ToList();
        _logger.LogDebug("Query table headers: {}", 
            string.Join(",", result.Headers.Select((header, index) => $"{index}={header}")));
        
        // Extract the data
        result.Data = GetTableData(result.Headers, table.SelectNodes(".//tbody//tr"));
        
        // Find the index of the requested column by name (throws exception when args[0] not found)
        var foundColumn = result.Headers.FindIndex(header => string.Equals(header, args.RequestedDataColumn, StringComparison.OrdinalIgnoreCase));
        if (foundColumn == -1)
            throw new KeyNotFoundException($"Requested column {args.RequestedDataColumn} not found.");
        result.RequestedColumn = result.Headers[foundColumn];
        result.RequestedColumnIndex = foundColumn;
        _logger.LogDebug("Requested column: {}={}", result.RequestedColumnIndex, result.RequestedColumn);
        
        // Store requested result
        result.RequestedData = result.Data
            .Where(row => row.ContainsKey(result.RequestedColumn))
            .Select(row => row[result.RequestedColumn])
            .Where(entry => entry.Contains(args.RequestedSearchTarget, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        result.Success = true;
        return await Task.FromResult(result);
    }
    
    private async Task<UserprofileScrapeResult> ScrapeUserprofile(UserprofileScrapeArguments? args)
    {
        if (args == null)
            throw new ArgumentException($"Scrape arguments {nameof(args)} must not be null.");
        var result = new UserprofileScrapeResult();
        // Make the GET request to the target URL
        var response = GetResponse(args.ConstructUri(_scraperSettings.BaseUrl, _scraperSettings.UserprofileRelativeUrl));

        // Verify the response (return on failure)
        if (response.Result is not { success: true, document: not null }) return await Task.FromResult(result);
        
        // Find the cmstable containing the user data (return on failure)
        var table = response.Result.document.DocumentNode.SelectSingleNode($"//table[@class='cmstable']");
        if (table == null) return await Task.FromResult(result);
        
        // Find the cmscontent containing the user data (return on failure)
        var cmscontent = table.SelectSingleNode(".//td[@class='cmscontent']");
        if (cmscontent == null) return await Task.FromResult(result);
        
        // Find the email address if available on the page
        var emailData = cmscontent.SelectSingleNode(".//table/tr/td[3]/a");
        if(emailData != null) result.Email = emailData.InnerText.Trim();
        
        // Find the user data entry in table (return on failure)
        var userData = cmscontent.SelectSingleNode(".//table/tr/td[2]");
        if (userData == null) return await Task.FromResult(result);
        
        // Extract the data
        ScrapeUserData(result, userData.InnerHtml);
        
        return await Task.FromResult(result);
    }
    
    private async Task<(HtmlDocument? document, bool success)> GetResponse(string targetUri)
    {
        // Make the GET request to the target URL
        var getResponse = await _httpClient.GetAsync(targetUri);

        // Process the GET response
        if (getResponse.IsSuccessStatusCode)
        {
            var content = await getResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("GET request to {} successful.", targetUri);
            var document = new HtmlDocument();
            document.LoadHtml(content);
            return await Task.FromResult<(HtmlDocument? document, bool success)>((document, true));
        }
        _logger.LogInformation("GET request to {} failed with code {}. Meaning: {}", 
            targetUri, getResponse.StatusCode, getResponse.ReasonPhrase);
        return await Task.FromResult<(HtmlDocument? document, bool success)>((null, false));
    }
    
    private static List<Dictionary<string, string>> GetTableData(IReadOnlyList<string> headers, HtmlNodeCollection tableData)
    {
        var result = new List<Dictionary<string, string>>();
        for (var i = 0; i < tableData.Count; i++)
        {
            var rowData = tableData[i].SelectNodes(".//td").Select(d => d.InnerText.Trim()).ToList();
            result.Add(new Dictionary<string, string>());
            for (var j = 0; i < headers.Count; i++)
            {
                result[i].Add(headers[j], rowData[j]);
            }
        }

        return result;
    }
    
    private static void ScrapeUserData(UserprofileScrapeResult result, string userData)
    {
        Regex regex;
        Match match;

        // Username
        regex = new Regex(@"Username:\s*(\w+)\s*<");
        match = regex.Match(userData);
        result.Username = match.Success ? match.Groups[1].Value : null;

        // Anrede
        regex = new Regex(@"Anrede:\s*(\w+)\s*<");
        match = regex.Match(userData);
        result.Anrede = match.Success ? match.Groups[1].Value : null;

        // Titel
        regex = new Regex(@"Titel:\s*([\w.\s]+)\s*<");
        match = regex.Match(userData);
        result.Titel = match.Success ? match.Groups[1].Value : null;

        // Vorname
        regex = new Regex(@"Vorname:\s*(\w+)\s*<");
        match = regex.Match(userData);
        result.Vorname = match.Success ? match.Groups[1].Value : null;

        // Nachname
        regex = new Regex(@"Nachname:\s*(\w+)\s*<");
        match = regex.Match(userData);
        result.Nachname = match.Success ? match.Groups[1].Value : null;

        // Studiengang
        regex = new Regex(@"Studiengang:\s*([\w- ]+)\s*<");
        match = regex.Match(userData);
        result.Studiengang = match.Success ? match.Groups[1].Value : null;

        // Semester
        regex = new Regex(@"Semester:\s*(\d+)\s*<");
        match = regex.Match(userData);
        result.Semester = match.Success ? Int32.Parse(match.Groups[1].Value) : -1;

        // Verband
        regex = new Regex(@"Verband:\s*(\w+)\s*<");
        match = regex.Match(userData);
        result.Verband = match.Success && !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value[0] : (char?)null;

        // Gruppe
        regex = new Regex(@"Gruppe:\s*(\d+)\s*<");
        match = regex.Match(userData);
        result.Gruppe = match.Success && !string.IsNullOrEmpty(match.Groups[1].Value) ? Int32.Parse(match.Groups[1].Value) : -1;
        
        result.Success = true;
    }

    private static HtmlNode GetTargetNode(string htmlContent, string searchValue, string targetNode, string subTarget = "", string subTargetType = "")
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var node = document.DocumentNode.SelectSingleNode($"//{targetNode}[@{subTargetType}='{subTarget}']");
        return node;
    }
}