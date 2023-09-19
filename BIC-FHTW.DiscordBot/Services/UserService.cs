using System;
using System.Threading.Tasks;
using BIC_FHTW.Database.Models;
using BIC_FHTW.Database.Services;
using BIC_FHTW.Scraper.Scrapers.Userprofile;
using BIC_FHTW.Shared;
using Discord.WebSocket;

namespace BIC_FHTW.DiscordBot.Services;

public class UserService : IUserService
{
    private readonly UserRepositoryManager _userRepositoryManager;

    public UserService(UserRepositoryManager userRepositoryManager)
    {
        _userRepositoryManager = userRepositoryManager ?? throw new ArgumentNullException(nameof(userRepositoryManager));
    }

    public async Task<DiscordUserDTO> AddUserAsync(ulong discordUserId, string activationToken, string activationMail)
        => EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.AddUserAsync(discordUserId, activationToken, activationMail)) ?? throw new InvalidOperationException();

    public async Task<DiscordUserDTO> GetUserByDiscordIdAsync(ulong discordUserId)
        => EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.GetUserByDiscordIdAsync(discordUserId)) ?? throw new InvalidOperationException();

    public async Task<DiscordUserDTO?> GetUserByActivationTokenAsync(string activationToken)
        => EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.GetUserByActivationTokenAsync(activationToken)) ?? null;

    public async Task<DiscordUserDTO?> GetUserByStudentUidAsync(string studentUid)
        => EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.GetDiscordUserByStudentUidAsync(studentUid)) ?? null;

    public async Task UpdateUserStatusAsync(DiscordUser discordUser, DiscordUser.UserStatus newStatus)
        => await _userRepositoryManager.UpdateUserStatusAsync(discordUser, newStatus);

    public async Task<DiscordUserDTO> LinkStudentToDiscordUserAsync(string? studentUid, ulong discordUserId)
        => EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.LinkStudentToDiscordUserAsync(studentUid, discordUserId)) ?? throw new InvalidOperationException();

    public async Task<StudentDTO?> GetStudentByUidAsync(string studentUid)
        => EntityFactory.ConvertFromStudent(await _userRepositoryManager.GetStudentByUidAsync(studentUid));

    public async Task<StudentDTO?> GetStudentByDiscordIdAsync(ulong discordUserId)
        => EntityFactory.ConvertFromStudent(await _userRepositoryManager.GetStudentByDiscordIdAsync(discordUserId));

    public async Task<DiscordUserDTO?> ActivateUserWithStudentInformation(ulong discordUserId,
        UserprofileScrapeResult scrapeResult)
    {
        var student = EntityFactory.ConvertFromUserprofileScrapeResult(scrapeResult);
        DiscordUserDTO? userDto = null;
        if (student != null)
        {
            userDto = EntityFactory.ConvertFromDiscordUser(await _userRepositoryManager.AddStudentAndActivateUserAsync(discordUserId, student));
        }

        return userDto;
    }
}