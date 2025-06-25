using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevelaer.API.Entities.RequestData;

public class CkzhaRequest : BaseAnalysisRequest
{
    public double Threshold { get; set; } = 20;
    public double CutCoefficient { get; set; } = 0.35;
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;
    public ImgChannel[] Channels { get; set; }
        = new ImgChannel[] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
    public ScIndexPair[] AnalysisCoeffs { get; set; } = 
    [
        HidingCoefficients.Coeff34,
        HidingCoefficients.Coeff35,
        HidingCoefficients.Coeff45
    ];
    public bool TryToExtract { get; set; } = true;
    public bool LoggingCSequences { get; set; } = false;

    public KzhaParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new KzhaParameters(imgHandler)
        {
            TraverseType = TraverseType,
            Threshold = Threshold,
            CutCoefficient = CutCoefficient,
            TryToExtract = TryToExtract,
            LoggingCSequences = LoggingCSequences
        };

        parameters.Channels.Clear();
        parameters.Channels.AddRange(Channels);

        parameters.AnalysisCoeffs.Clear();
        parameters.AnalysisCoeffs.AddRange(AnalysisCoeffs);

        return parameters;
    }
}
