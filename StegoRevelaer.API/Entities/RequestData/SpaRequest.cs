using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class SpaRequest : BaseAnalysisRequest
{
    public SpaVersion MethodVersion { get; set; } = SpaVersion.Original;
    public PairDirection Direction { get; set; } = PairDirection.Horizontal;
    public bool UseDoubleDirection { get; set; } = true;
    public ImgChannel[] Channels { get; set; }
        = new ImgChannel[] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    public SpaParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new SpaParameters(imgHandler)
        {
            MethodVersion = MethodVersion,
            Direction = Direction,
            UseDoubleDirection = UseDoubleDirection
        };

        parameters.Channels.Clear();
        parameters.Channels.AddRange(Channels);

        return parameters;
    }
}
