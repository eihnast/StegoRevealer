using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class KzhaMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public KzhaMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        CreateCoeffsLists();
    }

    [Experimental]
    public KzhaMethodParametersViewModel() : base() 
    {
        CreateCoeffsLists();
    }

    public void SetParameters(object parameters)
    {
        var kzhaParameters = parameters as KzhaParameters;
        if (kzhaParameters is null)
            return;

        KzhaParamsDto kzhaParamsDto = new KzhaParamsDto(kzhaParameters);

        TryToExtract = kzhaParamsDto.TryToExtract;
        CutCoefficientValue = kzhaParamsDto.CutCoefficient;
        ThresholdValue = kzhaParamsDto.Threshold;
        TraverseHorizontal = kzhaParamsDto.TraverseType is TraverseType.Horizontal;
        TraverseVertical = kzhaParamsDto.TraverseType is TraverseType.Vertical;
        ChannelRedChecked = kzhaParamsDto.Channels.Contains(ImgChannel.Red);
        ChannelGreenChecked = kzhaParamsDto.Channels.Contains(ImgChannel.Green);
        ChannelBlueChecked = kzhaParamsDto.Channels.Contains(ImgChannel.Blue);

        foreach (var coeff in kzhaParamsDto.AnalysisCoeffs)
            CoeffToSelected(coeff);
    }

    public object CollectParameters()
    {
        KzhaParamsDto result = new KzhaParamsDto();

        result.TryToExtract = TryToExtract;
        result.CutCoefficient = CutCoefficientValue;
        result.Threshold = ThresholdValue;

        result.TraverseType = TraverseType.Horizontal;
        if (!TraverseHorizontal && TraverseVertical)
            result.TraverseType = TraverseType.Vertical;

        result.Channels = new();
        if (ChannelRedChecked)
            result.Channels.Add(ImgChannel.Red);
        if (ChannelGreenChecked)
            result.Channels.Add(ImgChannel.Green);
        if (ChannelBlueChecked)
            result.Channels.Add(ImgChannel.Blue);

        result.AnalysisCoeffs = new List<ScIndexPair>(SelectedCoeffs.Items);

        return result;
    }

    private void CreateCoeffsLists()
    {
        foreach (var field in typeof(HidingCoefficients).GetFields())
        {
            var value = field.GetValue(typeof(HidingCoefficients)) as ScIndexPair?;
            if (value is not null)
                AvailableCoeffs.Add(value.Value);
        }
    }
    public void CoeffToAvailable(ScIndexPair coeff)
    {
        SelectedCoeffs.Remove(coeff);
        AvailableCoeffs.Add(coeff);
    }
    public void CoeffToSelected(ScIndexPair coeff)
    {
        AvailableCoeffs.Remove(coeff);
        SelectedCoeffs.Add(coeff);
    }


    // Поля - параметры

    private bool _tryToExtract = true;
    public bool TryToExtract
    {
        get => _tryToExtract;
        set => this.RaiseAndSetIfChanged(ref _tryToExtract, value);
    }

    private double _cutCoefficientValue = 0.80;
    public double CutCoefficientValue
    {
        get => _cutCoefficientValue;
        set => this.RaiseAndSetIfChanged(ref _cutCoefficientValue, value);
    }

    private double _thresholdValue = 120.0;
    public double ThresholdValue
    {
        get => _thresholdValue;
        set => this.RaiseAndSetIfChanged(ref _thresholdValue, value);
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

    private SourceList<ScIndexPair> _selectedCoeffs = new();
    public SourceList<ScIndexPair> SelectedCoeffs
    {
        get => _selectedCoeffs;
        set => this.RaiseAndSetIfChanged(ref _selectedCoeffs, value);
    }

    private SourceList<ScIndexPair> _availableCoeffs = new();
    public SourceList<ScIndexPair> AvailableCoeffs
    {
        get => _availableCoeffs;
        set => this.RaiseAndSetIfChanged(ref _availableCoeffs, value);
    }
}
