using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Server.Db;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using static Server.Db.BookmarkContext;

using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace Server.Discord
{
    public partial class BookmarkFeature
    {

        DiscordClient client;
        ILogger<BookmarkFeature> logger;
        Db.BookmarkContext context;

        public BookmarkFeature(DiscordClient client, ILogger<BookmarkFeature> logger, Db.BookmarkContext context, CommandsNextExtension commands)
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
                    //todo: null ref error when bookmarking a msg in a dm
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
                AuthorSnowflake = msg.Author.Id,
                MessageSummary = msg.Content.Substring(0, Math.Min( msg.Content.Length,50)) //we take more than we need to make sure  we have enough
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

        public Bookmark[] BookmarkQuery(DiscordUser user, int querySize, ulong filterUserId, ulong filterChannelId, ulong filterGuildId)
        {
            //todo: pagination
            IQueryable<Bookmark> query = context.Bookmarks
                .Where(b => b.UserSnowflake == user.Id)
                .OrderByDescending(b => b.BookmarkId);
            
            if (filterUserId != 0)    query = query.Where(b => b.AuthorSnowflake == filterUserId);
            if (filterChannelId != 0) query = query.Where(b => b.ChannelSnowflake == filterChannelId);
            if (filterGuildId != 0)   query = query.Where(b => b.GuildSnowFlake == filterGuildId);

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
