using System;

using Avalonia;
using Avalonia.ReactiveUI;
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
        Logger.LogInfo("Starting Stego Revealer App");

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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
