using System;
using NLog.Extensions.Logging;

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
