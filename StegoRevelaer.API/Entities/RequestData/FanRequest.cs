using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class FanRequest : BaseAnalysisRequest
{
    public double Threshold { get; set; } = 3.401714170610843;

    public FanParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new FanParameters(imgHandler)
        {
            Threshold = Threshold
        };

        return parameters;
    }
}
