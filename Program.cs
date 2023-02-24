using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Fluent;

namespace bic_fhtw_discord_bot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var bot = new FhtwBot(new NLogLoggerFactory());
            bot.MainAsync().GetAwaiter().GetResult();
            Console.WriteLine("Goodbye World!");
        }
    }
}
