using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class RsMethodParametersView : UserControl
{
    private RsMethodParametersViewModel _vm = null!;

    public RsMethodParametersView()
    {
        InitializeComponent();

        base.Loaded += RsMethodParametersView_Loaded;
    }

    private void RsMethodParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<RsMethodParametersViewModel>(this.DataContext);
    }
}
