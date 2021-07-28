using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            //REST API and Swagger
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });
            });

            //Discord client
            services.Add(new ServiceDescriptor(typeof(Discord.Bot), typeof(Discord.Bot), ServiceLifetime.Singleton));
            warmUpTypes.Add(typeof(Discord.Bot));

            //Bookmark
            if (Configuration.GetValue<bool>("BookmarkFeature"))
            {
                //services.AddDbContext<Db.BookmarkContext>(options =>
                //    options.UseNpgsql(Configuration.GetConnectionString("BookmarkContext")));

                services.Add(new ServiceDescriptor(typeof(Discord.Bookmark), typeof(Discord.Bookmark), ServiceLifetime.Singleton));
                warmUpTypes.Add(typeof(Discord.Bookmark));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //warm up types
            foreach (Type type in warmUpTypes)
                app.ApplicationServices.GetService(type);
        }
    }
}