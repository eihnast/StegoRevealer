using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;

/// <summary>
/// Результаты метода стегоанализа ZCA
/// </summary>
public class ZcaResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    public Dictionary<ImgChannel, bool> IsHidedByChannels { get; set; } = new Dictionary<ImgChannel, bool>()
    {
        { ImgChannel.Red, false },
        { ImgChannel.Green, false },
        { ImgChannel.Blue, false }
    };

    /// <summary>
    /// Обнаружено ли скрытие методом ZCA
    /// </summary>
    public bool IsHidingDetected { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public ZcaResult() { }
}
