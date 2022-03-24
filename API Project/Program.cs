using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;                          // Logging

namespace API_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Get config from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("REST API App Starting Up...");
                CreateHostBuilder(args).Build().Run();

            } catch(Exception ex)
            {
                Log.Fatal("Error! Rest API App Failed To Start! - " + ex.Message);

            } finally
            {
                // Clean up code and write log to file
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
