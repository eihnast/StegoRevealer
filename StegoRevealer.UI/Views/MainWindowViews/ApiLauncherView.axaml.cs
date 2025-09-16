using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class ApiLauncherView : UserControl
{
    private ApiLauncherViewModel _vm = null!;

    public ApiLauncherView()
    {
        InitializeComponent();

        base.Loaded += ApiLauncherView_Loaded;
    }

    private void ApiLauncherView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ApiLauncherViewModel>(this.DataContext);
    }

    private async void LaunchApiBtn_Click(object sender, RoutedEventArgs e)
    {
        LaunchApiBtn.IsEnabled = false;
        LoadingOverlay.IsVisible = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

        if (_vm.IsApiLaunched)
        {
            await _vm.StopApi();
            SettingsPanel.IsEnabled = true;
        }
        else
        {
            SettingsPanel.IsEnabled = false;
            await _vm.LaunchApi();
        }

        LoadingOverlay.IsVisible = false;
        LaunchApiBtn.IsEnabled = true;
    }
}
