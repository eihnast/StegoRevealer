using Avalonia.Controls;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels;

namespace StegoRevealer.UI.Windows;

public partial class AdditionalInfoWindow : Window
{
    private AdditionalInfoWindowViewModel _vm = null!;

    public AdditionalInfoWindow()
    {
        InitializeComponent();
        this.Loaded += AdditionalInfoWindow_Loaded;
    }

    private void AdditionalInfoWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<AdditionalInfoWindowViewModel>(this.DataContext);
        // this.Closing += (object? sender, WindowClosingEventArgs e) => _vm.FillParametersDtoAction();
    }
}
