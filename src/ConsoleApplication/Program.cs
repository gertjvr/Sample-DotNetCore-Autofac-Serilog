using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ConsoleApplication
{
    class Program
    {
        static int Main(string[] args)
        {
            var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);

            Log.Logger = BuildLogger(levelSwitch);

            try
            {
                BuildHost(args).Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHost BuildHost(string[] args)
            => new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(builder =>
                {
                    builder
                        .RegisterAssemblyModules(typeof(Program).Assembly);
                }))
                .ConfigureAppConfiguration((hostingContext,configuration) =>
                {
                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args ?? Array.Empty<string>());
                })
                .UseSerilog()
                .ConfigureServices((hostingContext, services) =>
                {
                    services
                        .AddSingleton<IHostedService, BusHostedService>();
                })
                .Build();

        static ILogger BuildLogger(LoggingLevelSwitch levelSwitch)
            => new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .MinimumLevel.Override("System", LogEventLevel.Verbose)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .WriteTo.Seq(
                    "http://localhost:5341",
                    compact: true,
                    controlLevelSwitch: levelSwitch)
                .CreateLogger();
    }
}
