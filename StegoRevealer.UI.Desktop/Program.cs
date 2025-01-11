using System;

using Avalonia;
using Avalonia.ReactiveUI;
using StegoRevealer.UI.Desktop.ConsoleInterface;
using StegoRevealer.UI.Tools;

namespace StegoRevealer.UI.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Logger.LogInfo("Starting Stego Revealer App");

        // Запуск в режиме интерфейса командной строки
        if (args.Length > 0)
        {
            Logger.LogInfo($"Started with command line args: {string.Join(", ", args)}");
            CommandLineParser.HandleCommand(args).Wait();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Logger.LogError(ex is null ? "Unknown error" : ex.Message);

        if (e.IsTerminating)
        {
            // Unexpected termintaion actions
            Configurator.SaveConfig();
            TempManager.Instance.DeleteImageHandlers();
            TempManager.Instance.DeleteTempImages();
            Logger.LogError("Stego Revealer closed due to an error");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
