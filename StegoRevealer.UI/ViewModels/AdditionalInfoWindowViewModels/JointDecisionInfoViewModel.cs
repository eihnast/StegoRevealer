using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

public class JointDecisionInfoViewModel : AdditionalInfoWindowViewModelBaseChild
{
    public JointDecisionInfoViewModel(AdditionalInfoWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public JointDecisionInfoViewModel() : base() { }


    public void ProcessResults(ChiSquareResult csaRes, RsResult rsRes)
    {
        CsaMessageRelativeVolume = csaRes.MessageRelativeVolume;
        RsMessageRelativeVolume = rsRes.MessageRelativeVolume;
    }


    private double _csaMessageRelativeVolume = 0.0;
    public double CsaMessageRelativeVolume
    {
        get => _csaMessageRelativeVolume;
        set => this.RaiseAndSetIfChanged(ref _csaMessageRelativeVolume, value);
    }

    private double _rsMessageRelativeVolume = 0.0;
    public double RsMessageRelativeVolume
    {
        get => _rsMessageRelativeVolume;
        set => this.RaiseAndSetIfChanged(ref _rsMessageRelativeVolume, value);
    }
}
