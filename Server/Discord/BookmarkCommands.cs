using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Server.Db.BookmarkContext;
using System.Collections.Generic;
using DSharpPlus;
using System.Linq;

namespace Server.Discord
{
    public partial class BookmarkFeature
    {
        public class BookmarkCommands : BaseCommandModule
        {
            ILogger<BookmarkCommands> logger;
            BookmarkFeature bf;
            DiscordClient client;

            public BookmarkCommands(ILogger<BookmarkCommands> logger, BookmarkFeature bf, DiscordClient client)
            {
                this.logger = logger;
                this.bf = bf;
                this.client = client;
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

            const string argumentsDescription = "Optional arguments: `user:<mention or id>` `channel:<mention or id>` `server:<id>` `compact:<true/false>`\n You can use `this` instead of an id.";
            const int embedMsgCount = 5, compactEmbedMsgCount = 20;

            [Command("list"), Aliases("l"), Description("List all the bookmarks. You can filter the results using arguments.")]
            public async Task List(CommandContext ctx, [RemainingText] [Description(argumentsDescription)] string arguments)
            {
                ulong filterUserId=0, filterChannelId=0, filterGuildId=0;
                bool compactEmbed=false;
            
                //arg parsing
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
                                    filterUserId = result;
                                else if (tokens[i + 1] == "this")
                                    filterUserId = ctx.User.Id;
                                else if (tokens[i + 1].StartsWith("<@!") && ulong.TryParse(tokens[i + 1].Substring(3, tokens[i + 1].Length - 4), out result))
                                    filterUserId = result;
                               break;

                            case "channel:":
                               if (ulong.TryParse(tokens[i + 1], out result))
                                   filterChannelId = result;
                                else if (tokens[i + 1] == "this")
                                    filterChannelId = ctx.Channel.Id;
                                else if (tokens[i + 1].StartsWith("<#") && ulong.TryParse(tokens[i + 1].Substring(2, tokens[i + 1].Length - 3), out result))
                                   filterChannelId = result;
                               break;

                            case "server:":
                               if (ulong.TryParse(tokens[i + 1], out result))
                                   filterGuildId = result;
                                else if (tokens[i + 1] == "this")
                                    filterGuildId = ctx.Guild.Id;
                                break;

                            case "compact:":
                                switch (tokens[i + 1])
                                {
                                    case "true":
                                    case "t":
                                    case "yes":
                                    case "y":
                                    case "1":
                                        compactEmbed = true;
                                        break;

                                    case "false":
                                    case "f":
                                    case "no":
                                    case "n":
                                    case "0":
                                        compactEmbed = false;
                                        break;
                                }
                                break;
                        }
                    }
                }
                
                //getting bookmarks
                Bookmark[] queryResult = bf.BookmarkQuery(ctx.User, compactEmbed?compactEmbedMsgCount:embedMsgCount, filterUserId, filterChannelId, filterGuildId);

                if (queryResult.Length == 0)
                {
                    await ctx.Message.RespondAsync("No bookmarks found!");
                    return;
                }

                // resolve all authors in the query
                // linq is fun!
                var users = Task.WhenAll(
                    queryResult.Select(bookmark => bookmark.AuthorSnowflake).Distinct()
                    .Select(async id => await client.GetUserAsync(id)).ToArray())
                    .Result
                    .ToDictionary(user=> user.Id);
                
                //build message
                DiscordMessageBuilder builder = new();
                builder.WithContent($"Showing {queryResult.Length} bookmarks:");

                if (compactEmbed)
                {
                    DiscordEmbedBuilder embedBuilder = new();

                    for (int i=0;i<queryResult.Length;i++)
                    {
                        Bookmark bookmark = queryResult[i];
                        embedBuilder.AddField( (i+1).ToString() + ") " + users[bookmark.AuthorSnowflake].Username, bookmark.MessageSummary + (bookmark.MessageSummary.Length == 50 ? "..." : ""), false);
                    }

                    builder.WithEmbed(embedBuilder);
                }
                else
                {
                     
                    //generate embeds
                    for (int i=0;i<queryResult.Length;i++)
                    {
                        Bookmark bookmark = queryResult[i];
                        DiscordUser author = users[bookmark.AuthorSnowflake];

                        DiscordEmbedBuilder embedBuilder = new()
                        {
                            Title = "Go to " + (bookmark.GuildSnowFlake != null ? client.Guilds[bookmark.GuildSnowFlake.Value]?.Name : "@me"),
                            Url = $"https://discord.com/channels/{ bookmark.GuildSnowFlake.Value.ToString() ?? "@me"}/{bookmark.ChannelSnowflake}/{bookmark.MessageSnowflake}",
                            Color = DiscordColor.Blurple
                        };
                        //on desktop the url will open discord in browser so it's not a perfect solution
                        embedBuilder.WithAuthor(author.Username, $"https://discord.com/users/{author.Id}", author.AvatarUrl);
                        embedBuilder.WithFooter(bookmark.MessageSummary + (bookmark.MessageSummary.Length==50?"...":""));

                        builder.AddEmbed(embedBuilder);
                    }    
                }

                await ctx.RespondAsync(builder);
            }
            #endregion
        }
    }
}
