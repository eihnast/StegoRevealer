using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class ChiSqrMethodParametersView : UserControl
{
    private ChiSqrMethodParametersViewModel _vm = null!;

    public ChiSqrMethodParametersView()
    {
        InitializeComponent();

        base.Loaded += ChiSqrMethodParametersView_Loaded;
    }

    private void ChiSqrMethodParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ChiSqrMethodParametersViewModel>(this.DataContext);
    }
}
