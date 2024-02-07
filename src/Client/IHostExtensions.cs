using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;

namespace Vividly.AspNetCore.Hosting
{
    /// <summary>
    /// Extensions methods for IHost.
    /// </summary>
    public static class IHostExtensions
    {
        public static IHost MigrateDbContext<TContext, TContextSeeder>(this IHost host, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            using(var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContextSeeder>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    // @note - this might be best suited as an IConfiguration Option, which would require the Configuration to be built out.
                    var retries = 10;
                    var retry = Policy.Handle<SqlException>()
                            .WaitAndRetry(
                                retries,
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                onRetry: (exception, timeSpan, retry, ctx) =>
                                {
                                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                                });

                    // if the sql server container is not created on run docker compose this
                    // migration can't fail for network related exception. The retry options for DbContext only 
                    // apply to transient exceptions
                    // Note that this is NOT applied when running some orchestrators (let the orchestrator to recreate the failing service)
                    retry.Execute(() => InvokeSeeder(seeder, context, services));

                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }

            return host;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }

        /** note - This should maybe be put in a different namespace. It may not be used in this app anyways */

        /// <summary>
        /// Gets the appsettings configuration file settings.
        /// </summary>
        /// <param name="directory">The directory of the configuration file. Defaults to the root directory of the appsettings.json file.</param>
        /// <returns>Application settings configuration.</returns>
        private static IConfiguration GetConfiguration(string directory = "") => 
            new ConfigurationBuilder()
                .SetBasePath(String.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : directory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
