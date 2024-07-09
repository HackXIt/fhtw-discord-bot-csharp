using System.Threading.Tasks;
using Discord;

namespace FHTW.DiscordBot.Services;

public interface IDirectMessageService
{
    Task SendDirectMessage(IUser user, string message);
}