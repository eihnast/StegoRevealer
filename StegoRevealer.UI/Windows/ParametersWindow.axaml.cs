using Avalonia.Controls;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels;

namespace StegoRevealer.UI.Windows;

public partial class ParametersWindow : Window
{
    private ParametersWindowViewModel _vm = null!;

    public ParametersWindow()
    {
        InitializeComponent();
        this.Loaded += ParametersWindow_Loaded;
    }

    private void ParametersWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ParametersWindowViewModel>(this.DataContext);
        this.Closing += (object? sender, WindowClosingEventArgs e) => _vm.FillParametersDtoAction();
    }
}