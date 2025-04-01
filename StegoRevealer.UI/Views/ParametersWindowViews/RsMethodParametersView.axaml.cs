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

    private static void FilterForInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private static void FilterForDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private static void FilterForPositiveInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private static void FilterForPositiveDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
}
