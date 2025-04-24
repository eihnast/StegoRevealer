using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.HcfCom;

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
    public bool IsHidingDetected { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public FanResult() { }
}
