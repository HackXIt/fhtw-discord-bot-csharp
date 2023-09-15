using System.Threading.Tasks;
using Discord;

namespace BIC_FHTW.DiscordBot.Services;

public interface IEphemeralMessageService
{
    Task SendEphemeralMessage(IMessageChannel channel, string message);
}