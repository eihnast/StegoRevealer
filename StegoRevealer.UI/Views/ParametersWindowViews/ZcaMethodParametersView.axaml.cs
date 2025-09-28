using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class ZcaMethodParametersView : UserControl
{
    private ZcaMethodParametersViewModel _vm = null!;

    public ZcaMethodParametersView()
    {
        InitializeComponent();

        base.Loaded += ZcaMethodParametersView_Loaded;
    }

    private void ZcaMethodParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ZcaMethodParametersViewModel>(this.DataContext);
    }


    #region Input Filtering

    private void FilterForInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private void FilterForInteger_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private async void FilterForInteger_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);

    private void FilterForDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private void FilterForDouble_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private async void FilterForDouble_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);

    private void FilterForPositiveInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private void FilterForPositiveInteger_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private async void FilterForPositiveInteger_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);

    private void FilterForPositiveDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
    private void FilterForPositiveDouble_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
    private async void FilterForPositiveDouble_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);

    #endregion
}
