using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class InfoPageView : UserControl
{
    private InfoPageViewModel _vm = null!;

    
    public InfoPageView()
    {
        InitializeComponent();

        base.Loaded += InfoPageView_Loaded;
    }

    private void InfoPageView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<InfoPageViewModel>(this.DataContext);
    }
}
