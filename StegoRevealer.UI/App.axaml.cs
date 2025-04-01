using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StegoRevealer.Common;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels;
using StegoRevealer.UI.Windows;

namespace StegoRevealer.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Logger.LogInfo("Creating App logic");
        var mainWindowVm = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Logger.LogInfo("Creating main window");

            var mainWindow = new MainWindow { DataContext = mainWindowVm };
            desktop.MainWindow = mainWindow;
            mainWindowVm.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
        Logger.LogInfo("Initialization completed");
    }
}