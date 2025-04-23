using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.ParamsHelpers;

namespace StegoRevealer.UI.Lib.MethodsHelper;

/// <summary>
/// DTO для параметров стегоаналитического SPA: 
/// <see cref="SpaParameters"/>
/// </summary>
public class SpaParamsDto : IParamsDto<SpaParameters>
{
    public PairDirection Direction { get; set; } = PairDirection.Horizontal;

    public bool UseDoubleDirection { get; set; } = true;

    public UniqueList<ImgChannel> Channels { get; set; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    public SpaParamsDto() { }

    public SpaParamsDto(SpaParameters parameters)
    {
        Channels = new();
        foreach (var channel in parameters.Channels)
            Channels.Add(channel);

        UseDoubleDirection = parameters.UseDoubleDirection;
        Direction = parameters.Direction;
    }

    /// <inheritdoc/>
    public void FillParameters(ref SpaParameters parameters)
    {
        if (parameters is null)
            return;

        parameters.Channels.Clear();
        foreach (var channel in Channels)
            parameters.Channels.Add(channel);

        parameters.UseDoubleDirection = UseDoubleDirection;
        parameters.Direction = Direction;
    }
}
