namespace BIC_FHTW.DiscordBot.Services;

public interface IEmailValidationService
{
    bool IsValidEmail(string email);
}