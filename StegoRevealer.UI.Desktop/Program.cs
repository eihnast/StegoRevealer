using System;

using Avalonia;
using ALogger = Avalonia.Logging;
using ReactiveUI.Avalonia;

using StegoRevealer.Common;
using StegoRevealer.Common.ConsoleInterface;

namespace StegoRevealer.UI.Desktop;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        CommonLogger.LogInfo("Starting Stego Revealer App");

        // Запуск в режиме интерфейса командной строки
        if (args.Length > 0)
        {
            bool isWindows = Environment.OSVersion.Platform is PlatformID.Win32NT;
            if (isWindows)
                WinConsole.ConnectConsole();
            
            CommonLogger.LogInfo($"Started with command line args: {string.Join(", ", args)}");
            CommandLineParser.HandleCommand(args).Wait();

            if (isWindows)
            {
                WinConsole.RestorePrompt();

                // Освобождаем консоль, если она была создана
                WinConsole.StopConsole();
            }

            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        CommonLogger.LogError(ex is null ? "Unknown error" : ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);

        if (e.IsTerminating)
        {
            // Unexpected termintaion actions
            Configurator.SaveConfig();
            TempManager.Instance.DeleteImageHandlers();
            TempManager.Instance.DeleteTempImages();
            TempManager.Instance.DeleteTempFiles();
            CommonLogger.LogError("Stego Revealer closed due to an error");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(ALogger.LogEventLevel.Debug)
            .UseReactiveUI();
}
