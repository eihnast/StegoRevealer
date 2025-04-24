using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class ZcaMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public ZcaMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public ZcaMethodParametersViewModel() : base() { }

    public void SetParameters(object parameters)
    {
        var zcaParameters = parameters as ZcaParameters;
        if (zcaParameters is null)
            return;

        ZcaParamsDto zcaParamsDto = new ZcaParamsDto(zcaParameters);

        RatioThresholdValue = zcaParamsDto.RatioThreshold;

        ChannelRedChecked = zcaParamsDto.Channels.Contains(ImgChannel.Red);
        ChannelGreenChecked = zcaParamsDto.Channels.Contains(ImgChannel.Green);
        ChannelBlueChecked = zcaParamsDto.Channels.Contains(ImgChannel.Blue);

        TraverseHorizontal = zcaParamsDto.TraverseType is TraverseType.Horizontal;
        TraverseVertical = zcaParamsDto.TraverseType is TraverseType.Vertical;

        BlockWidthValue = zcaParamsDto.BlockWidth;
        BlockHeightValue = zcaParamsDto.BlockHeight;

        CompressingAlgorithmZip = zcaParamsDto.CompressingAlgorithm is CompressingAlgorithm.ZIP;
        CompressingAlgorithmBZip2 = zcaParamsDto.CompressingAlgorithm is CompressingAlgorithm.BZIP2;
        CompressingAlgorithmGZip = zcaParamsDto.CompressingAlgorithm is CompressingAlgorithm.GZIP;

        UseOverallCompression = zcaParamsDto.UseOverallCompression;
    }

    public object CollectParameters()
    {
        ZcaParamsDto result = new ZcaParamsDto();

        result.RatioThreshold = RatioThresholdValue;

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

        result.BlockWidth = BlockWidthValue;
        result.BlockHeight = BlockHeightValue;

        result.UseOverallCompression = UseOverallCompression;

        result.CompressingAlgorithm = CompressingAlgorithm.ZIP;
        if (!CompressingAlgorithmZip && !CompressingAlgorithmGZip && CompressingAlgorithmBZip2)
            result.CompressingAlgorithm = CompressingAlgorithm.BZIP2;
        else if (!CompressingAlgorithmZip && !CompressingAlgorithmBZip2 && CompressingAlgorithmGZip)
            result.CompressingAlgorithm = CompressingAlgorithm.GZIP;

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

    private double _ratioThresholdValue = 0.08;
    public double RatioThresholdValue
    {
        get => _ratioThresholdValue;
        set => this.RaiseAndSetIfChanged(ref _ratioThresholdValue, value);
    }

    private bool _compressingAlgorithmZip = true;
    public bool CompressingAlgorithmZip
    {
        get => _compressingAlgorithmZip;
        set => this.RaiseAndSetIfChanged(ref _compressingAlgorithmZip, value);
    }

    private bool _compressingAlgorithmBZip2 = false;
    public bool CompressingAlgorithmBZip2
    {
        get => _compressingAlgorithmBZip2;
        set => this.RaiseAndSetIfChanged(ref _compressingAlgorithmBZip2, value);
    }

    private bool _compressingAlgorithmGZip = false;
    public bool CompressingAlgorithmGZip
    {
        get => _compressingAlgorithmGZip;
        set => this.RaiseAndSetIfChanged(ref _compressingAlgorithmGZip, value);
    }

    private bool _useOverallCompression = true;
    public bool UseOverallCompression
    {
        get => _useOverallCompression;
        set => this.RaiseAndSetIfChanged(ref _useOverallCompression, value);
    }
}
