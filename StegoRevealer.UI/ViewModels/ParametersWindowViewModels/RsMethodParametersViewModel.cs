﻿using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.Interfaces;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class RsMethodParametersViewModel : ParametersWindowViewModelBaseChild, IParametersViewModel
{
    public RsMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public RsMethodParametersViewModel() : base() { }

    public void SetParameters(object parameters)
    {
        var rsParameters = parameters as RsParameters;
        if (rsParameters is null)
            return;

        RsParamsDto rsParamsDto = new RsParamsDto(rsParameters);

        PixelsGroupLengthValue = rsParamsDto.PixelsGroupLength;

        string mask = string.Join("", rsParamsDto.FlippingMask.Select(val => val.ToString()));
        FlippingMaskValue = mask;

        ChannelRedChecked = rsParamsDto.Channels.Contains(ImgChannel.Red);
        ChannelGreenChecked = rsParamsDto.Channels.Contains(ImgChannel.Green);
        ChannelBlueChecked = rsParamsDto.Channels.Contains(ImgChannel.Blue);

        TraverseHorizontal = rsParamsDto.TraverseType is TraverseType.Horizontal;
        TraverseVertical = rsParamsDto.TraverseType is TraverseType.Vertical;
        BlockWidthValue = rsParamsDto.BlockWidth;
        BlockHeightValue = rsParamsDto.BlockHeight;
    }

    public object CollectParameters()
    {
        RsParamsDto result = new RsParamsDto();

        result.PixelsGroupLength = PixelsGroupLengthValue;

        string currentNum = string.Empty;
        var mask = new List<int>();
        for (int i = 0; i < FlippingMaskValue.Length; i++)
        {
            currentNum += FlippingMaskValue[i];
            if (currentNum == "-")
                continue;
            if (currentNum[..^0] == "0" || currentNum[..^0] == "1")
                if (int.TryParse(currentNum, out int value))
                    mask.Add(value);
            currentNum = string.Empty;
        }
        result.FlippingMask = mask.ToArray();

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

        return result;
    }


    // Поля - параметры

    private int _pixelsGroupLengthValue = 4;
    public int PixelsGroupLengthValue
    {
        get => _pixelsGroupLengthValue;
        set => this.RaiseAndSetIfChanged(ref _pixelsGroupLengthValue, value);
    }

    private string _flippingMaskValue = "1001";
    public string FlippingMaskValue
    {
        get => _flippingMaskValue;
        set => this.RaiseAndSetIfChanged(ref _flippingMaskValue, value);
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
