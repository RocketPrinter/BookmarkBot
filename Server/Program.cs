using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static bool inDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // make sure db is up to date if we are running inside docker
            if (inDocker)
            {
                using (var scope = host.Services.CreateScope())
                {
                    var logger = new Logger<Program>(host.Services.GetRequiredService<ILoggerFactory>());
                    var db = scope.ServiceProvider.GetRequiredService<Db.BookmarkContext>();

                    await Task.Delay(5000); // wait for db to start
                    while (true)
                    {
                        try
                        {
                            logger.LogInformation("Migrating db...");
                            await db.Database.MigrateAsync();
                            break;
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Migration failed! Db probably hasn't started yet. Retrying after 5 seconds...");
                            await Task.Delay(5000);
                        }
                    }
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext,config) =>
                {
                    //if we are inside a container load the secret
                    // since the secret manager is only for development you can use this to specify the bot token and ConnectionStrings
                    if (inDocker)
                    {
                        // for some bizzare reason docker strips the file extension
                        config.AddJsonFile("/run/secrets/server", false,false);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}