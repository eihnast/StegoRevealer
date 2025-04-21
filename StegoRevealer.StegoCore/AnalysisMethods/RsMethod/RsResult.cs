using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Результаты метода стегоанализа Regular-Singular
/// </summary>
public class RsResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// Усреднённый относительный объём сообщения
    /// </summary>
    public double MessageRelativeVolume { get; set; }

    /// <summary>
    /// Относительные объёмы сообщения по каналам
    /// </summary>
    public Dictionary<ImgChannel, double> MessageRelativeVolumesByChannels { get; set; } = new Dictionary<ImgChannel, double>()
    {
        { ImgChannel.Red, 0.0 },
        { ImgChannel.Green, 0.0 },
        { ImgChannel.Blue, 0.0 }
    };


    public RsResult() { }
}
