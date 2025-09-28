using Avalonia.Controls;
using Avalonia.Data;
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

        if (_vm.IsApiLaunched)
            BindLaunchBtnStopText();
        else
            BindLaunchBtnStartText();
    }

    private async void LaunchApiBtn_Click(object sender, RoutedEventArgs e)
    {
        LaunchApiBtn.IsEnabled = false;
        LoadingOverlay.IsVisible = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

        if (_vm.IsApiLaunched)
        {
            BindLaunchBtnStartText();
            await _vm.StopApi();
            SettingsPanel.IsEnabled = true;
        }
        else
        {
            BindLaunchBtnStopText();
            SettingsPanel.IsEnabled = false;
            await _vm.LaunchApi();
        }

        LoadingOverlay.IsVisible = false;
        LaunchApiBtn.IsEnabled = true;
    }

    private void RemoveLaunchBtnBinding() => BindingOperations.GetBindingExpressionBase(LaunchApiBtn, Button.ContentProperty)?.Dispose();
    private void BindLaunchBtnStartText()
    {
        RemoveLaunchBtnBinding();
        LaunchApiBtn.Bind(Button.ContentProperty, new Binding
        {
            Source = _vm,
            Path = "L[ApiLauncherTab.LaunchBtn.Start]",
            Mode = BindingMode.TwoWay
        });
    }
    private void BindLaunchBtnStopText()
    {
        RemoveLaunchBtnBinding();
        LaunchApiBtn.Bind(Button.ContentProperty, new Binding
        {
            Source = _vm,
            Path = "L[ApiLauncherTab.LaunchBtn.Stop]",
            Mode = BindingMode.TwoWay
        });
    }
}
