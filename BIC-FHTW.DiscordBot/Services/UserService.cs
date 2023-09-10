using System;
using System.Threading.Tasks;
using BIC_FHTW.Database.Models;
using BIC_FHTW.Database.Services;
using Discord.WebSocket;

namespace BIC_FHTW.DiscordBot.Services;

public class UserService : IUserService
{
    private readonly DiscordSocketClient _client;
    private readonly UserRepositoryManager _userRepositoryManager;

    public UserService(DiscordSocketClient client, UserRepositoryManager userManager)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _userRepositoryManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public Task<DiscordUser> AddUserAsync(ulong discordUserId, string activationToken)
        => _userRepositoryManager.AddUserAsync(discordUserId, activationToken);

    public Task<DiscordUser> GetUserByDiscordIdAsync(ulong discordUserId)
        => _userRepositoryManager.GetUserByDiscordIdAsync(discordUserId);

    public Task<DiscordUser> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }
}