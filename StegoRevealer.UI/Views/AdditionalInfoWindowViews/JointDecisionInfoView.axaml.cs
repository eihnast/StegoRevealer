using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

namespace StegoRevealer.UI.Views.AdditionalInfoWindowViews;

public partial class JointDecisionInfoView : UserControl
{
    private JointDecisionInfoViewModel _vm = null!;

    public JointDecisionInfoView()
    {
        InitializeComponent();

        base.Loaded += JointDecisionInfoView_Loaded;
    }

    private void JointDecisionInfoView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<JointDecisionInfoViewModel>(this.DataContext);
    }
}