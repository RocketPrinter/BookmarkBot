
![Bookmark](bookmark.png)
# BookmarkBot # 
A simple discord bot written in C# and using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus), [ASP.NET Core](https://github.com/dotnet/aspnetcore), [EF Core](https://github.com/dotnet/efcore) and [PostgresSQL](https://www.postgresql.org/).
It uses [docker-compose](https://github.com/docker/compose) for easy and simple deployment.
I made it to prepare myself for making a more complex and actually useful bot.

## Features ##
* Bookmark messages using `b/add` or by reacting with :bookmark:. `b/remove` does the opposite.
* View bookmarks using `b/list` and filter them by server, channel and user. It includes a compact and non-compact mode as well as a very crude pagination system.
* `b/invite` and `b/status` commands.

## Invite ##
Click [here](https://discord.com/api/oauth2/authorize?client_id=873944900931051551&permissions=240518548544&scope=bot) to invite the bot to your own server.

### How to develop
1. `git pull`
2. Use the secrets manager to store the bot token and connection string. (read `./secrets/templates.txt`)
3. Install + config postgres on your local machine.
4. Have fun

### How to deploy
1. `git pull`
2. Use `./secrets/templates.txt` to create and fill in the neccesary files.
3. `docker-compose up`