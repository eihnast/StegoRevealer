using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class KzhaMethodParametersView : UserControl
{
    private KzhaMethodParametersViewModel _vm = null!;

    public KzhaMethodParametersView()
    {
        InitializeComponent();

        base.Loaded += KzhaMethodParametersView_Loaded;
    }

    private void KzhaMethodParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<KzhaMethodParametersViewModel>(this.DataContext);
    }
}
