namespace BIC_FHTW.DiscordBot;

public class BotSettings
{
    public ulong OwnerId { get; set; }
    public string BotToken { get; set; } = string.Empty;
    public string Prefix { get; set; } = "bic!";
    public int MessageCacheSize { get; set; }
    public bool AlwaysDownloadUsers { get; set; }
    public bool CaseSensitiveComands { get; set; }
    public bool UseMentionPrefix { get; set; }
    public bool AutocompleteHandlers { get; set; }
    public string ValidMailDomain { get; set; } = string.Empty;
    public string WebApiUrl { get; set; } = string.Empty;
    public string RegistrationSubUrl { get; set; } = "api/registration";
    public string BotName { get; set; } = "BIC-FHTW";
    public string BotMail { get; set; } = "no-reply@bic-fhtw.com";
    public ulong GuildId { get; set; }
}