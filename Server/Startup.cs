using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public List<System.Type> warmUpTypes = new();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region API
            //REST API and Swagger
            //services.AddControllers();
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookmarkbot", Version = "v1" });
            //});
            #endregion

            #region Bot
            //Discord client
            services.AddSingleton(x =>
            {
                var client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Configuration.GetValue<string>("Bot:Token"),
                    LoggerFactory = x.GetRequiredService<ILoggerFactory>(),
                    Intents = DiscordIntents.All //TODO: Remove unused Intents
                });
                client.ConnectAsync(/*new DiscordActivity("the dashboard", ActivityType.Watching)*/).Wait();
                return client;
            });

            //Commandsnext
            services.AddSingleton(x => x.GetRequiredService<DiscordClient>().UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "b/" },
                Services = x.GetRequiredService<IServiceProvider>()
            }));

            //Interactivity
            services.AddSingleton(x => x.GetRequiredService<DiscordClient>().UseInteractivity(new DSharpPlus.Interactivity.InteractivityConfiguration()
            {
                Timeout = new TimeSpan(0, 0, 30)
            }));
            #endregion

            //Bookmark
            if (Configuration.GetValue<bool>("BookmarkFeature"))
            {
                services.AddDbContext<Db.BookmarkContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("Bookmark")), contextLifetime:ServiceLifetime.Singleton);

                services.AddSingleton(typeof(Discord.BookmarkFeature));
                warmUpTypes.Add(typeof(Discord.BookmarkFeature));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"));
            //}
            //
            //app.UseHttpsRedirection();
            //
            //app.UseRouting();
            //
            //app.UseAuthorization();
            //
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

            //register command modules
            var cnext = app.ApplicationServices.GetService<CommandsNextExtension>();
            if (Configuration.GetValue<bool>("BookmarkFeature"))
                cnext.RegisterCommands<Discord.BookmarkCommands>();

            //warm up types
            foreach (Type type in warmUpTypes)
                app.ApplicationServices.GetService(type);
        }
    }
}