using System.Reactive;
using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class ChiSqrMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public ChiSqrMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public ChiSqrMethodParametersViewModel() : base() { }

    public void SetParameters(object parameters)
    {
        var chiSqrParameters = parameters as ChiSquareParameters;
        if (chiSqrParameters is null)
            return;

        ChiSqrParamsDto chiSqrParamsDto = new ChiSqrParamsDto(chiSqrParameters);

        Visualize = chiSqrParamsDto.Visualize;
        ExcludeZeroPairs = chiSqrParamsDto.ExcludeZeroPairs;
        UseUnifiedCathegories = chiSqrParamsDto.UseUnifiedCathegories;
        UsePreviousCnums = chiSqrParamsDto.UsePreviousCnums;
        UnifyingCathegoriesThresholdValue = chiSqrParamsDto.UnifyingCathegoriesThreshold;
        ChannelRedChecked = chiSqrParamsDto.Channels.Contains(ImgChannel.Red);
        ChannelGreenChecked = chiSqrParamsDto.Channels.Contains(ImgChannel.Green);
        ChannelBlueChecked = chiSqrParamsDto.Channels.Contains(ImgChannel.Blue);
        TraverseHorizontal = chiSqrParamsDto.TraverseType is TraverseType.Horizontal;
        TraverseVertical = chiSqrParamsDto.TraverseType is TraverseType.Vertical;
        PValueThresholdValue = chiSqrParamsDto.Threshold;
        BlockWidthValue = chiSqrParamsDto.BlockWidth;
        BlockHeightValue = chiSqrParamsDto.BlockHeight;
    }

    public object CollectParameters()
    {
        ChiSqrParamsDto result = new ChiSqrParamsDto();

        result.Visualize = Visualize;
        result.ExcludeZeroPairs = ExcludeZeroPairs;
        result.UsePreviousCnums = UsePreviousCnums;
        result.UseUnifiedCathegories = UseUnifiedCathegories;

        result.UnifyingCathegoriesThreshold = UnifyingCathegoriesThresholdValue;

        result.Channels = new();
        if (ChannelRedChecked)
            result.Channels.Add(ImgChannel.Red);
        if (ChannelGreenChecked)
            result.Channels.Add(ImgChannel.Green);
        if (ChannelBlueChecked)
            result.Channels.Add(ImgChannel.Blue);

        result.TraverseType = TraverseType.Horizontal;
        if (!TraverseHorizontal && TraverseVertical)
            result.TraverseType = TraverseType.Vertical;

        result.Threshold = PValueThresholdValue;
        result.BlockWidth = BlockWidthValue;
        result.BlockHeight = BlockHeightValue;

        return result;
    }


    // Поля - параметры

    private bool _visualize = true;
    public bool Visualize
    {
        get => _visualize;
        set => this.RaiseAndSetIfChanged(ref _visualize, value);
    }

    private bool _excludeZeroPairs = true;
    public bool ExcludeZeroPairs
    {
        get => _excludeZeroPairs;
        set => this.RaiseAndSetIfChanged(ref _excludeZeroPairs, value);
    }

    private bool _useUnifiedCathegories = true;
    public bool UseUnifiedCathegories
    {
        get => _useUnifiedCathegories;
        set => this.RaiseAndSetIfChanged(ref _useUnifiedCathegories, value);
    }

    private bool _usePreviousCnums = true;
    public bool UsePreviousCnums
    {
        get => _usePreviousCnums;
        set => this.RaiseAndSetIfChanged(ref _usePreviousCnums, value);
    }

    private int _unifyingCathegoriesThresholdValue = 4;
    public int UnifyingCathegoriesThresholdValue
    {
        get => _unifyingCathegoriesThresholdValue;
        set => this.RaiseAndSetIfChanged(ref _unifyingCathegoriesThresholdValue, value);
    }

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

    private bool _traverseHorizontal = true;
    public bool TraverseHorizontal
    {
        get => _traverseHorizontal;
        set => this.RaiseAndSetIfChanged(ref _traverseHorizontal, value);
    }

    private bool _traverseVertical = false;
    public bool TraverseVertical
    {
        get => _traverseVertical;
        set => this.RaiseAndSetIfChanged(ref _traverseVertical, value);
    }

    private double _pValueThresholdValue = 0.95;
    public double PValueThresholdValue
    {
        get => _pValueThresholdValue;
        set => this.RaiseAndSetIfChanged(ref _pValueThresholdValue, value);
    }

    private int _blockWidthValue = 1;
    public int BlockWidthValue
    {
        get => _blockWidthValue;
        set => this.RaiseAndSetIfChanged(ref _blockWidthValue, value);
    }

    private int _blockHeightValue = 1;
    public int BlockHeightValue
    {
        get => _blockHeightValue;
        set => this.RaiseAndSetIfChanged(ref _blockHeightValue, value);
    }
}
