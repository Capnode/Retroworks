using System;
using Avalonia;
using Serilog;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

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

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ReportException("Non-UI exception", (Exception)e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, e) => ReportException("Task exception", e.Exception);
            
            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                ReportException("Unhandled exception", ex);
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

        private static void ReportException(string cause, Exception ex)
        {
            Log.Fatal(ex, cause);
            string[] args = new string[] { cause, ex.GetType().ToString(), ex.Message, ex?.StackTrace ?? string.Empty};
            Process.Start(typeof(Program).Assembly.Location.Replace(".dll", ".exe"), args);
        }
    }
}
