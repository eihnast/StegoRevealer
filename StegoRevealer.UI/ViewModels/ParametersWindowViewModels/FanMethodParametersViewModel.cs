using ReactiveUI;
using System.Reactive;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class FanMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public FanMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public FanMethodParametersViewModel() : base() { }

    public void SetParameters(object parameters)
    {
        var fanParameters = parameters as FanParameters;
        if (fanParameters is null)
            return;

        FanParamsDto fanParamsDto = new FanParamsDto(fanParameters);

        Threshold = fanParamsDto.Threshold;
    }

    public object CollectParameters()
    {
        FanParamsDto result = new FanParamsDto();

        result.Threshold = Threshold;

        return result;
    }


    // Поля - параметры

    private double _threshold = 3.401714170610843;
    public double Threshold
    {
        get => _threshold;
        set => this.RaiseAndSetIfChanged(ref _threshold, value);
    }
}
