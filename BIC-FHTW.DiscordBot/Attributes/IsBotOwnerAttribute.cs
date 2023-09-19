using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BIC_FHTW.DiscordBot.Attributes;

public class IsBotOwnerAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return Task.FromResult(context.User.Id == services.GetRequiredService<BotSettings>().OwnerId 
            ? PreconditionResult.FromSuccess() 
            : PreconditionResult.FromError("You are not allowed to use this command."));
    }
}