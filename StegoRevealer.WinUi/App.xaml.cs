using System.Windows;
using StegoRevealer.WinUi.ViewModels;

namespace StegoRevealer.WinUi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var rootVeiwModel = new RootViewModel();
            var mainWindow = new MainWindow() { DataContext = rootVeiwModel };
            mainWindow.Show();
        }
    }
}
