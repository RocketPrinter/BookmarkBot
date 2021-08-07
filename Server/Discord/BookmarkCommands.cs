using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Server.Db.BookmarkContext;

namespace Server.Discord
{
    public partial class BookmarkFeature
    {
        public class BookmarkCommands : BaseCommandModule
        {
            ILogger<BookmarkCommands> logger;
            BookmarkFeature bf;

            public BookmarkCommands(ILogger<BookmarkCommands> logger, BookmarkFeature bf)
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
    }
}
