using System;

namespace FHTW.DiscordBot;

public class BotSettings
{
    public ulong OwnerId { get; set; } = Convert.ToUInt64(Environment.GetEnvironmentVariable("BOTSETTINGS_OWNERID"));
    public string BotToken { get; set; } = Environment.GetEnvironmentVariable("BOTSETTINGS_TOKEN") ?? string.Empty;
    public string Prefix { get; set; } = "/";
    public int MessageCacheSize { get; set; }
    public bool AlwaysDownloadUsers { get; set; }
    public bool CaseSensitiveComands { get; set; }
    public bool UseMentionPrefix { get; set; }
    public bool AutocompleteHandlers { get; set; }
    public string ValidMailDomain { get; set; } = string.Empty;
    public string WebApiUrl { get; set; } = string.Empty;
    public string RegistrationSubUrl { get; set; } = "api/registration";
    public string BotName { get; set; } = "BIC-FHTW";
    public string BotMail { get; set; } = Environment.GetEnvironmentVariable("BOTSETTING_BOTMAIL") ?? string.Empty;
    public ulong GuildId { get; set; } = Convert.ToUInt64(Environment.GetEnvironmentVariable("BOTSETTINGS_GUILDID"));
}