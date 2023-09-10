using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIC_FHTW.Database.Models;

public class RequestableRole
{
    [Key]
    public ulong RoleId { get; set; }
    [MaxLength(32), Required]
    public string RoleName { get; set; }
    public ulong GuildId { get; set; }

    public ICollection<DiscordUserRole> DiscordUserRoles { get; set; }
}