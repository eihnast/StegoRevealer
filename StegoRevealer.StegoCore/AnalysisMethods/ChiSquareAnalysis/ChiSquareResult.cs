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
    /// Размер скрытого сообщения
    /// </summary>
    public int MessageLength { get; set; }

    /// <summary>
    /// Относительный объём сообщения
    /// </summary>
    public double MessageRelativeVolume { get; set; }

    /// <summary>
    /// Изображение (если визуализация включена)
    /// </summary>
    [JsonIgnore]
    public ImageHandler? Image { get; set; }


    public ChiSquareResult() { }
}
