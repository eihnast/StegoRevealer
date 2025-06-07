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

        var rs = _vm.RsMessageRelativeVolume;
        var csa = _vm.CsaMessageRelativeVolume;
        if (rs <= 4.0 && csa <= 0.1)
            RsCsaGraphO1.SetPoint(rs, csa);
        else
            RsCsaGraphFull.SetPoint(rs, csa);

        ResultLabel.Content = _vm.GetDecision();
    }
}
