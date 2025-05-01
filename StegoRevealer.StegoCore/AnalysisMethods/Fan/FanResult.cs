using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.Fan;

/// <summary>
/// Результаты стегоанализа метода анализа аддитивного шума
/// </summary>
public class FanResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// Массив вычисленнных центров масс
    /// </summary>
    public IEnumerable<double> ComsList { get; set; } = new List<double>();

    /// <summary>
    /// Обнаружено ли скрытие методом SPA
    /// </summary>
    public bool IsHidingDetected { get; set; } = false;

    /// <summary>
    /// Вычисленное расстояние Махаланобиса
    /// </summary>
    public double? MahalanobisDistance { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public FanResult() { }
}
