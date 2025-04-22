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
    /// Усреднённый оценённый относительный объём встроенных данных
    /// </summary>
    public double MessageRelativeVolume { get; set; } = 0.0;

    /// <summary>
    /// Оценённые относительные объёмы встроенных данных
    /// </summary>
    public Dictionary<ImgChannel, double> MessageRelativeVolumesByChannels { get; set; } = new Dictionary<ImgChannel, double>
    {
        { ImgChannel.Red, 0.0 },
        { ImgChannel.Green, 0.0 },
        { ImgChannel.Blue, 0.0 }
    };

    /// <summary>
    /// Обнаружено ли скрытие методом SPA
    /// </summary>
    public bool IsHidingDetected { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public SpaResult() { }
}
