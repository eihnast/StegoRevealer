using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class RsRequest : BaseAnalysisRequest
{
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;
    public ImgChannel[] Channels { get; set; }
        = new ImgChannel[] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
    public int? BlockWidth { get; set; }
    public int? BlockHeight { get; set; }

    public RsParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new RsParameters(imgHandler)
        {
            TraverseType = TraverseType
        };

        parameters.Channels.Clear();
        parameters.Channels.AddRange(Channels);

        parameters.BlockWidth = BlockWidth ?? imgHandler.Width;
        parameters.BlockHeight = BlockHeight ?? 1;

        return parameters;
    }
}
