namespace BIC_FHTW.DiscordBot.Services;

public interface IDatabaseService
{
    void AddUser(string discordId, string email, string token);
    string GenerateUniqueToken();
}