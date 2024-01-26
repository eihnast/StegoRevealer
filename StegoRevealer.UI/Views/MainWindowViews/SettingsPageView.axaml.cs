using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class SettingsPageView : UserControl
{
    private SettingsPageViewModel _vm = null!;

    public SettingsPageView()
    {
        InitializeComponent();

        base.Loaded += SettingsPageView_Loaded;
    }

    private void SettingsPageView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<SettingsPageViewModel>(this.DataContext);
    }
}
