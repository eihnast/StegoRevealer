﻿using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.ParamsHelpers;

namespace StegoRevealer.UI.Lib.MethodsHelper;

/// <summary>
/// DTO для параметров стегоаналитического метода Regular-Singular: 
/// <see cref="RsParameters"/>
/// </summary>
public class RsParamsDto : IParamsDto<RsParameters>
{
    public UniqueList<ImgChannel> Channels { get; set; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    public int PixelsGroupLength { get; set; } = 4;

    public int[] FlippingMask { get; set; } = new int[4] { 1, 0, 0, 1 };


    public RsParamsDto() { }

    public RsParamsDto(RsParameters parameters)
    {
        Channels = new();
        foreach (var channel in parameters.Channels)
            Channels.Add(channel);

        PixelsGroupLength = parameters.PixelsGroupLength;
        FlippingMask = (int[])parameters.FlippingMask.Clone();
    }

    /// <inheritdoc/>
    public void FillParameters(ref RsParameters parameters)
    {
        if (parameters is null)
            return;

        parameters.Channels.Clear();
        foreach (var channel in Channels)
            parameters.Channels.Add(channel);

        parameters.PixelsGroupLength = PixelsGroupLength;
        parameters.FlippingMask = (int[])FlippingMask.Clone();
    }
}
