using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using static Server.Db.BookmarkContext;
using System.Collections.Generic;
using DSharpPlus;
using System.Linq;
using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Discord
{
    public class CoreCommands : BaseCommandModule
    {
        ILogger<CoreCommands> logger;
        DiscordClient client;
        CommandsNextExtension cnext;
        IConfiguration configuration;
        IServiceCollection services;

        public CoreCommands(ILogger<CoreCommands> logger, DiscordClient client, CommandsNextExtension cnext, IConfiguration configuration, IServiceCollection services)
        {
            this.logger = logger;
            this.client = client;
            this.cnext = cnext;
            this.configuration = configuration;
            this.services = services;
        }

        [Command("invite")]
        [Description("Invite the bot to your server")]
        public async Task Invite(CommandContext ctx) =>
            await ctx.RespondAsync(
                new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Blurple,
                    //ImageUrl = client.CurrentUser.AvatarUrl,
                    Title = "Click here!",
                    Url = configuration.GetValue<string>("InviteLink")
                }
                .WithAuthor("Invite")
            );

        [Command("status")]
        [Description("Displays some information about the bot")]
        public async Task Status(CommandContext ctx) =>
            await ctx.RespondAsync(
                new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Blurple,
                    //ImageUrl = client.CurrentUser.AvatarUrl,
                    Title = "Status"
                }
                .AddField($"Guilds", $"in {client.Guilds.Count} of them", false)

                .AddField("Running", client.VersionString, true)
                .AddField("Commands", $"{cnext.RegisteredCommands.Count} loaded",true)
                .AddField("Services", $"{services.Count} loaded", true)

                .AddField("Shard", $"#{client.ShardId} out of {client.ShardCount}", true)
                .AddField("Gateway", $"version {client.GatewayVersion}", true)
                .AddField("Ping", $"of {client.Ping} ms", true)
            );
    }
}
