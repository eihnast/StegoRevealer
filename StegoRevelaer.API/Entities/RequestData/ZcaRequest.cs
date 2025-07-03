using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class ZcaRequest : BaseAnalysisRequest
{
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;
    public ImgChannel[] Channels { get; set; }
        = new ImgChannel[] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
    public double RatioThreshold { get; set; } = 0.008;
    public bool UseOverallCompression { get; set; } = true;
    public CompressingAlgorithm CompressingAlgorithm { get; set; } = CompressingAlgorithm.ZIP;
    public int? BlockWidth { get; set; }
    public int? BlockHeight { get; set; }

    public ZcaParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new ZcaParameters(imgHandler)
        {
            TraverseType = TraverseType,
            RatioThreshold = RatioThreshold,
            UseOverallCompression = UseOverallCompression,
            CompressingAlgorithm = CompressingAlgorithm
        };

        parameters.Channels.Clear();
        parameters.Channels.AddRange(Channels);

        parameters.BlockWidth = BlockWidth ?? 16;
        parameters.BlockHeight = BlockHeight ?? 16;

        return parameters;
    }
}
