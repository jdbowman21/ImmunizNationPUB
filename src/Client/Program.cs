using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore;
using Vividly.AspNetCore.Hosting;
using ImmunizNation.Client.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ImmunizNation.Client;

namespace ImmunizNation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            // Automatically apply migrations when the application is started and seed the database.
            // This is implemented rather than using the DbContext.OnModelCreating function for a cleaner and includes
            // methods for adding entities as on one and EF will link all foreign keys up, and allows for a much cleaer workflow.
            host.MigrateDbContext<ApplicationDbContext, ApplicationDbContextSeed>((context, services) =>
            {
                var env = services.GetService<IWebHostEnvironment>();
                var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
                var settings = services.GetService<IOptions<AppSettings>>();

                new ApplicationDbContextSeed(services)
                    .SeedAsync(context, env, logger, settings)
                    .Wait();
            });

            host.Run();
            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
