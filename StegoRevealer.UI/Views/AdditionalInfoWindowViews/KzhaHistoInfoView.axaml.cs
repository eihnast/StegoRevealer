using Avalonia.Controls;
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
}