using System;
using Avalonia;
using Serilog;
using System.IO;

namespace Retroworks.RCBus
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Set current directory to a writable location
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cwd = Path.Combine(appData, "Retroworks.RCBus");
            Directory.CreateDirectory(cwd);
            Directory.SetCurrentDirectory(cwd);
            string logPath = Path.Combine(cwd, "Retroworks.RCBus-.log");

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Program starting...");
            Log.Information($"Working directory: {cwd}");

            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
