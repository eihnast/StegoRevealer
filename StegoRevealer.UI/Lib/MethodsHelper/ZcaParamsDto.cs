using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.ParamsHelpers;

namespace StegoRevealer.UI.Lib.MethodsHelper;

/// <summary>
/// DTO для параметров стегоаналитического ZCA: 
/// <see cref="ZcaParameters"/>
/// </summary>
public class ZcaParamsDto : IParamsDto<ZcaParameters>
{
    public double RatioThreshold { get; set; } = 0.008;

    public bool UseOverallCompression { get; set; } = true;

    public CompressingAlgorithm CompressingAlgorithm { get; set; } = CompressingAlgorithm.ZIP;

    public UniqueList<ImgChannel> Channels { get; set; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;

    public int BlockWidth { get; set; } = 1;
    public int BlockHeight { get; set; } = 1;

    public ZcaParamsDto() { }

    public ZcaParamsDto(ZcaParameters parameters)
    {
        Channels = new();
        foreach (var channel in parameters.Channels)
            Channels.Add(channel);

        BlockHeight = parameters.BlockHeight;
        BlockWidth = parameters.BlockWidth;

        RatioThreshold = parameters.RatioThreshold;
        UseOverallCompression = parameters.UseOverallCompression;
        CompressingAlgorithm = parameters.CompressingAlgorithm;

        TraverseType = parameters.TraverseType;
    }

    /// <inheritdoc/>
    public void FillParameters(ref ZcaParameters parameters)
    {
        if (parameters is null)
            return;

        parameters.Channels.Clear();
        foreach (var channel in Channels)
            parameters.Channels.Add(channel);

        parameters.BlockHeight = BlockHeight;
        parameters.BlockWidth = BlockWidth;

        parameters.RatioThreshold = RatioThreshold;
        parameters.UseOverallCompression = UseOverallCompression;
        parameters.CompressingAlgorithm = CompressingAlgorithm;

        parameters.TraverseType = TraverseType;
    }
}
