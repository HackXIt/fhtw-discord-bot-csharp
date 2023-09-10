namespace BIC_FHTW.Database.Models;

public class DiscordUserRole
{
    public ulong DiscordUserId { get; set; }
    public DiscordUser DiscordUser { get; set; }

    public ulong RoleId { get; set; }
    public RequestableRole RequestableRole { get; set; }
}