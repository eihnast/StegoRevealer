using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class ExtractorView : UserControl
{
    private ExtractorViewModel _vm = null!;

    public ExtractorView()
    {
        InitializeComponent();

        base.Loaded += ExtractorView_Loaded;
    }

    private void ExtractorView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ExtractorViewModel>(this.DataContext);
    }
}
