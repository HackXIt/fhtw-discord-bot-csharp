using System.Threading.Tasks;
using Discord;

namespace BIC_FHTW.DiscordBot.Services;

public interface IDirectMessageService
{
    Task SendDirectMessage(IUser user, string message);
}