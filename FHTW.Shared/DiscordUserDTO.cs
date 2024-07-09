using System.Collections.Generic;
using System.Net.Mail;

namespace FHTW.Shared;

public class DiscordUserDTO
{
    public bool IsAuthenticated { get; set; }
    public string Token { get; set; }
    public string Username { get; set; }
    public MailAddress? MailAddress { get; set; }
    public string MailUsername => MailAddress?.User ?? string.Empty;
    public ulong UserId { get; set; }
    public string AvatarHash { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}