using System;
using System.Threading.Tasks;
using Discord;

namespace BIC_FHTW.DiscordBot.Services;

public class EphemeralMessageService : IEphemeralMessageService
{
    public async Task SendEphemeralMessage(IMessageChannel channel, string message)
    {
        /*
        var messageOptions = new MessageReference(new MessageReferenceProperties { FailIfNotExists = false });
        await channel.SendMessageAsync(message, messageReference: messageOptions);
        */
        throw new NotImplementedException();
    }
}