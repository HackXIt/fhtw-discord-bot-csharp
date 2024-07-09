using System;
using System.Threading.Tasks;
using Discord;

namespace FHTW.DiscordBot.Services;

public class DirectMessageService
{
    public async Task SendDirectMessage(IUser user, string message)
    {
        /*
        var dmChannel = await user.GetOrCreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        */
        throw new NotImplementedException();
    }
}