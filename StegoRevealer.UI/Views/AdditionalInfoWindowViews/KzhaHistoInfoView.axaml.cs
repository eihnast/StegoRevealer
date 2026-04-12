using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

namespace StegoRevealer.UI.Views.AdditionalInfoWindowViews;

public partial class KzhaHistoInfoView : UserControl
{
    private KzhaHistoInfoViewModel _vm = null!;

    public KzhaHistoInfoView()
    {
        InitializeComponent();

        base.Loaded += KzhaHistoInfoView_Loaded;
    }

    private void KzhaHistoInfoView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<KzhaHistoInfoViewModel>(this.DataContext);
        _vm.SelectedIndexPairIndex = 0;
    }

    private void ResetZoomBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Histo.ResetZoom();
    }

    private void BackPreviousZoomBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Histo.ZoomBack();
    }

    private async void SetHorizontalTraverse(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SetHorizontalTraverse();
    }
    private async void SetVerticalTraverse(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SetVerticalTraverse();
    }

    private void VerticalTraverseChoice_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb)
            return;

        if (tb.IsChecked == true)
        {
            SetVerticalTraverse(sender, e);
        }
        else if (tb.IsChecked == false)
        {
            SetHorizontalTraverse(sender, e);
        }
    }

    private void HorizontalTraverseChoice_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb)
            return;

        if (tb.IsChecked == true)
        {
            SetVerticalTraverse(sender, e);
        }
        else if (tb.IsChecked == false)
        {
            SetHorizontalTraverse(sender, e);
        }
    }
}
