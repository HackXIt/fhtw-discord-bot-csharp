using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace BIC_FHTW.Database.Models;

public class DiscordUser
{
    public DiscordUser(ulong discordUserId, string activationToken, string activationMail, UserStatus status)
    {
        DiscordUserId = discordUserId;
        ActivationToken = activationToken;
        ActivationMail = activationMail;
        Status = status;
    }

    [Key]
    public ulong DiscordUserId { get; set; }
    // ReSharper disable once InconsistentNaming
    public string? StudentUid { get; set; }  // Foreign Key for Student
    public Student? Student { get; set; }  // Navigation Property
    
    [Required]
    public string ActivationToken { get; set; }
    
    [Required]
    public string ActivationMail { get; set; }
    
    [NotMapped]
    public MailAddress Email
    {
        get => new(ActivationMail);
        set => ActivationMail = value.ToString();
    }
    
    [Required]
    public UserStatus Status { get; set; }
    
    public enum UserStatus
    {
        Active,
        Pending,
        Inactive
    }
    
    public ICollection<DiscordUserRole>? DiscordUserRoles { get; set; }
}