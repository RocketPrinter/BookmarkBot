using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Server.Db;

namespace Server.Discord
{
    public class Bookmark
    {
        public class BookmarkCommands : BaseCommandModule
        {
            ILogger<BookmarkCommands> logger;
            Bookmark bookmark;

            public BookmarkCommands(ILogger<BookmarkCommands> logger, Bookmark bookmark)
            {
                this.logger = logger;
                this.bookmark = bookmark;
            }

            #region add
            [Command("add"), Aliases("a"), Description("Bookmark a message by replying to it with this command or pasting the message id.")]
            public async Task Add(CommandContext ctx)
            {
                if (ctx.Message.ReferencedMessage == null)
                {
                    await ctx.Message.RespondAsync("Please reply to the message you want to bookmark or paste the message id.");
                    return;
                }
                await Add(ctx, ctx.Message.ReferencedMessage);
            }
            [Command("add")]
            public async Task Add(CommandContext ctx, DiscordMessage message)
            {
                if(await bookmark.BookmarkAdd(ctx.User, message))
                    await ctx.RespondAsync("Bookmark added! Use b!list to view your bookmarks!");
                else
                    await ctx.RespondAsync("That message is already bookmarked!");
                //todo: handle exceptions
            }
            #endregion

            #region rem
            [Command("remove"), Aliases("rem", "r"), Description("Remove a bookmark from a message by replying to it with this command or pasting the message id.")]
            public async Task Rem(CommandContext ctx)
            {
                if (ctx.Message.ReferencedMessage == null)
                {
                    await ctx.Message.RespondAsync("Please reply to the message you want to remove the bookmark to or paste the message id.");
                    return;
                }
                await Add(ctx, ctx.Message.ReferencedMessage);
            }
            [Command("remove")]
            public async Task Rem(CommandContext ctx, DiscordMessage message)
            {
                if(await bookmark.BookmarkRemove(ctx.User, message))
                    await ctx.RespondAsync("Bookmark removed! Use b!list to view your bookmarks!");
                else
                    await ctx.RespondAsync("That message is not bookmarked!");
                //todo: handle exceptions
            }
            #endregion

            #region list
            [Command("list"), Aliases("l"), Description("List all the bookmarks. You can add a mention or a channel to filter the results.")]
            public async Task List(CommandContext ctx)
            {
                await List(ctx, null, null);
            }
            [Command("list")]
            public async Task List(CommandContext ctx, [Description("User to filter by")] DiscordUser user)
            {
                await List(ctx, user, null);
            }
            [Command("list")]
            public async Task List(CommandContext ctx, [Description("Channel to filter by")] DiscordChannel channel)
            {
                await List(ctx, null, channel);
            }
            [Command("list")]
            public async Task List(CommandContext ctx, [Description("User to filter by")] DiscordUser user, [Description("Channel to filter by")] DiscordChannel channel)
            {
                throw new NotImplementedException();
            }
            #endregion
        }

        public Bot bot;
        ILogger<Bookmark> logger;
        Db.BookmarkContext context;

        public Bookmark(Bot bot, ILogger<Bookmark> logger, Db.BookmarkContext context, IServiceProvider services)
        {
            this.bot = bot;
            this.logger = logger;
            this.context = context;

            bot.client.MessageReactionAdded += ReactionAdded;
            bot.client.MessageReactionRemoved += ReactionRemoved;

            CommandsNextExtension commands = bot.client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "b!" },
                Services = services
            });
            commands.RegisterCommands<BookmarkCommands>();

            logger.LogInformation("Bookmark featureset enabled");
        }

        #region Events
        readonly DiscordEmoji bookmarkEmoji = DiscordEmoji.FromUnicode("📑");
        const int replyDeleteDelay=5000; //in miliseconds

        private async Task ReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if (e.Emoji != bookmarkEmoji)
                return;

            bool ok = await BookmarkAdd(e.User,e.Message);

            DiscordMessage reply = await e.Message.RespondAsync($"{e.User.Mention} " + (ok?"bookmark added!": "That message is already bookmarked"));
            await Task.Delay(replyDeleteDelay);
            await reply.DeleteAsync();

            //todo: handle exceptions
        }
        private async Task ReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            if (e.Emoji != bookmarkEmoji)
                return;

            bool ok = await BookmarkRemove(e.User, e.Message);

            DiscordMessage reply = await e.Message.RespondAsync($"{e.User.Mention} " + (ok?"bookmark removed!": "That message is not bookmarked"));
            await Task.Delay(replyDeleteDelay);
            await reply.DeleteAsync();

            //todo: handle exceptions
        }
        #endregion

        #region DB Interactions

        public async Task<bool> BookmarkAdd(DiscordUser user, DiscordMessage msg)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> BookmarkRemove(DiscordUser user, DiscordMessage msg)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
