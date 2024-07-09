using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FHTW.DiscordBot.Middleware;

public interface IInteractionMiddleware : IMiddleware
{
    string CommandName { get; }
    string Description { get; }
    List<SlashCommandOptionBuilder> Options { get; }
    Task<bool> ExecuteCmdAsync(SocketSlashCommand command);
}