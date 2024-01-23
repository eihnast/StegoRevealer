using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class HiderView : UserControl
{
    private HiderViewModel _vm = null!;

    public HiderView()
    {
        InitializeComponent();

        base.Loaded += HiderView_Loaded;
    }

    private void HiderView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<HiderViewModel>(this.DataContext);
    }
}
