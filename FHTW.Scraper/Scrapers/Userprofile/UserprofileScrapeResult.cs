namespace BIC_FHTW.Scraper.Scrapers.Userprofile;

public class UserprofileScrapeResult : IScrapeResult
{
    public bool Success { get; set; }
    public string? Username { get; set; }
    public string? Matrikelnummer { get; set; }
    public string? Anrede { get; set; }
    public string? Titel { get; set; }
    public string? Vorname { get; set; }
    public string? Nachname { get; set; }
    public string? Email { get; set; }
    public string? Studiengang { get; set; }
    public int Semester { get; set; } = -1;
    public char? Verband { get; set; }
    public int Gruppe { get; set; } = -1;
    
}