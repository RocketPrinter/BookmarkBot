using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DSharpPlus;

namespace Server.Discord
{
    public class Bot
    {
        DiscordClient client;
        ILogger<Bot> logger;

        public Bot(ILogger<Bot> logger, IConfiguration configuration)
        {
            this.logger = logger;
            logger.LogInformation("Starting Bot...");

            client = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration.GetValue<string>("Bot:Token"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All //TODO: Remove unused Intents
            });
            Run();
        }

        async void Run()
        {
            client.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.Channel.SendMessageAsync("pong!");

            };

            await client.ConnectAsync();
        }
    }
}
