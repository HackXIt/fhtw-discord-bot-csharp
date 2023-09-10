using System.Threading.Tasks;
using BIC_FHTW.Database.Models;

namespace BIC_FHTW.DiscordBot.Services;

public interface IUserService
{
    Task<DiscordUser> AddUserAsync(ulong discordUserId, string activationToken);
    Task<DiscordUser> GetUserByDiscordIdAsync(ulong discordUserId);
    Task<DiscordUser> GetUserByEmailAsync(string email);
}