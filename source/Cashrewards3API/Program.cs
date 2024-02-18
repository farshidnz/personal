using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Threading;

namespace Cashrewards3API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logLevel = (Environment.GetEnvironmentVariable("LOG_LEVEL")?.ToLower() ?? "warning") switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Warning
            };

            ThreadPool.SetMinThreads(50, 50);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(logLevel))
                .Filter.ByExcluding(log => log.Exception != null)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Log Level is {LogLevel}", logLevel);

            Log.Information("Starting up");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var isDevops4Deployed = Environment.GetEnvironmentVariable("Pipeline") == "DevOps4";
                    if (!isDevops4Deployed)
                    {
                        var environment = Environment.GetEnvironmentVariable("DEPLOY_STAGE");
                        if (environment != "Development")
                            config.AddSystemsManager($"/ECS/Cashrewards3-API-{environment}");
                        if (File.Exists($"appsettings.{environment}.json"))
                            config.AddJsonFile($"appsettings.{environment}.json");
                    }
                    else
                    {
                        // Note: this is a copy of appsettings.json. The per-environment
                        // settings are configured as environment variables via configurator.
                        // For values not specified in configurator.json, the values in this file 
                        // will apply. 
                        config.AddJsonFile($"appsettings.devops4.json");
                    }

                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}