using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;

/// <summary>
/// Результаты метода стегоанализа SPA
/// </summary>
public class SpaResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// Усреднённая вероятность наличия скрытого сообщения
    /// </summary>
    public double AvgHidedDataProbability { get; set; } = 0.0;

    /// <summary>
    /// Вероятности наличия скрытого сообщения по каналам
    /// </summary>
    public Dictionary<ImgChannel, double> HidedDataProbabilities { get; set; } = new Dictionary<ImgChannel, double> 
    {
        { ImgChannel.Red, 0.0 },
        { ImgChannel.Green, 0.0 },
        { ImgChannel.Blue, 0.0 }
    };


    public SpaResult() { }
}
