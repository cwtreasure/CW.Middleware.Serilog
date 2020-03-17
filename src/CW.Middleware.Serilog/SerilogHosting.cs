namespace CW.Middleware.Serilog
{
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Hosting;
    using System;

    public static class SerilogHosting
    {
        private static string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{AppName}] [{SourceContext}] [{UserIp}] [{cTraceId}] {Message}{NewLine}{Exception}";

        public static void Run(Func<IHostBuilder> builder, Action<LoggerConfiguration> loggerAction = null)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

            if (loggerAction == null)
            {
                loggerConfiguration = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("AppName", "AppName")
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Debug()
                    .WriteTo.Console(
                        outputTemplate: outputTemplate)
                    .WriteTo.File(
                        path: "logs/AppName.log",
                        outputTemplate: outputTemplate,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 5,
                        encoding: System.Text.Encoding.UTF8);
            }
            else
            {
                loggerAction(loggerConfiguration);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            try
            {
                Log.ForContext("SourceContext", "Program").Information("Application starting...");

                builder().UseSerilog().Build().Run();
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "Program").Fatal(ex, "Application start-up failed!!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
