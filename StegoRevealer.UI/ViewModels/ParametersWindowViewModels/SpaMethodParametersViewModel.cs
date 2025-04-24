using ReactiveUI;
using System.Reactive;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class SpaMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public SpaMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public SpaMethodParametersViewModel() : base() { }

    public void SetParameters(object parameters)
    {
        var spaParameters = parameters as SpaParameters;
        if (spaParameters is null)
            return;

        SpaParamsDto rsParamsDto = new SpaParamsDto(spaParameters);

        UseDoubleDirection = rsParamsDto.UseDoubleDirection;

        ChannelRedChecked = rsParamsDto.Channels.Contains(ImgChannel.Red);
        ChannelGreenChecked = rsParamsDto.Channels.Contains(ImgChannel.Green);
        ChannelBlueChecked = rsParamsDto.Channels.Contains(ImgChannel.Blue);

        DirectionHorizontal = rsParamsDto.Direction is PairDirection.Horizontal;
        DirectionVertical = rsParamsDto.Direction is PairDirection.Vertical;
        DirectionDiagonal = rsParamsDto.Direction is PairDirection.Diagonal;
    }

    public object CollectParameters()
    {
        SpaParamsDto result = new SpaParamsDto();

        result.Channels = new();
        if (ChannelRedChecked)
            result.Channels.Add(ImgChannel.Red);
        if (ChannelGreenChecked)
            result.Channels.Add(ImgChannel.Green);
        if (ChannelBlueChecked)
            result.Channels.Add(ImgChannel.Blue);

        result.Direction = PairDirection.Horizontal;
        if (!DirectionHorizontal && !DirectionDiagonal && DirectionVertical)
            result.Direction = PairDirection.Vertical;
        else if (!DirectionHorizontal && !DirectionVertical && DirectionDiagonal)
            result.Direction = PairDirection.Diagonal;

        result.UseDoubleDirection = UseDoubleDirection;

        return result;
    }


    // Поля - параметры

    private bool _channelRedChecked = true;
    public bool ChannelRedChecked
    {
        get => _channelRedChecked;
        set => this.RaiseAndSetIfChanged(ref _channelRedChecked, value);
    }

    private bool _channelGreenChecked = true;
    public bool ChannelGreenChecked
    {
        get => _channelGreenChecked;
        set => this.RaiseAndSetIfChanged(ref _channelGreenChecked, value);
    }

    private bool _channelBlueChecked = true;
    public bool ChannelBlueChecked
    {
        get => _channelBlueChecked;
        set => this.RaiseAndSetIfChanged(ref _channelBlueChecked, value);
    }

    private bool _directionHorizontal = true;
    public bool DirectionHorizontal
    {
        get => _directionHorizontal;
        set => this.RaiseAndSetIfChanged(ref _directionHorizontal, value);
    }

    private bool _directionVertical = false;
    public bool DirectionVertical
    {
        get => _directionVertical;
        set => this.RaiseAndSetIfChanged(ref _directionVertical, value);
    }

    private bool _directionDiagonal = false;
    public bool DirectionDiagonal
    {
        get => _directionDiagonal;
        set => this.RaiseAndSetIfChanged(ref _directionDiagonal, value);
    }

    private bool _useDoubleDirection = true;
    public bool UseDoubleDirection
    {
        get => _useDoubleDirection;
        set => this.RaiseAndSetIfChanged(ref _useDoubleDirection, value);
    }
}
