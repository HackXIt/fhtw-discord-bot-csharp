using System.Net;

namespace BIC_FHTW.Scraper;

public class ScraperSettings
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string MainUrl => BaseUrl + MainPageRelativeUrl;
    public string MainPageRelativeUrl { get; set; } = string.Empty;
    public string LoginRelativeUrl { get; set; } = string.Empty;
    public string LogoutRelativeUrl { get; set; } = string.Empty;
    public string PostLoginRelativeUrl { get; set; } = string.Empty;
    public string MailGroupsRelativeUrl { get; set; } = string.Empty;
    public string PersonMailGroupRelativeUrl { get; set; } = string.Empty;
    public string StudentMailGroupRelativeUrl { get; set; } = string.Empty;
    public string QueryRelativeUrl { get; set; } = string.Empty;
    public string UserprofileRelativeUrl { get; set; } = string.Empty;
    // See https://www.zenrows.com/blog/user-agent-web-scraping#why-is-a-user-agent-important-for-web-scraping
    public string[] UserAgents { get; set; } = 
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Safari/605.1.15",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Safari/605.1.15",
    };
    // https://www.zenrows.com/blog/web-scraping-headers#what-are-http-headers
    public string AcceptHeader { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8";
    public string AcceptLanguageHeader { get; set; } = "de,en-US;q=0.7,en;q=0.3";
    public string AcceptEncodingHeader { get; set; } = "gzip, deflate, br";
    public string ConnectionHeader { get; set; } = "keep-alive";
    public string UpgradeInsecureRequestsHeader { get; set; } = "1";
    public string RefererHeader => BaseUrl;
    public bool UseCookies { get; set; } = true;
    public bool AllowAutoRedirect { get; set; } = false;
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip | DecompressionMethods.Deflate;
}