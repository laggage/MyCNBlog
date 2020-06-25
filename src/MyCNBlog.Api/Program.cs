using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCNBlog.Database;
using Serilog;
using Serilog.Events;

namespace MyCNBlog.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                IHostBuilder builder = CreateHostBuilder(args);
                IHost host = SeedData(args) ?
                    builder.ConfigureServices(s => s.AddScoped<DbInitializer>()).Build() :
                    builder.Build();

                if (SeedData(args))
                {

                    using (IServiceScope scope = host.Services.CreateScope())
                    {
                        IServiceProvider services = scope.ServiceProvider;
                        DbInitializer dbInitializer = services.GetRequiredService<DbInitializer>();
                        dbInitializer.InitDatabase();
                    }
                }
                else
                {

                    host.Run();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog((h, configuration) =>
            {
                configuration.ReadFrom.Configuration(h.Configuration);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

        private static bool SeedData(string[] args)
        {
            if (args.Contains("--seed")) return true;
            return false;
        }
    }
}
