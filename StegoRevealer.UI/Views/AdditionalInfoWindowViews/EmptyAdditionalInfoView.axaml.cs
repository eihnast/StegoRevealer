using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

namespace StegoRevealer.UI.Views.AdditionalInfoWindowViews;

public partial class EmptyAdditionalInfoView : UserControl
{
    private EmptyAdditionalInfoViewModel _vm = null!;

    public EmptyAdditionalInfoView()
    {
        InitializeComponent();

        base.Loaded += EmptyAdditionalInfoView_Loaded;
    }

    private void EmptyAdditionalInfoView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<EmptyAdditionalInfoViewModel>(this.DataContext);
    }
}