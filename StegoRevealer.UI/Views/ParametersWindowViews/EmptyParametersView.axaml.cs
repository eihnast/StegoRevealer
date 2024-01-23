using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class EmptyParametersView : UserControl
{
    private EmptyParametersViewModel _vm = null!;

    public EmptyParametersView()
    {
        InitializeComponent();

        base.Loaded += EmptyParametersView_Loaded;
    }

    private void EmptyParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<EmptyParametersViewModel>(this.DataContext);
    }
}
