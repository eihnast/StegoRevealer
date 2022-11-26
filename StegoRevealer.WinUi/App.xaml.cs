using StegoRevealer.WinUi.ViewModels;
using StegoRevealer.WinUi.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
