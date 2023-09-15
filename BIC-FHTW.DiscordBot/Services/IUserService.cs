using System.Threading.Tasks;
using BIC_FHTW.Database.Models;
using BIC_FHTW.Scraper.Scrapers.Userprofile;
using BIC_FHTW.Shared;

namespace BIC_FHTW.DiscordBot.Services;

public interface IUserService
{
    Task<DiscordUserDTO> AddUserAsync(ulong discordUserId, string activationToken, string activationMail);
    Task<DiscordUserDTO> GetUserByDiscordIdAsync(ulong discordUserId);
    Task<DiscordUserDTO?> GetUserByActivationTokenAsync(string activationToken);
    Task<DiscordUserDTO?> GetUserByStudentUidAsync(string studentUid);
    Task UpdateUserStatusAsync(DiscordUser discordUser, DiscordUser.UserStatus newStatus);
    Task<DiscordUserDTO> LinkStudentToDiscordUserAsync(string? studentUid, ulong discordUserId);
    Task<StudentDTO?> GetStudentByUidAsync(string studentUid);
    Task<StudentDTO?> GetStudentByDiscordIdAsync(ulong discordUserId);
    Task<DiscordUserDTO?> ActivateUserWithStudentInformation(ulong discordUserId, UserprofileScrapeResult scrapeResult);
    // Task<DiscordUser> AddStudentAndActivateUserAsync(ulong discordUserId, Student newStudentData)
    // Add more methods here if necessary
}