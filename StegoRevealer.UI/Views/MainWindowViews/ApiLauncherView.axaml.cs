using Avalonia.Controls;
using Avalonia.Interactivity;
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

    private async void StartServer_Click(object sender, RoutedEventArgs e)
    {

    }
}