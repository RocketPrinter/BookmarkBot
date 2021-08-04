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
using Npgsql;
using Microsoft.EntityFrameworkCore;
using static Server.Db.BookmarkContext;

namespace Server.Discord
{
    public class BookmarkFeatures
    {
        public class BookmarkCommands : BaseCommandModule
        {
            ILogger<BookmarkCommands> logger;
            BookmarkFeatures bookmark;

            public BookmarkCommands(ILogger<BookmarkCommands> logger, BookmarkFeatures bookmark)
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
                if(bookmark.BookmarkAdd(ctx.User, message))
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
                await Rem(ctx, ctx.Message.ReferencedMessage);
            }
            [Command("remove")]
            public async Task Rem(CommandContext ctx, DiscordMessage message)
            {
                if(bookmark.BookmarkRemove(ctx.User, message))
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

        DiscordClient client;
        ILogger<BookmarkFeatures> logger;
        Db.BookmarkContext context;

        public BookmarkFeatures(DiscordClient client, ILogger<BookmarkFeatures> logger, Db.BookmarkContext context, CommandsNextExtension commands)
        {
            this.client = client;
            this.logger = logger;
            this.context = context;

            client.MessageReactionAdded += ReactionAdded;
            client.MessageReactionRemoved += ReactionRemoved;

            //commands.RegisterCommands<BookmarkCommands>();

            logger.LogInformation("Bookmark featureset enabled");
        }

        #region Events
        readonly DiscordEmoji bookmarkEmoji = DiscordEmoji.FromUnicode("🔖");
        const int replyDeleteDelay=5000; //in miliseconds

        private Task ReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if (e.Emoji != bookmarkEmoji)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                DiscordMessage msg;
                if (e.Message.Author == null)
                {
                    //fix weird caching issue where author is null
                    msg = await e.Message.Channel.GetMessageAsync(e.Message.Id);
                }
                else
                    msg = e.Message;

                bool ok = BookmarkAdd(e.User, msg);

                DiscordMessage reply = await e.Message.RespondAsync($"{e.User.Mention} " + (ok ? "bookmark added!" : "That message is already bookmarked"));
                await Task.Delay(replyDeleteDelay);
                await reply.DeleteAsync();
            });

            return Task.CompletedTask;
        }
        private Task ReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            if (e.Emoji != bookmarkEmoji)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                bool ok = BookmarkRemove(e.User, e.Message);

                DiscordMessage reply = await e.Message.RespondAsync($"{e.User.Mention} " + (ok ? "bookmark removed!" : "That message is not bookmarked"));
                await Task.Delay(replyDeleteDelay);
                await reply.DeleteAsync();
            });

            return Task.CompletedTask;
        }
        #endregion

        #region DB Interactions

        /// <returns> false if a bookmark already existed, throws an exception if any other error occurs</returns>
        public bool BookmarkAdd(DiscordUser user, DiscordMessage msg)
        {
            Bookmark b = new()
            {
                UserSnowflake = user.Id,
                GuildSnowFlake = msg.Channel.GuildId,
                ChannelSnowflake = msg.ChannelId,
                MessageSnowflake = msg.Id,
                AuthorSnowflake = msg.Author.Id
            };
            context.Add(b);

            try
            {
                context.SaveChanges();    
            }
            catch (DbUpdateException ex)
            {
                HandlePostgressErrorCode(ex,PostgresErrorCodes.UniqueViolation);
                context.Entry(b).State = EntityState.Detached;
                return false;
            }

            return true;
        }

        /// <returns> false if the bookmark didn't exist, throws an exception if any other error occurs</returns>
        public bool BookmarkRemove(DiscordUser user, DiscordMessage msg)//todo: this ugh
        {
            //context.Database.ExecuteSqlInterpolated($@"DELETE FROM schema.""TABLE_NAME"" WHERE ""YOUR_COLUMN_NAME"" = {VALUE_TO_DELETE}");

            //todo: use a sql query instead
            Bookmark b = context.Bookmarks.Single(b => b.MessageSnowflake == msg.Id && b.UserSnowflake == user.Id);
            context.Bookmarks.Remove(b);

            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                HandlePostgressErrorCode(ex,"");//todo: figure out the correct code
                context.Entry(b).State = EntityState.Detached;
                return false;
            }

            return true;
        }

        public void HandlePostgressErrorCode(DbUpdateException ex, string code)
        {
            var sqlEx = ex.InnerException as PostgresException;
            if (sqlEx != null && sqlEx.SqlState == code)
                return;
            else
                throw ex;
        }

        #endregion
    }
}
