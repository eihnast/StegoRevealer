using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class MainView : UserControl
{
    private MainViewModel _vm = null!;

    public MainView()
    {
        InitializeComponent();

        base.Loaded += MainView_Loaded;
    }

    private void MainView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<MainViewModel>(this.DataContext);
    }
}
