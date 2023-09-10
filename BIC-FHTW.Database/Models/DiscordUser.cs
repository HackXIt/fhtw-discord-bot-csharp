using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIC_FHTW.Database.Models;

public class DiscordUser
{
    public DiscordUser(ulong discordUserId, string activationToken, UserStatus status)
    {
        DiscordUserId = discordUserId;
        ActivationToken = activationToken;
        Status = status;
    }

    [Key]
    public ulong DiscordUserId { get; set; }
    // ReSharper disable once InconsistentNaming
    public string? StudentMail { get; set; }  // Foreign Key for Student
    public Student? Student { get; set; }  // Navigation Property
    [Required]
    public string ActivationToken { get; set; }
    [Required]
    public UserStatus Status { get; set; }
    
    public enum UserStatus
    {
        Active,
        Pending,
        Inactive
    }
    
    public ICollection<DiscordUserRole> DiscordUserRoles { get; set; }
}