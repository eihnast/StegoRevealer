using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class ComplexSsaRequest : BaseAnalysisRequest
{
    // Пока нет параметров

    public ComplexSaMethodParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new ComplexSaMethodParameters(imgHandler);

        return parameters;
    }
}
