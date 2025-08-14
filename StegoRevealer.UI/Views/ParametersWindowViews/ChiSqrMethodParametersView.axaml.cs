using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

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

    private void FilterForInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private void FilterForDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private void FilterForPositiveInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private void FilterForPositiveDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
}
