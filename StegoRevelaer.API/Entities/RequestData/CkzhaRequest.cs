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
        = new ImgChannel[] { ImgChannel.Blue };
    public (int, int)[] AnalysisCoeffs { get; set; } = 
    [
        (2, 3),
        (2, 4),
        (3, 4)
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
        foreach (var coeff in AnalysisCoeffs)
        {
            parameters.AnalysisCoeffs.Add(new ScIndexPair(coeff.Item1, coeff.Item2));
        }

        return parameters;
    }
}
