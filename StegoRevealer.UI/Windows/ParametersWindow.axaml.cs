using Avalonia.Controls;
using StegoRevealer.UI.ViewModels;

namespace StegoRevealer.UI.Windows;

public partial class ParametersWindow : Window
{
    public ParametersWindow()
    {
        InitializeComponent();
        this.Closing += ParametersWindow_Closing;
    }

    private void ParametersWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        var vm = this.DataContext as ParametersWindowViewModel;
    }
}