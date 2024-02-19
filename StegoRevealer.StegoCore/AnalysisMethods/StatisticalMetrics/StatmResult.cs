using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Результаты оценки статистических показателей
/// </summary>
public class StatmResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }

    // Результаты

    /// <summary>
    /// Интенсивность шума
    /// </summary>
    public double NoiseValue { get; set; }

    public StatmResult() { }
}
