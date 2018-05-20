using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleApplication
{
    public static class SerilogHostBuilderExtensions
    {
        /// <summary>
        /// Sets Serilog as the logging provider.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="Serilog.Log"/> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger"/> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose"/> is true, the <see cref="Log.CloseAndFlush()"/> method will be
        /// called on the static <see cref="Log"/> class instead.</param>
        /// <returns>The web host builder.</returns>
        public static IHostBuilder UseSerilog(this IHostBuilder builder, Serilog.ILogger logger = null, bool dispose = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ConfigureServices(collection =>
                collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(logger, dispose)));
            return builder;
        }

        /// <summary>Sets Serilog as the logging provider.</summary>
        /// <remarks>
        /// A <see cref="HostBuilderContext"/> is supplied so that configuration and hosting information can be used.
        /// The logger will be shut down when application services are disposed.
        /// </remarks>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="configureLogger">The delegate for configuring the <see cref="LoggerConfiguration" /> that will be used to construct a <see cref="Logger" />.</param>
        /// <param name="preserveStaticLogger">Indicates whether to preserve the value of <see cref="Log.Logger"/>.</param>
        /// <returns>The web host builder.</returns>
        public static IHostBuilder UseSerilog(this IHostBuilder builder, Action<HostBuilderContext, LoggerConfiguration> configureLogger, bool preserveStaticLogger = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureLogger == null) throw new ArgumentNullException(nameof(configureLogger));

            builder.ConfigureServices((context, collection) =>
            {
                var loggerConfiguration = new LoggerConfiguration();
                configureLogger(context, loggerConfiguration);
                var logger = loggerConfiguration.CreateLogger();
                if (preserveStaticLogger)
                {
                    collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(logger, true));
                }
                else
                {
                    // Passing a `null` logger to `SerilogLoggerFactory` results in disposal via
                    // `Log.CloseAndFlush()`, which additionally replaces the static logger with a no-op.
                    Log.Logger = logger;
                    collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(null, true));
                }
            });

            return builder;
        }
    }
}
