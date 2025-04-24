using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.Logger;
using System.Text.Json.Serialization;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;

/// <summary>
/// Результаты стегоанализа по методу Хи-квадрат
/// </summary>
public class ChiSquareResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// Относительный объём сообщения
    /// </summary>
    public double MessageRelativeVolume { get; set; }

    /// <summary>
    /// Относительные объёмы сообщения по каналам
    /// </summary>
    public Dictionary<ImgChannel, double>? MessageRelativeVolumesByChannels { get; set; } = new Dictionary<ImgChannel, double>()
    {
        { ImgChannel.Red, 0.0 },
        { ImgChannel.Green, 0.0 },
        { ImgChannel.Blue, 0.0 }
    };

    /// <summary>
    /// Изображение (если визуализация включена)
    /// </summary>
    [JsonIgnore]
    public ImageHandler? Image { get; set; }
    
    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public ChiSquareResult() { }
}
