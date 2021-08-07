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
            BookmarkFeatures bf;

            public BookmarkCommands(ILogger<BookmarkCommands> logger, BookmarkFeatures bf)
            {
                this.logger = logger;
                this.bf = bf;
            }

            #region add
            [Command("add"), Aliases("a"), Description("Bookmark a message by replying to it with this command or pasting the message id. You an also react with 🔖 for the same result.")]
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
                if(bf.BookmarkAdd(ctx.User, message))
                    await ctx.RespondAsync("Bookmark added! Use the list command to view your bookmarks!");
                else
                    await ctx.RespondAsync("That message is already bookmarked!");
            }
            #endregion

            #region rem
            [Command("remove"), Aliases("rem", "r"), Description("Remove a bookmark from a message by replying to it with this command or pasting the message id.  Remove this reaction 🔖 for the same result.")]
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
                if(bf.BookmarkRemove(ctx.User, message))
                    await ctx.RespondAsync("Bookmark removed! Use the list command to view your bookmarks!");
                else
                    await ctx.RespondAsync("That message is not bookmarked!");
            }
            #endregion

            #region list

            const string argumentsDescription = "Optional arguments: `user:<mention or id>` `channel:<mention or id>` `server:<id>`";

            [Command("list"), Aliases("l"), Description("List all the bookmarks. You can filter the results using arguments.")]
            public async Task List(CommandContext ctx, [RemainingText] [Description(argumentsDescription)] string arguments)
            {
                ulong userFilterId=0, channelFilterId=0, queryFilterId=0;
                if (arguments != null && arguments != "")
                {
                    //preprocess and split string
                    string[] tokens = arguments.Replace(":", ": ").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    //check
                    for (int i=0;i<tokens.Length-1;i++)
                    {
                        ulong result;
                        switch (tokens[i])
                        {
                            case "user:":
                               if (ulong.TryParse(tokens[i + 1], out result))
                                   userFilterId = result;
                               else if (tokens[i + 1].StartsWith("<@!") && ulong.TryParse(tokens[i+1].Substring(3,tokens[i+1].Length-4),out result))
                                   userFilterId = result;
                               break;

                            case "channel:":
                               if (ulong.TryParse(tokens[i + 1], out result))
                                   channelFilterId = result;
                               else if (tokens[i + 1].StartsWith("<#") && ulong.TryParse(tokens[i + 1].Substring(2, tokens[i + 1].Length - 3), out result))
                                   channelFilterId = result;
                               break;

                            case "server:":
                               if (ulong.TryParse(tokens[i + 1], out result))
                                   queryFilterId = result;
                               break;
                        }
                    }

                    Bookmark[] queryResult = bf.BookmarkQuery(ctx.User, userFilterId, channelFilterId, queryFilterId);

                    //todo: actually display the results
                    await ctx.Message.RespondAsync($"Found {queryResult.Length} bookmarks");
                }
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
            //todo: should also save a small part of the message to display later
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
        public bool BookmarkRemove(DiscordUser user, DiscordMessage msg)
        {
            //todo: use a sql query instead
            Bookmark b = context.Bookmarks.SingleOrDefault(b => b.MessageSnowflake == msg.Id && b.UserSnowflake == user.Id);
            if (b == null)
                return false;
            context.Bookmarks.Remove(b);

            //try
            //{
                context.SaveChanges();
            //}
            //catch (DbUpdateException ex)
            //{
            //    HandlePostgressErrorCode(ex,"");
            //    context.Entry(b).State = EntityState.Detached;
            //    return false;
            //}

            return true;
        }

        const int querySize = 10;

        public Bookmark[] BookmarkQuery(DiscordUser user, ulong userFilterId, ulong channelFilterId, ulong guildFilterId)
        {
            //todo: pagination
            IQueryable<Bookmark> query = context.Bookmarks
                .Where(b => b.UserSnowflake == user.Id);
            
            if (userFilterId != 0)    query = query.Where(b => b.UserSnowflake == userFilterId);
            if (channelFilterId != 0) query = query.Where(b => b.ChannelSnowflake == channelFilterId);
            if (guildFilterId != 0)   query = query.Where(b => b.GuildSnowFlake == guildFilterId);

            return query.ToArray();
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
